using CommunityToolkit.Maui.Views;
using PriceMonitoringLibrary;
using System.Text;
namespace PriceMonitoringApp;

public partial class ItemDetailsContentView : Popup
{
    private readonly MonitoredItem _monitoredItem;
    public ItemDetailsContentView(MonitoredItem item)
    {
        InitializeComponent();
        _monitoredItem = item;
        DateItemAdded.Text = $"Date item was added: {item.DateItemAdded}";
        InitialPrice.Text = $"Initial product price: {item.InitialProductPrice}";
        Brand.Text = item.Brand;
        Description.Text = item.Descripton;
        ImageURL.Source = item.ImageUrl;
        OriginalPrice.Text = $"Original price: {item.OriginalPrice}";
        Discount.Text = $"Discount: {item.DiscountPercent}";
        SalePrice.Text = $"Current price: {item.Price}";
        PreviousPrice.Text = $"Previous price: {item.PreviousPrice}";
        AvailableSizes.Text = $"\nAvailable sizes:\n {ConcatSizeDetails(item.AvailableSizes)}";
        AllSizes.Text = $"\nAll sizes:\n {ConcatSizeDetails(item.AllSizes)}";
    }

    private void ClosePopup_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    private string ConcatSizeDetails(List<SizeDetails> item)
    {
        return string.Join("", item.Select(s => $"\n{s.Size}: [ {s.Availability} ]"));
    }

    private async void HistoryButton_Clicked(object sender, EventArgs e)
    {        
        var message = new StringBuilder();

        _monitoredItem.PriceHistory.ForEach(h => message.AppendLine($"{h.Date} - {h.Price}"));
        await Application.Current.MainPage.DisplayAlert("Price History", message.ToString(), "Close");        
    }
}