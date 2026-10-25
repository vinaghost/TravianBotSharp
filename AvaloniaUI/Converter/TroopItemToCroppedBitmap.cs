using System.Globalization;

using Avalonia.Data.Converters;

using MainCore.UI.Models.Output;

namespace AvaloniaUI.Converter
{
    public class TroopItemToCroppedBitmap : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not TroopItem item) return null;
            var path = item.ImageSource;

            if (path is null) return null;

            return ResourceImages.Crop(path, item.ImageMask);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
