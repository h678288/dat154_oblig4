using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Resepsjon.Views;
using HotelDBLibrary.Models;
using Resepsjon.Services;
using SystemTasks = System.Threading.Tasks;

namespace Resepsjon.ViewModels;

public class FrontDeskDashboardViewModel : INotifyPropertyChanged
{
    private string _welcomeMessage;
    private string _dashboardInfo;

    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set { _welcomeMessage = value; OnPropertyChanged(); }
    }

    public string DashboardInfo
    {
        get => _dashboardInfo;
        set { _dashboardInfo = value; OnPropertyChanged(); }
    }

    public ICommand GoToReservationsCommand { get; }
    public ICommand GoToRoomsCommand { get; }
    public ICommand GoToTasksCommand { get; }
    public ICommand LoadDashboardDataCommand { get; }

    public FrontDeskDashboardViewModel()
    {

        WelcomeMessage = "Welcome, Receptionist!";
        GoToReservationsCommand = new Command(async () => await GoToPage(nameof(ReservationPage)));
        GoToRoomsCommand = new Command(async () => await GoToPage(nameof(RoomManagementPage)));
        GoToTasksCommand = new Command(async () => await GoToPage(nameof(RegisterTaskPage)));
        LoadDashboardDataCommand = new Command(LoadData);

        LoadData();
    }

    private async SystemTasks.Task GoToPage(string pageName)
    {
        try
        {
            var navigation = Application.Current?.MainPage?.Navigation;
            if (navigation == null)
            {
                Console.WriteLine("Navigasjonsfeil: Navigation context not found.");
                return;
            }

            Page targetPage = null;
            switch (pageName)
            {
                case nameof(ReservationPage):
                    targetPage = App.Services.GetService<ReservationPage>();
                    break;
                case nameof(RoomManagementPage):
                    targetPage = App.Services.GetService<RoomManagementPage>();
                    break;
                case nameof(RegisterTaskPage):
                    targetPage = App.Services.GetService<RegisterTaskPage>();
                    break;
                default:
                    Console.WriteLine($"Navigasjonsfeil: Unknown page name '{pageName}'.");
                    break;
            }

            if (targetPage != null)
            {
                await navigation.PushAsync(targetPage);
            }
            else
            {
                Console.WriteLine($"Navigasjonsfeil: Could not resolve page '{pageName}'. Ensure it's registered in MauiProgram.cs.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Navigasjonsfeil ({pageName}): {ex}");
        }
    }

    private void LoadData()
    {

        DashboardInfo = $"Systemstatus OK. Klokken er {DateTime.Now:HH:mm}.";
        OnPropertyChanged(nameof(DashboardInfo));
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}