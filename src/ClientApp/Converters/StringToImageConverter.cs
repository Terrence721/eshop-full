using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace eShop.ClientApp.Converters
{
    public sealed class StringToImageConverter : IValueConverter
    {
        public static readonly StringToImageConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            string[] candidates = path.StartsWith("avares://", StringComparison.OrdinalIgnoreCase)
                ? new[] { path }
                : new[] { path, $"avares://ClientApp/Assets/{path}", $"avares://ClientApp/{path}" };

            foreach (var candidate in candidates)
            {
                try
                {
                    using var stream = AssetLoader.Open(new Uri(candidate));
                    return new Bitmap(stream);
                }
                catch
                {
                    // Try the next candidate URI.
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
