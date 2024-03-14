using PriceMonitoringApp.ViewModel;

namespace PriceMonitoringApp;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
