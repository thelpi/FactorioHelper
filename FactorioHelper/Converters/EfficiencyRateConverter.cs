using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FactorioHelper.Converters
{
    public class EfficiencyRateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rate = (Fraction)value;
            var rate255 = rate.Decimal * 255;
            return new SolidColorBrush(Color.FromRgb((byte)(255 - rate255), (byte)rate255, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
