using System;
using System.Globalization;
using System.Windows.Data;
using FactorioHelper.Enums;

namespace FactorioHelper.Converters
{
    internal class ModuleImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageHelper.GetBitmapImage("Modules", ((ModuleType)value).ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
