using PriceMonitoringLibrary.Enums;
using System.Globalization;

namespace PriceMonitoringApp.Converters;

internal class PriceEnumToBackgroundColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return Colors.Transparent;
        }

        var color = (CheaperPrice)value switch
        {
            CheaperPrice.Yes => Colors.LightCyan,
            CheaperPrice.No => Colors.LightPink,
            CheaperPrice.Same => Colors.Transparent,
            _ => Colors.Transparent,
        };

        return color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
