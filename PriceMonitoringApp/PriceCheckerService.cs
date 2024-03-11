using Android.App;
using Android.Content;
using Plugin.LocalNotification;
using PriceMonitoringLibrary;
using System.Collections.ObjectModel;
using Application = Android.App.Application;

namespace PriceMonitoringApp;

[Service]
public class PriceCheckerService : IPriceCheckerService
{
    private const int ServiceId = 1001;
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
                        await OutputServiceStatus();
                        await CheckPriceAndAvailability();
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

    private async Task CheckPriceAndAvailability()
    {
        var detailsChanged = false;
        var monitoredItems = await FileHelper.GetSavedItemData();

        foreach (var item in monitoredItems)
        {
            //item.Price = "90000";
            var newItemData = await DataScraper.GetItemFromUrl(item.ShareUrl);
            if (item.Price != newItemData.Price)
            {
                detailsChanged = true;
                item.PreviousPrice = item.Price;
                item.PriceHistory.Add(new HistoryDetails { Price = item.Price });
                item.Price = newItemData.Price;
            }
            var result = item.AvailableSizes.Except(newItemData.AvailableSizes, new SizeDetailsComparer())?.ToList();
            if (result is not null)
            {
                detailsChanged = true;
                item.AvailableSizes = newItemData.AvailableSizes;
                // changedSizeInfo.AddRange(result);
            }
        }

        if (detailsChanged)
        {
            await FileHelper.SaveToFile(new ObservableCollection<MonitoredItem>(monitoredItems));
            ShowGeneralChangeNotification();
        }
    }

    private async void ShowGeneralChangeNotification()
    {
        var request = new NotificationRequest
        {
            Title = "Product details changed",
            Description = $"Item details have changed!"
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    private Task OutputServiceStatus()
    {
        return Task.Run(() => Console.WriteLine("Service is running!"));
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
