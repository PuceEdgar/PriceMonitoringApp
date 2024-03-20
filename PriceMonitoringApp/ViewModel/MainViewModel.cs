using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceMonitoringApp.Views;
using PriceMonitoringLibrary.Models;
using PriceMonitoringLibrary.Services;
using System.Collections.ObjectModel;

namespace PriceMonitoringApp.ViewModel;

public partial class MainViewModel : ObservableObject
{
    public IRelayCommand LoadCommand { get; set; }

    private readonly Page? _mainPage = Application.Current!.MainPage;

    [ObservableProperty]
    ObservableCollection<MonitoredItem> items = [];

    [ObservableProperty]
    string? url;

    [ObservableProperty]
    string? availableSizeText;

    public MainViewModel()
    {
        LoadCommand = new AsyncRelayCommand(async () =>
        {
            Items = new ObservableCollection<MonitoredItem>(await FileService.GetSavedItemData());
        });
    }

    [RelayCommand]
    async Task AddItem()
    {
        await AddItemFromUrl();
    }

    [RelayCommand]
    async Task DeleteItem(object sender)
    {
        if (sender is not MonitoredItem itemToDelete)
        {
            return;
        }

        var isDeleteConfirmed = await _mainPage!.DisplayAlert("Deleting item", $"This item will be deleted\n {itemToDelete?.Brand}\n {itemToDelete?.Description}", "Delete", "Cancel");
        if (isDeleteConfirmed && Items.Remove(itemToDelete!))
        {
            await FileService.SaveToFile(Items);
        }
    }

    [RelayCommand]
    async Task Tap(object sender)
    {
        await Shell.Current.GoToAsync(nameof(DetailPage), true, new Dictionary<string, object>() { { "Item", sender} });
    }

    [RelayCommand]
    async Task GotToSettings(object sender)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    private async Task AddItemFromUrl()
    {
        if (string.IsNullOrWhiteSpace(Url) || !Uri.IsWellFormedUriString(Url, UriKind.Absolute) || !Url.Contains(Constants.MobileHanstyleDomainName, StringComparison.CurrentCultureIgnoreCase))
        {
            await _mainPage!.DisplayAlert("Adding new item", "Link to item was not provided, is in wrong format or not from Hanstyle mobile app. Please provide link again.", "OK");
            return;
        }
       
        var productCode = UriService.GetProductCodeValueFromUri(Url);

        if (Items.Any(mi => mi.ProductCode == productCode))
        {
            await _mainPage!.DisplayAlert("Adding new item", "Item already exists in the item list.", "OK");
            return;
        }

        var item = await DataScraperService.GetItemFromUrl(Url);

        if (item is null)
        {
            await _mainPage!.DisplayAlert("Adding new item", "Failed to load item from link. Please check link and try again.", "OK");
            return;
        }

        Items.Add(item);

        await FileService.SaveToFile(Items);
        Url = null;
    }
}
