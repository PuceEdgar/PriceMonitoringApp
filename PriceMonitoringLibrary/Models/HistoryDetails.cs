namespace PriceMonitoringLibrary.Models;

public class HistoryDetails
{
    public string? Price { get; set; }
    public string? Date { get; set; } = DateTime.Now.ToShortDateString();
}
