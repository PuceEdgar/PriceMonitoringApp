using PriceMonitoringLibrary.Enums;
using System.Globalization;

namespace PriceMonitoringApp.Converters;

internal class PriceEnumToStrokeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return Colors.Grey;
        }

        var color = (CheaperPrice)value switch
        {
            CheaperPrice.Yes => Colors.Green,
            CheaperPrice.No => Colors.Red,
            CheaperPrice.Same => Colors.Grey,
            _ => Colors.Grey,
        };

        return color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
