using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceMonitoringLibrary.Models;
using System.Text;

namespace PriceMonitoringApp.ViewModel;

[QueryProperty("Item", "Item")]
public partial class DetailViewModel : ObservableObject
{
    [ObservableProperty]
    MonitoredItem item;

    [ObservableProperty]
    string? availableSizes;

    [ObservableProperty]
    string? allSizes;

    [RelayCommand]
    async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    async Task ShowHistory()
    {
        var message = new StringBuilder();

        Item.PriceHistory?.ForEach(h => message.AppendLine($"{h.Date} - {h.Price}"));
        await Application.Current.MainPage.DisplayAlert("Price History", message.ToString(), "Close");
    }

    partial void OnItemChanged(MonitoredItem? oldValue, MonitoredItem newValue)
    {
        if (newValue is not null)
        {
            AvailableSizes = ConcatSizeDetails(newValue.AvailableSizes);
            AllSizes = ConcatSizeDetails(newValue.AllSizes);
        }
    }

    private static string ConcatSizeDetails(List<SizeDetails>? item)
    {
        if (item is null)
        {
            return string.Empty;
        }
        return string.Join("", item.Select(s => $"\n{s.Size}: [ {s.Availability} ]"));
    }
}
