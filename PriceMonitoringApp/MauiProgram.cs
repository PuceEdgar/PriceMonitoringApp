using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Plugin.LocalNotification;

namespace PriceMonitoringApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
                .UseMauiCommunityToolkit();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<MainPage>();
#if ANDROID
            builder.Services.AddTransient<IPriceCheckerService, PriceCheckerService>();
#endif
            return builder.Build();
        }
    }
}