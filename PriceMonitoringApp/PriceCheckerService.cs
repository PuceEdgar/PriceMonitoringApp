using Android.App;
using Android.Content;
using PriceMonitoringLibrary;
using Application = Android.App.Application;

namespace PriceMonitoringApp;

[Service]
public class PriceCheckerService : IPriceCheckerService
{
    private const int IntervalMilliseconds = 3 * 60 * 60 * 1000; // 3 hours
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
            // Perform HTTP call every 3 hours
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    try
                    {
                        var detailshanged = await DataScraper.CheckIfItemDetailsHaveCHanged();
                        if (detailshanged)
                        {
                            await NotificationService.ShowGeneralChangeNotification();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking price and availability: {ex.Message}");
                    }

                    // Wait for the specified interval
                    await Task.Delay(IntervalMilliseconds);
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
