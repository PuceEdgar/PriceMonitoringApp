using Android.App;
using Android.Content;
using Android.OS;

namespace PriceMonitoringApp;

[Service]
internal class ForegroundServiceNotification : Service
{
    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        // Create and display the foreground notification
        NotificationChannel channel = new NotificationChannel("ServiceChannel", "PriceService", NotificationImportance.Max);
        NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(NotificationService);
        manager.CreateNotificationChannel(channel);

        var notification = new Notification.Builder(this, "ServiceChannel")
            .SetContentTitle("Foreground Service")
            .SetContentText("Running")
            .Build();

        StartForeground(100, notification);

        return StartCommandResult.NotSticky;
    }
}
