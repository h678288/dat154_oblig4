using Resepsjon.ViewModels;

namespace Resepsjon.Views;

public partial class RegisterTaskPage : ContentPage
{
    public RegisterTaskPage(RegisterTaskViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RegisterTaskViewModel vm)
        {
            vm.LoadTasksCommand?.Execute(null);
        }
    }
}