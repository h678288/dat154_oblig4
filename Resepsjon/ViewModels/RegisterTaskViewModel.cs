using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using HotelDBLibrary;
using HotelDBLibrary.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using SystemTasks = System.Threading.Tasks;
using System;
using Resepsjon.Views;
using Resepsjon.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Resepsjon.ViewModels;

public class RegisterTaskViewModel : INotifyPropertyChanged
{
    private HotelDBLibrary.Models.Task _newTask;
    private string _statusMessage = string.Empty;
    private bool _isRefreshing;
    private ObservableCollection<HotelDBLibrary.Models.Task> _existingTasks;

    private readonly TaskManager _taskManager;
    private readonly RoomManager _roomManager;

    public HotelDBLibrary.Models.Task NewTask
    {
        get => _newTask;
        set { _newTask = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set { _isRefreshing = value; OnPropertyChanged(); }
    }

    public ObservableCollection<HotelDBLibrary.Models.Task> ExistingTasks
    {
        get => _existingTasks;
        set { _existingTasks = value; OnPropertyChanged(); }
    }

    public List<string> AvailableStatuses => new List<string>
    {
        TaskManager.StatusPending,
        TaskManager.StatusInProgress,
        TaskManager.StatusCompleted
    };

    public List<string> AvailableTypes => new List<string>
    {
        TaskManager.TypeCleaning,
        TaskManager.TypeMaintenance,
        TaskManager.TypeService
    };

    public ICommand RegisterTaskCommand { get; }
    public ICommand LoadTasksCommand { get; }
    public ICommand GoBackToDashboardCommand { get; }

    public RegisterTaskViewModel(TaskManager taskManager, RoomManager roomManager)
    {
        _taskManager = taskManager;
        _roomManager = roomManager;
        _newTask = new HotelDBLibrary.Models.Task { Status = TaskManager.StatusPending };
        _existingTasks = new ObservableCollection<HotelDBLibrary.Models.Task>();
        RegisterTaskCommand = new Command(async () => await RegisterTaskAsync());
        LoadTasksCommand = new Command(async () => await LoadTasksAsync());
        GoBackToDashboardCommand = new Command(async () => await GoBackToDashboardAsync());
        SystemTasks.Task.Run(LoadTasksAsync);
    }

    private async SystemTasks.Task<bool> RegisterTaskAsync()
    {
        if (NewTask.RoomId <= 0 || string.IsNullOrWhiteSpace(NewTask.Task1) || string.IsNullOrWhiteSpace(NewTask.Status)) {
            StatusMessage = "Room ID, Task Description, and Status are required.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(NewTask.Task1))
        {
             StatusMessage = "Task Type is required.";
             return false;
        }

        var roomExists = await _roomManager.GetRoomByIdAsync(NewTask.RoomId);
        if (roomExists == null) {
             StatusMessage = $"Room with ID {NewTask.RoomId} does not exist.";
             return false;
        }

        StatusMessage = "Registering task...";
        try
        {
            var addedTask = await _taskManager.AddTaskAsync(NewTask);
            StatusMessage = $"Task {addedTask.Id} registered for room {addedTask.RoomId}.";
            NewTask = new HotelDBLibrary.Models.Task { Status = TaskManager.StatusPending };
            await LoadTasksAsync();
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during registration: {ex.Message}";
            Console.WriteLine($"[RegisterTaskAsync Error] {ex}");
            return false;
        }
    }

    private async SystemTasks.Task LoadTasksAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        StatusMessage = "Loading tasks...";
        try
        {
            var tasks = await _taskManager.GetAllTasksAsync();
            ExistingTasks.Clear();
            if (tasks != null)
            {
                foreach (var task in tasks.OrderByDescending(t => t.Id))
                {
                    ExistingTasks.Add(task);
                }
            }
            StatusMessage = $"Loaded {ExistingTasks.Count} tasks.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load tasks: {ex.Message}";
            Console.WriteLine($"[LoadTasksAsync Error] {ex}");
        }
        finally {
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

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}