using Android.App;
using Android.Content;
using Application = Android.App.Application;

namespace PriceMonitoringApp.ForegroundService;

public static class ForegroundServiceUtility
{
    public static bool IsForegroundServiceRunning()
    {
        var context = Application.Context;
        var manager = (ActivityManager)context.GetSystemService(Context.ActivityService);

        foreach (var service in manager.GetRunningServices(int.MaxValue))
        {
            if (service.Service.ClassName.Contains("ForegroundServiceNotification"))
            {
                return true;
            }
        }
        return false;
    }
}
