using MainCore.UI.Models.Output;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPFUI.Converter
{
    [ValueConversion(typeof(TribeItem), typeof(CroppedBitmap))]
    public class TribeItemToCroppedBitmap : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TribeItem item) return null;
            var path = item.ImageSource;

            if (path is null) return null;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            var rect = new Int32Rect(
                TribeItem.ImageMask.X,
                TribeItem.ImageMask.Y,
                TribeItem.ImageMask.Width,
                TribeItem.ImageMask.Height);
            return new CroppedBitmap(bitmap, rect);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}