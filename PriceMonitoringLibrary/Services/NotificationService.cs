using Plugin.LocalNotification;

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
}
