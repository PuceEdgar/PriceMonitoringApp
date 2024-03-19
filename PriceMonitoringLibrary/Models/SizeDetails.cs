using CommunityToolkit.Mvvm.ComponentModel;

namespace PriceMonitoringLibrary.Models;

public class SizeDetails : ObservableObject
{
    private string? _availability;

    public SizeDetails(string size, string availability)
    {
        Size = size;
        Availability = availability;
    }

    public string? Size { get; set; }
    public string? Availability
    {
        get => _availability;
        set => SetProperty(ref _availability, value);
    }
}
