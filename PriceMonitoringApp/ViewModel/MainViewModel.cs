using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceMonitoringLibrary;
using System.Collections.ObjectModel;

namespace PriceMonitoringApp.ViewModel;

public partial class MainViewModel : ObservableObject
{
    public IRelayCommand LoadCommand { get; set; }

    [ObservableProperty]
    ObservableCollection<MonitoredItem> items;

    [ObservableProperty]
    bool itemExistsIsVisible;

    [ObservableProperty]
    bool failedToLoadItem;

    [ObservableProperty]
    string? url;

    public MainViewModel()
    {
        LoadCommand = new AsyncRelayCommand(async () =>
        {
            Items ??= new ObservableCollection<MonitoredItem>(await FileHelper.GetSavedItemData());
            
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

        var isDeleteConfirmed = await Application.Current.MainPage.DisplayAlert("Deleting item", $"This item will be deleted\n {itemToDelete?.Brand}\n {itemToDelete?.Descripton}", "Delete", "Cancel");
        if (isDeleteConfirmed && Items.Remove(itemToDelete!))
        {
            await FileHelper.SaveToFile(Items);
        }
    }

    [RelayCommand]
    async Task Tap(object sender)
    {
        await Shell.Current.GoToAsync(nameof(DetailPage), true, new Dictionary<string, object>() { { "Item", sender} });
    }

    private async Task AddItemFromUrl()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            await Application.Current.MainPage.DisplayAlert("Adding new item", "Link to item was not provided. Please provide link again.", "OK");
            return;
        }
        if (Items.Any(mi => mi.ShareUrl == Url))
        {
            ItemExistsIsVisible = true;
            return;
        }

        var item = await DataScraper.GetItemFromUrl(Url);

        if (item is null)
        {
            FailedToLoadItem = true;
            return;
        }
        item.ShareUrl = Url;

        Items.Add(item);

        await FileHelper.SaveToFile(Items);
        Url = null;
    }
}
