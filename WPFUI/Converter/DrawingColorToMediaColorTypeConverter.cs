using Splat;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFUI.Converter
{
    [ValueConversion(typeof(SplatColor), typeof(Color))]
    public class SplatColorToMediaColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SplatColor color) return null;

            return color.ToNative();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Color color) return null;

            return SplatColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}