using HotelDBLibrary;
using HotelDBLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Resepsjon.ViewModels;
using Resepsjon.Views;

namespace Resepsjon;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        string connectionString = "Server=tcp:dat154-hoteldb.database.windows.net,1433;Initial Catalog=HotelDB;Persist Security Info=False;User ID=dat154gr18;Password=tX4%YsKz6@DQhktM5TGf;Encrypt=false;";

        #if DEBUG
        builder.Services.AddDbContext<HotelDbContext>(options => options.UseSqlServer(connectionString),
            ServiceLifetime.Transient);
        #endif

        builder.Services.AddTransient<Func<HotelDbContext>>(sp => () =>
            sp.GetRequiredService<HotelDbContext>());

        builder.Services.AddSingleton<Func<HotelDbContext>>(sp => () =>
            sp.GetRequiredService<IDbContextFactory<HotelDbContext>>().CreateDbContext());

        builder.Services.AddTransient<EmployeeManager>();
        builder.Services.AddTransient<ReservationManager>();
        builder.Services.AddTransient<RoomManager>();
        builder.Services.AddTransient<TaskManager>();
        builder.Services.AddTransient<GuestManager>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<SignUpViewModel>();
        builder.Services.AddTransient<FrontDeskDashboardViewModel>();
        builder.Services.AddTransient<ReservationViewModel>();
        builder.Services.AddTransient<RoomManagementViewModel>();
        builder.Services.AddTransient<RegisterTaskViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<FrontDeskDashboardPage>();
        builder.Services.AddTransient<ReservationPage>();
        builder.Services.AddTransient<RoomManagementPage>();
        builder.Services.AddTransient<RegisterTaskPage>();


        return builder.Build();
    }
}