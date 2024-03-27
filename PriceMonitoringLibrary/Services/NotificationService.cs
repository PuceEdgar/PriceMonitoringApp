using Plugin.LocalNotification;
using PriceMonitoringLibrary.Models;

namespace PriceMonitoringLibrary.Services;

public static class NotificationService
{
    public static async Task ShowGeneralChangeNotification()
    {
        var request = new NotificationRequest
        {
            Title = "Product details changed",
            Description = $"Item details have changed!"
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public static async Task ShowGeneralErrorNotification(MonitoredItem item)
    {
        var request = new NotificationRequest
        {
            Title = "Product details changed",
            Description = $"Failed to get details for brand: {item.Brand} \ndescription: {item.Description}."
        };

        await LocalNotificationCenter.Current.Show(request);
    }
}
