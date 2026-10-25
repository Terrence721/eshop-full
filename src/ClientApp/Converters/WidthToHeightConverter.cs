using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace eShop.ClientApp.Converters
{
    // Turns a (typically bound) width into a proportional height for an image whose
    // aspect ratio is supplied via ConverterParameter ("width,height" or "ratio").
    public sealed class WidthToHeightConverter : IValueConverter
    {
        public static readonly WidthToHeightConverter Instance = new();

        public double DefaultAspectRatio { get; init; } = 2060d / 1757d;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not double width || double.IsNaN(width) || width <= 0)
            {
                return null;
            }

            double ratio = DefaultAspectRatio;

            if (parameter is string text && !string.IsNullOrWhiteSpace(text))
            {
                text = text.Trim();
                if (text.Contains(',') && text.Split(',') is { Length: 2 } parts
                    && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var h)
                    && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var w)
                    && w > 0)
                {
                    ratio = h / w;
                }
                else if (text.Contains('/') && text.Split('/') is { Length: 2 } frac
                         && double.TryParse(frac[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var n)
                         && double.TryParse(frac[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                         && d > 0)
                {
                    ratio = n / d;
                }
                else if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var r) && r > 0)
                {
                    ratio = r;
                }
            }
            else if (parameter is double r && r > 0)
            {
                ratio = r;
            }

            return width * ratio;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
