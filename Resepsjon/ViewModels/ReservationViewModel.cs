using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HotelDBLibrary;
using HotelDBLibrary.Models;
using System.Collections.ObjectModel;
using SystemTasks = System.Threading.Tasks;
using Resepsjon.Views;
using System.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace Resepsjon.ViewModels;

public class ReservationViewModel : INotifyPropertyChanged
{
    private Reservation _currentReservation;
    private string _statusMessage;
    private string _searchGuestTlf;
    private string _searchRoomId;
    private ObservableCollection<Reservation> _foundReservations;
    private bool _isRefreshing;

    private readonly ReservationManager _reservationManager;
    private readonly GuestManager _guestManager;
    private readonly RoomManager _roomManager;

    private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);

    public Reservation CurrentReservation { get => _currentReservation; set { _currentReservation = value; OnPropertyChanged(); } }
    public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
    public string SearchGuestTlf { get => _searchGuestTlf; set { _searchGuestTlf = value; OnPropertyChanged(); } }
    public string SearchRoomId { get => _searchRoomId; set { _searchRoomId = value; OnPropertyChanged(); } }
    public ObservableCollection<Reservation> FoundReservations { get => _foundReservations; set { _foundReservations = value; OnPropertyChanged(); } }
    public bool IsRefreshing { get => _isRefreshing; set { _isRefreshing = value; OnPropertyChanged(); } }

    public ICommand AddReservationCommand { get; }
    public ICommand FindReservationsCommand { get; }
    public ICommand UpdateReservationCommand { get; }
    public ICommand DeleteReservationCommand { get; }
    public ICommand GoBackToDashboardCommand { get; }
    public ICommand SelectReservationCommand { get; }


    public ReservationViewModel(ReservationManager reservationManager, GuestManager guestManager, RoomManager roomManager)
    {
        _reservationManager = reservationManager;
        _guestManager = guestManager;
        _roomManager = roomManager;

        CurrentReservation = new Reservation { Start = DateTime.Today, End = DateTime.Today.AddDays(1) };
        FoundReservations = new ObservableCollection<Reservation>();

        AddReservationCommand = new Command(async () => await ExecuteAddReservationAsync());
        FindReservationsCommand = new Command(async () => await ExecuteFindReservationsAsync());
        UpdateReservationCommand = new Command<Reservation>(async (res) => await ExecuteUpdateReservationAsync(res));
        DeleteReservationCommand = new Command<int>(async (id) => await ExecuteDeleteReservationAsync(id));
        SelectReservationCommand = new Command<Reservation>(SelectReservation);
        GoBackToDashboardCommand = new Command(async () => await ExecuteGoBackToDashboardAsync());
    }


    private async SystemTasks.Task ExecuteGoBackToDashboardAsync()
    {
        await GoBackToDashboardAsync();
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
    private void SelectReservation(Reservation? reservation)
    {
        if (reservation != null)
        {
            CurrentReservation = new Reservation
            {
                Id = reservation.Id,
                GuestId = reservation.GuestId,
                RoomId = reservation.RoomId,
                Start = reservation.Start,
                End = reservation.End,
                NumberOfGuests = reservation.NumberOfGuests,
                ReservationDate = reservation.ReservationDate
            };
        }
    }

    private async SystemTasks.Task ExecuteAddReservationAsync()
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            await AddReservation();
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private async SystemTasks.Task ExecuteFindReservationsAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        StatusMessage = "Searching for reservations...";
        OnPropertyChanged(nameof(IsRefreshing));

        await _dbSemaphore.WaitAsync();
        try
        {
            await FindReservations();
        }
        finally
        {
            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
            _dbSemaphore.Release();
        }
    }

    private async SystemTasks.Task ExecuteUpdateReservationAsync(Reservation? res)
    {
        if (res == null || res.Id <= 0)
        {
            StatusMessage = "Select a reservation to update.";
            return;
        }
        await _dbSemaphore.WaitAsync();
        try
        {
            await UpdateReservation(CurrentReservation);
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private async SystemTasks.Task ExecuteDeleteReservationAsync(int id)
    {
        if (id <= 0)
        {
            StatusMessage = "Select a reservation to delete.";
            return;
        }
        await _dbSemaphore.WaitAsync();
        try
        {
            await DeleteReservation(id);
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private async SystemTasks.Task AddReservation()
{
    if (string.IsNullOrWhiteSpace(CurrentReservation.GuestId) || CurrentReservation.RoomId <= 0)
    {
        StatusMessage = "Guest Phone/ID and Room ID are required.";
        OnPropertyChanged(nameof(StatusMessage));
        return;
    }

    Guest? guest = null;
    Room? room = null;

    try
    {
        guest = await _guestManager.GetGuestByTlfAsync(CurrentReservation.GuestId);

        if (guest == null)
        {
            StatusMessage = $"Guest {CurrentReservation.GuestId} not found. Attempting to create new guest...";
            OnPropertyChanged(nameof(StatusMessage));

            var newGuest = new Guest
            {
                Tlf = CurrentReservation.GuestId,
                Navn = null,
                Passord = null
            };

            try
            {
                guest = await _guestManager.AddGuestAsync(newGuest);
                StatusMessage = $"New guest {guest.Tlf} created successfully.";
                OnPropertyChanged(nameof(StatusMessage));
            }
            catch (Exception guestEx)
            {
                StatusMessage = $"Error creating new guest: {guestEx.Message}";
                OnPropertyChanged(nameof(StatusMessage));
                return;
            }
        }

        room = await _roomManager.GetRoomByIdAsync(CurrentReservation.RoomId.Value);
        if (room == null)
        {
            StatusMessage = $"Error: Room with ID {CurrentReservation.RoomId.Value} does not exist.";
            OnPropertyChanged(nameof(StatusMessage));
            return;
        }

        StatusMessage = "Adding reservation...";
        OnPropertyChanged(nameof(StatusMessage));

        if (CurrentReservation.ReservationDate == default)
        {
            CurrentReservation.ReservationDate = DateTime.Now;
        }

        CurrentReservation.GuestId = guest.Tlf;

        var addedReservation = await _reservationManager.AddReservationAsync(CurrentReservation);
        StatusMessage = $"Reservation {addedReservation.Id} added for guest {guest.Tlf}!";
        OnPropertyChanged(nameof(StatusMessage));

        CurrentReservation = new Reservation { Start = DateTime.Today, End = DateTime.Today.AddDays(1) };
        OnPropertyChanged(nameof(CurrentReservation));
        _ = SystemTasks.Task.Run(async () => await ExecuteFindReservationsAsync());

    }
    catch (Exception ex)
    {
        StatusMessage = $"Error during reservation process: {ex.Message}";
        OnPropertyChanged(nameof(StatusMessage));
    }
}


    private async SystemTasks.Task FindReservations()
    {
        try
        {
            List<Reservation>? results = null;
            int? roomId = null;
            if(!string.IsNullOrWhiteSpace(SearchRoomId) && int.TryParse(SearchRoomId, out int id)) { roomId = id; }

            if (!string.IsNullOrEmpty(SearchGuestTlf)) {
                results = await _reservationManager.GetReservationsByGuestIdAsync(SearchGuestTlf);
            } else if (roomId.HasValue) {
                results = await _reservationManager.GetReservationsByRoomIdAsync(roomId.Value);
            } else {
                results = await _reservationManager.GetAllReservationsAsync();
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                FoundReservations.Clear();
                if (results != null)
                {
                    foreach(var res in results.OrderByDescending(r => r.Start))
                    {
                        FoundReservations.Add(res);
                    }
                    StatusMessage = $"Found {results.Count} reservations.";
                } else {
                    StatusMessage = "No reservations found.";
                }
                OnPropertyChanged(nameof(FoundReservations));
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                StatusMessage = $"Error searching for reservations: {ex.Message}";
            });
        }
    }

    private async SystemTasks.Task UpdateReservation(Reservation? reservationToUpdate)
    {
        if (reservationToUpdate == null || reservationToUpdate.Id <= 0)
        {
            StatusMessage = "Cannot update reservation - Invalid data.";
            return;
        }
        StatusMessage = $"Updating reservation {reservationToUpdate.Id}...";
        try
        {
            var guestExists = await _guestManager.GetGuestByTlfAsync(reservationToUpdate.GuestId);
            if (guestExists == null) throw new Exception($"Guest {reservationToUpdate.GuestId} not found.");
            var roomExists = await _roomManager.GetRoomByIdAsync(reservationToUpdate.RoomId.Value);
            if (roomExists == null) throw new Exception($"Room {reservationToUpdate.RoomId} not found.");

            await _reservationManager.UpdateReservationAsync(reservationToUpdate);
            StatusMessage = "Reservation updated.";

            _ = SystemTasks.Task.Run(async () => await ExecuteFindReservationsAsync());
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating reservation: {ex.Message}";
        }
    }

    private async SystemTasks.Task DeleteReservation(int reservationId)
    {
        if (reservationId <= 0) return;
        StatusMessage = $"Deleting reservation {reservationId}...";
        try
        {
            bool success = await _reservationManager.DeleteReservationAsync(reservationId);
            StatusMessage = success ? "Reservation deleted." : "Could not delete reservation.";

            if (success && CurrentReservation.Id == reservationId)
            {
                CurrentReservation = new Reservation { Start = DateTime.Today, End = DateTime.Today.AddDays(1) };
            }

            _ = SystemTasks.Task.Run(async () => await ExecuteFindReservationsAsync());
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting reservation: {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}