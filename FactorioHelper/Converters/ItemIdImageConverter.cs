using System;
using System.Globalization;
using System.Windows.Data;

namespace FactorioHelper.Converters
{
    class ItemIdImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageHelper.GetBitmapImage("items", (int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
