namespace Resepsjon;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        Services = serviceProvider;
        MainPage = new AppShell();
    }

    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<HotelDBLibrary.EmployeeManager>();
    }
}