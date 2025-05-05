using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HotelDBLibrary;
using HotelDBLibrary.Models;
using SystemTasks = System.Threading.Tasks;
using Resepsjon.Views;
using Microsoft.Maui.Controls;

namespace Resepsjon.ViewModels;

public class SignUpViewModel : INotifyPropertyChanged
{
    private string _newUsername = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    private readonly EmployeeManager _employeeManager;

    public string NewUsername
    {
        get => _newUsername;
        set { _newUsername = value; OnPropertyChanged(); }
    }
    public string NewPassword
    {
        get => _newPassword;
        set { _newPassword = value; OnPropertyChanged(); }
    }
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { _confirmPassword = value; OnPropertyChanged(); }
    }
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }
     public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); (SignUpCommand as Command)?.ChangeCanExecute(); }
    }

    public ICommand SignUpCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public SignUpViewModel(EmployeeManager employeeManager)
    {
        _employeeManager = employeeManager;
        SignUpCommand = new Command(async () => await RegisterUser(), CanSignUp);
        GoToLoginCommand = new Command(async () => await GoToLogin());
    }

    private bool CanSignUp()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(NewUsername) && !string.IsNullOrWhiteSpace(NewPassword) && NewPassword == ConfirmPassword;
    }

    private async SystemTasks.Task RegisterUser()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
        {
            StatusMessage = "Username and password must be filled out.";
            return;
        }
        if (NewPassword != ConfirmPassword)
        {
            StatusMessage = "Passwords do not match.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Registering user...";

        try
        {
            var newEmployee = new Employee
            {
                Username = this.NewUsername,
                Password = this.NewPassword
            };

            var addedEmployee = await _employeeManager.AddEmployeeAsync(newEmployee);
            StatusMessage = $"User '{addedEmployee.Username}' registered! You can now log in.";

            NewUsername = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;

            await SystemTasks.Task.Delay(1500);
            await GoToLogin();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Registration failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"SignUp Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async SystemTasks.Task GoToLogin()
    {
        try
        {
            if (Application.Current?.MainPage?.Navigation != null && Application.Current.MainPage.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            else if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
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

        if (propertyName == nameof(NewUsername) ||
            propertyName == nameof(NewPassword) ||
            propertyName == nameof(ConfirmPassword) ||
            propertyName == nameof(IsBusy))
        {
            (SignUpCommand as Command)?.ChangeCanExecute();
        }
    }
}