using System.Globalization;

namespace PriceMonitoringApp;

internal class ValueToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return Colors.Black;
        }

        var color = (string)value switch
        {
            string a when a.Equals(Constants.LastItem, StringComparison.CurrentCultureIgnoreCase) => Colors.Red,
            string a when a.Equals(Constants.LessThanFive, StringComparison.CurrentCultureIgnoreCase) => Colors.Orange,
            string a when a.Equals(Constants.Many, StringComparison.CurrentCultureIgnoreCase) => Colors.Green,
            _ => Colors.Grey,
        };

        return color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
