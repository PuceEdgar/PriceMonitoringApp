using System.Globalization;

namespace PriceMonitoringApp.Converters;

internal class ServiceStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return Colors.Grey;
        }

        var color = (string)value switch
        {
            string a when a.Equals(Constants.ServiceRunning, StringComparison.CurrentCultureIgnoreCase) => Colors.Green,
            string a when a.Equals(Constants.ServiceStopped, StringComparison.CurrentCultureIgnoreCase) => Colors.Red,
            _ => Colors.Grey,
        };

        return color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
