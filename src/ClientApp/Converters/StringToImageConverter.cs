using System;
using System.Collections.Concurrent;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace eShop.ClientApp.Converters
{
    public sealed class StringToImageConverter : IValueConverter
    {
        public static readonly StringToImageConverter Instance = new();

        private static readonly ConcurrentDictionary<string, Bitmap?> Cache = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (Cache.TryGetValue(path, out var cached))
            {
                return cached;
            }

            string[] candidates = path.StartsWith("avares://", StringComparison.OrdinalIgnoreCase)
                ? new[] { path }
                : new[] { path, $"avares://ClientApp/Assets/{path}", $"avares://ClientApp/{path}" };

            Bitmap? bitmap = null;
            foreach (var candidate in candidates)
            {
                try
                {
                    using var stream = AssetLoader.Open(new Uri(candidate));
                    bitmap = new Bitmap(stream);
                    break;
                }
                catch
                {
                    // Try the next candidate URI.
                }
            }

            Cache[path] = bitmap;
            return bitmap;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
