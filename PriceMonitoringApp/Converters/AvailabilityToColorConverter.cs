using System.Globalization;

namespace PriceMonitoringApp.Converters;

internal class AvailabilityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return Colors.Black;
        }

        var color = (string)value switch
        {
            string a when a.Equals(Constants.LastItem, StringComparison.CurrentCultureIgnoreCase) || (int.TryParse(a, out int q) && q == 1) => Colors.Red,
            string a when a.Equals(Constants.LessThanFive, StringComparison.CurrentCultureIgnoreCase) || (int.TryParse(a, out int q) && q == 2) => Colors.Orange,
            string a when a.Equals(Constants.Many, StringComparison.CurrentCultureIgnoreCase) || (int.TryParse(a, out int q) && q > 2) => Colors.Green,
            _ => Colors.Grey,
        };

        return color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
