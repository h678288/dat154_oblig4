using Resepsjon.ViewModels;

namespace Resepsjon.Views;

public partial class ReservationPage : ContentPage
{
    public ReservationPage(ReservationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ReservationViewModel vm)
        {
            vm.FindReservationsCommand?.Execute(null);
        }
    }
}