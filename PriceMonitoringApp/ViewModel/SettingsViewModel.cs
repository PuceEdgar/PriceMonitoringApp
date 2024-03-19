using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceMonitoringLibrary.Services;

namespace PriceMonitoringApp.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    readonly IPriceCheckerService PriceCheckerService;

    public SettingsViewModel(IPriceCheckerService priceCheckerService)
    {
        PriceCheckerService = priceCheckerService;
        var isRunning = ForegroundServiceHelper.IsForegroundServiceRunning();
        SetTextValuesForService(isRunning);
        Frequency = Preferences.Get(Constants.FrequencyKey, 6);
    }

    [ObservableProperty]
    string? serviceButtonText;

    [ObservableProperty]
    string? serviceStatus;

    [ObservableProperty]
    int frequency;

    [RelayCommand]
    void ToggleService()
    {
        //TODO
        //start service on app start up, but if there are no items saved, don't start service
        var isRunning = PriceCheckerService.ToggleService();
        SetTextValuesForService(isRunning);
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
    void SetFrequency()
    {
        Preferences.Set(Constants.FrequencyKey, Frequency);
    }

    [RelayCommand]
    static async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private void SetTextValuesForService(bool isRunning)
    {
        ServiceButtonText = isRunning ? Constants.StopService : Constants.StartService;
        ServiceStatus = isRunning ? Constants.ServiceRunning : Constants.ServiceStopped;
    }
}
