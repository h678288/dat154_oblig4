using Resepsjon.Views;
namespace Resepsjon;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
		Routing.RegisterRoute(nameof(FrontDeskDashboardPage), typeof(FrontDeskDashboardPage));
		Routing.RegisterRoute(nameof(ReservationPage), typeof(ReservationPage));
		Routing.RegisterRoute(nameof(RoomManagementPage), typeof(RoomManagementPage));
		Routing.RegisterRoute(nameof(RegisterTaskPage), typeof(RegisterTaskPage));
		Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
	}
}
