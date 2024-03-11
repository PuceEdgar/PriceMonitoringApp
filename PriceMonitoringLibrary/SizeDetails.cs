namespace PriceMonitoringLibrary;

public class SizeDetails : PropertyChangedNotifier
{
    private string? _availability;

    public SizeDetails(string size, string availability)
    {
        Size = size;
        Availability = availability;
    }    

    public string? Size { get; set; }
    public string? Availability {
        get
        {
            return _availability;
        }
        set
        {
            _availability = value;
            RaisePropertyChanged(nameof(_availability));
        }
    }
}
