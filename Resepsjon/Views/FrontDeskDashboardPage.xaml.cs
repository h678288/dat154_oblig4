using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resepsjon.ViewModels;

namespace Resepsjon.Views;

public partial class FrontDeskDashboardPage : ContentPage
{
    public FrontDeskDashboardPage()
    {
        InitializeComponent();
        BindingContext = App.Services.GetService<FrontDeskDashboardViewModel>();
    }
}