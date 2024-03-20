using PriceMonitoringApp.ViewModel;

namespace PriceMonitoringApp.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}