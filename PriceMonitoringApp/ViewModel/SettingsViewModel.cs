using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceMonitoringLibrary;

namespace PriceMonitoringApp.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    readonly IPriceCheckerService PriceCheckerService;

    public SettingsViewModel(IPriceCheckerService priceCheckerService)
    {
        PriceCheckerService = priceCheckerService;
        var isRunning = ForegroundServiceHelper.IsForegroundServiceRunning();
        SetTextValuesForService(isRunning);
        Frequency = Preferences.Get(Constants.FrequencyKey, 24);
    }

    [ObservableProperty]
    string serviceButtonText;

    [ObservableProperty]
    string serviceStatus;

    [ObservableProperty]
    int frequency;

    [RelayCommand]
    void ToggleService()
    {
        var isRunning = PriceCheckerService.ToggleService();
        SetTextValuesForService(isRunning);
    }

    [RelayCommand]
    async Task RefreshData()
    {
        var hasSomethingChanged = await DataScraper.CheckIfItemDetailsHaveCHanged();

        if (hasSomethingChanged)
        {
            await NotificationService.ShowGeneralChangeNotification();
        }
    }

    [RelayCommand]
    void SetFrequency()
    {
        Preferences.Set(Constants.FrequencyKey, Frequency);
    }

    private void SetTextValuesForService(bool isRunning)
    {
        ServiceButtonText = isRunning ? Constants.StopService : Constants.StartService;
        ServiceStatus = isRunning ? Constants.ServiceRunning : Constants.ServiceStopped;
    }
}
