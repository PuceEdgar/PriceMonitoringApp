using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PriceMonitoringApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public static MainActivity ActivityCurrent { get; set; }
    public MainActivity()
    {
        ActivityCurrent = this;
    }
    //protected override void OnStart()
    //{
    //    base.OnStart();
    //    const int requestLocationId = 0;

    //    string[] notiPermission =
    //    {
    //        Manifest.Permission.PostNotifications
    //    };

    //    if ((int)Build.VERSION.SdkInt < 33) return;

    //    if (this.CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted)
    //    {
    //        this.RequestPermissions(notiPermission, requestLocationId);
    //    }
    //}
}
