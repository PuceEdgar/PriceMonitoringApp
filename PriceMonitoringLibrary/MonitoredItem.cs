namespace PriceMonitoringLibrary;

public class MonitoredItem : PropertyChangedNotifier
{
    private string? _price;
    private bool _isSoldOut;
    private string? _previousPrice;

    public string? DateItemAdded { get; set; } = DateTime.Now.ToShortDateString();
    public string? Brand { get; set; }
    public string? Descripton { get; set; }
    public string? InitialProductPrice { get; set; }
    public string? Price { 
        get
        {
            return _price;
        }
        set
        {
            _price = value;
            RaisePropertyChanged(nameof(Price));
        }
    }
    public string? PreviousPrice {
        get
        {
            return _previousPrice;
        }
        set
        {
            _previousPrice = value;
            RaisePropertyChanged(nameof(PreviousPrice));
        }
    }
    public List<HistoryDetails>? PriceHistory { get; set; } = [];
    public string? OriginalPrice { get; set; }
    public string? DiscountPercent { get; set; }
    public List<SizeDetails>? AllSizes { get; set; }
    public List<SizeDetails>? AvailableSizes {  get; set; }
    public string? AvailableSizesAsString {  get; set; }
    public string? ImageUrl { get; set; }
    public bool IsSoldOut
    {
        get
        {
            return _isSoldOut;
        }
        set
        {
            _isSoldOut = value;
            RaisePropertyChanged(nameof(_isSoldOut));
        }
    }
    public string? ProductUrl { get; set; }
    public string? ShareUrl { get; set; }
}