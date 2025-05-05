using Resepsjon.ViewModels;

namespace Resepsjon.Views;

public partial class RoomManagementPage : ContentPage
{
    public RoomManagementPage(RoomManagementViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RoomManagementViewModel vm && vm.LoadRoomsCommand.CanExecute(null))
        {
            vm.LoadRoomsCommand.Execute(null);
        }
    }
}