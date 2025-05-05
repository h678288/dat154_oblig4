using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HotelDBLibrary;
using HotelDBLibrary.Models;
using System.Collections.ObjectModel;
using SystemTasks = System.Threading.Tasks;
using Resepsjon.Views;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
using System.Linq;

namespace Resepsjon.ViewModels;

public class RoomManagementViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Room> _rooms;
    private string _statusMessage = string.Empty;
    private bool _isRefreshing;
    private Room _newRoom = new();

    private readonly RoomManager _roomManager;

    public ObservableCollection<Room> Rooms
    {
        get => _rooms;
        set { _rooms = value; OnPropertyChanged(); }
    }
    public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
    public bool IsRefreshing { get => _isRefreshing; set { _isRefreshing = value; OnPropertyChanged(); } }

    public Room NewRoom
    {
        get => _newRoom;
        set { _newRoom = value; OnPropertyChanged(); }
    }

    public ICommand LoadRoomsCommand { get; }
    public ICommand GoBackToDashboardCommand { get; }
    public ICommand ChangeStatusCommand { get; }
    public ICommand AddRoomCommand { get; }

    public RoomManagementViewModel(RoomManager roomManager)
    {
        _roomManager = roomManager;
        Rooms = new ObservableCollection<Room>();
        NewRoom = new Room();

        LoadRoomsCommand = new Command(async () => await LoadRooms());
        GoBackToDashboardCommand = new Command(async () => await GoBackToDashboardAsync());
        ChangeStatusCommand = new Command<Room>(async (room) => await ChangeStatus(room));
        AddRoomCommand = new Command(async () => await AddRoomAsync());
    }

    private async SystemTasks.Task LoadRooms()
    {
        if (IsRefreshing) return;
        IsRefreshing = true;
        StatusMessage = "Loading rooms...";
        List<Room>? roomsList = null;

        try
        {
            roomsList = await _roomManager.GetAllRoomsAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Rooms.Clear();
                if (roomsList != null && roomsList.Any())
                {
                    foreach (var room in roomsList.OrderBy(r => r.Id))
                    {
                        Rooms.Add(room);
                    }
                    StatusMessage = $"Loaded {Rooms.Count} rooms.";
                }
                else
                {
                    StatusMessage = "No rooms found.";
                }
                IsRefreshing = false;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading rooms: {ex}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusMessage = $"Could not load rooms: {ex.Message}";
                IsRefreshing = false;
            });
        }
    }

    private async SystemTasks.Task ChangeStatus(Room? room)
    {
        if (room == null)
        {
            StatusMessage = "No room selected for status change.";
            return;
        }

        string? currentTaskStatus = null;
        if (room.Tasks != null && room.Tasks.Any())
        {
            currentTaskStatus = room.Tasks.First().Status;
        }

        string? newStatus = await MainThread.InvokeOnMainThreadAsync(async () =>
            await Application.Current!.MainPage!.DisplayPromptAsync(
                "Change Status", $"New status for room {room.Id}:", "OK", "Cancel",
                placeholder: "E.g. Needs Cleaning", initialValue: currentTaskStatus ?? "")
        );

        if (!string.IsNullOrWhiteSpace(newStatus))
        {
            StatusMessage = $"Updating status for room {room.Id}...";
            IsRefreshing = true;
            try
            {
                bool success = await _roomManager.UpdateRoomStatusAsync(room.Id, newStatus);
                if(success)
                {
                    StatusMessage = $"Status for room {room.Id} updated.";
                    await LoadRooms();
                }
                else
                {
                    StatusMessage = "Could not update status (room not found or no tasks?).";
                    IsRefreshing = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error changing status: {ex}");
                StatusMessage = $"Error updating status: {ex.Message}";
                IsRefreshing = false;
            }
        }
        else
        {
            StatusMessage = "Status change cancelled.";
        }
    }

    private async SystemTasks.Task AddRoomAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRoom.Type)) {
            StatusMessage = "Room type is required."; return;
        }
        if (NewRoom.Type.Length > 1) {
            StatusMessage = "Room type can only be one character (e.g., S, D)."; return;
        }
        if (NewRoom.NumOfBeds == null || NewRoom.NumOfBeds <= 0) {
            StatusMessage = "Number of beds must be a positive number."; return;
        }

        StatusMessage = "Adding new room...";
        IsRefreshing = true;
        try
        {
            var roomToAdd = new Room { Type = NewRoom.Type, NumOfBeds = NewRoom.NumOfBeds };

            var addedRoom = await _roomManager.AddRoomAsync(roomToAdd);

            StatusMessage = $"Room {addedRoom.Id} ({addedRoom.Type}, {addedRoom.NumOfBeds} beds) added.";
            NewRoom = new Room();
            await LoadRooms();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding room: {ex}");
            StatusMessage = $"Could not add room: {ex.Message}";
            IsRefreshing = false;
        }
    }

    private async SystemTasks.Task GoBackToDashboardAsync()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(FrontDeskDashboardPage));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Navigation error: {ex.Message}";
            Console.WriteLine($"Navigation error: {ex}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}