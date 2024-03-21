using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceMonitoringApp.ForegroundService;
using PriceMonitoringApp.Services;
using PriceMonitoringLibrary.Models;
using PriceMonitoringLibrary.Services;
using System.Collections.ObjectModel;

namespace PriceMonitoringApp.ViewModel;

[QueryProperty("Items", "Items")]
public partial class SettingsViewModel : ObservableObject
{
    readonly IPriceCheckerService PriceCheckerService;
    private readonly Page? _mainPage = Application.Current!.MainPage;
    private bool _isRunning;    

    public SettingsViewModel(IPriceCheckerService priceCheckerService)
    {
        PriceCheckerService = priceCheckerService;
        _isRunning = ForegroundServiceUtility.IsForegroundServiceRunning();
        SetTextValuesForService();
        Frequency = Preferences.Get(Constants.FrequencyKey, 6);
    }

    [ObservableProperty]
    ObservableCollection<MonitoredItem> items = [];

    [ObservableProperty]
    string? serviceButtonText;

    [ObservableProperty]
    string? serviceStatus;

    [ObservableProperty]
    int frequency;

    [RelayCommand]
    async Task ToggleService()
    {
        if (Items!.Count > 0 || _isRunning)
        {
            _isRunning = PriceCheckerService.ToggleService();
            SetTextValuesForService();
        } else
        {
            await _mainPage!.DisplayAlert("Toggle service action", "You don't have items in your list to start service", "OK");
        }        
    }

    [RelayCommand]
    static async Task RefreshData()
    {
        var hasSomethingChanged = await DataScraperService.CheckIfItemDetailsHaveChanged();

        if (hasSomethingChanged)
        {
            await NotificationService.ShowGeneralChangeNotification();
        }
    }

    [RelayCommand]
    async Task Focused(object entry)
    {
        var e = (Entry)entry;
        var value = await _mainPage!.DisplayPromptAsync("Frequency", "How often in hours to check for changes?\n Enter number from 1 to 24. ", maxLength:2, keyboard: Keyboard.Numeric);
        if (value != null && int.TryParse(value, out int result) && result > 0 && result < 25)
        {
            Preferences.Set(Constants.FrequencyKey, result);
            Frequency = result;
        }
        e.Unfocus();
    }

    [RelayCommand]
    static async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private void SetTextValuesForService()
    {
        ServiceButtonText = _isRunning ? Constants.StopService : Constants.StartService;
        ServiceStatus = _isRunning ? Constants.ServiceRunning : Constants.ServiceStopped;
    }
}
