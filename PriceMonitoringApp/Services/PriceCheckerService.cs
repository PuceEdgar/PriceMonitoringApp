using Android.App;
using Android.Content;
using PriceMonitoringApp.ForegroundService;
using PriceMonitoringLibrary.Services;
using Application = Android.App.Application;

namespace PriceMonitoringApp.Services;

[Service]
public class PriceCheckerService : IPriceCheckerService
{
    private readonly int _frequency = Preferences.Get(Constants.FrequencyKey, 6);
    private bool _isRunning;

    public bool ToggleService()
    {
        if (_isRunning)
        {
            StopForegroundService();
        }
        else
        {
            StartForegroundService();
        }
        return _isRunning;
    }

    private void StartForegroundService()
    {
        if (!_isRunning)
        {
            _isRunning = true;

            // Start the foreground service
            var intent = new Intent(Application.Context, typeof(ForegroundServiceNotification));
            Application.Context.StartService(intent);
            Console.WriteLine("Service started!");
            // Perform HTTP call every N hours
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    try
                    {
                        var detailshanged = await DataScraperService.CheckIfItemDetailsHaveChanged();
                        if (detailshanged)
                        {
                            await NotificationService.ShowGeneralChangeNotification();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking price and availability: {ex.Message}");
                    }

                    var intervalInMilliseconds = _frequency * 60 * 60 * 1000;
                    // Wait for the specified interval
                    await Task.Delay(intervalInMilliseconds);
                }
            });
        }
    }

    private void StopForegroundService()
    {
        _isRunning = false;
        // Stop the foreground service
        var intent = new Intent(Application.Context, typeof(ForegroundServiceNotification));
        Application.Context.StopService(intent);
        Console.WriteLine("Service stopped!");
    }
}
