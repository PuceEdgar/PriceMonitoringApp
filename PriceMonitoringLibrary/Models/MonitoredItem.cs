using CommunityToolkit.Mvvm.ComponentModel;

namespace PriceMonitoringLibrary.Models;

public partial class MonitoredItem : ObservableObject
{
    private string? _price;
    private bool _isSoldOut;
    private string? _previousPrice;

    public string? DateItemAdded { get; set; } = DateTime.Now.ToShortDateString();

    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string? InitialProductPrice { get; set; }
    public string? Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public string? PreviousPrice
    {
        get => _previousPrice;
        set => SetProperty(ref _previousPrice, value);
    }

    public List<HistoryDetails>? PriceHistory { get; set; } = [];
    public string? OriginalPrice { get; set; }
    public string? DiscountPercent { get; set; }
    public List<SizeDetails>? AllSizes { get; set; }
    public List<SizeDetails>? AvailableSizes { get; set; }
    public string? AvailableSizesAsString { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsSoldOut
    {
        get => _isSoldOut;
        set => SetProperty(ref _isSoldOut, value);
    }
    public string? ProductUrl { get; set; }
    public string? ShareUrl { get; set; }
    public string? ProductCode { get; set; }
}