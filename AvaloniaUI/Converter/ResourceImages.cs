using System.Collections.Concurrent;

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AvaloniaUI.Converter
{
    /// <summary>
    /// MainCore hands out WPF pack URIs ("pack://application:,,,/Resources/roman.png").
    /// The same files are embedded here as Avalonia resources, so only the file name is used.
    /// </summary>
    internal static class ResourceImages
    {
        private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();

        public static Bitmap Load(string source)
        {
            var fileName = Path.GetFileName(source);
            return _cache.GetOrAdd(fileName, name =>
            {
                using var stream = AssetLoader.Open(new Uri($"avares://TravianBotSharp/Resources/{name}"));
                return new Bitmap(stream);
            });
        }

        public static CroppedBitmap Crop(string source, System.Drawing.Rectangle mask)
        {
            return new CroppedBitmap(Load(source), new PixelRect(mask.X, mask.Y, mask.Width, mask.Height));
        }
    }
}
