using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HotelDBLibrary;
using HotelDBLibrary.Models;
using Resepsjon.Views;
using SystemTasks = System.Threading.Tasks;

namespace Resepsjon.ViewModels;

public class LoginViewModel : INotifyPropertyChanged {

    private string _username;
    private string _password;
    private string _statusMessage;
    private readonly EmployeeManager _employeeManager;

    public string Username {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string Password {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public string StatusMessage {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }
    public Command GoToSignUpCommand { get; }

    public LoginViewModel(EmployeeManager employeeManager)
    {
        _employeeManager = employeeManager;

        LoginCommand = new Command(ExecuteLogin);

        GoToSignUpCommand = new Command(async () =>
        {
            try
            {
                var signUpViewModel = App.Services.GetService<SignUpViewModel>();

                await Application.Current.MainPage.Navigation.PushAsync(new SignUpPage(signUpViewModel));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Navigation error: {ex.Message}";
                Console.WriteLine($"Navigation error: {ex}");
            }
        });
    }

    private void ExecuteLogin()
    {
        _ = LoginAsync();
    }

    private async SystemTasks.Task LoginAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            StatusMessage = "Username and password must be filled out";
            return;
        }

        try
        {
            if (_employeeManager == null)
            {
                StatusMessage = "Login failed: Could not connect to database";
                return;
            }

            var employee = await _employeeManager.AuthenticateEmployee(Username, Password);

            if (employee != null)
            {
                StatusMessage = "Login successful!";

                await Shell.Current.GoToAsync(nameof(FrontDeskDashboardPage));
            }
            else
            {
                StatusMessage = "Login failed: Invalid username or password";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Login failed: {ex.Message}";
            Console.WriteLine($"Login error: {ex}");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}