using PriceMonitoringApp.ViewModel;

namespace PriceMonitoringApp;

public partial class DetailPage : ContentPage
{
	public DetailPage(DetailViewModel detailViewModel)
	{
		InitializeComponent();
		BindingContext = detailViewModel;
	}
}