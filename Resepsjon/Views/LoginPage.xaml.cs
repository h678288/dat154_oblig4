namespace Resepsjon.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            var employeeManager = App.Services.GetService<HotelDBLibrary.EmployeeManager>();
            BindingContext = new ViewModels.LoginViewModel(employeeManager);
        }
    }
}