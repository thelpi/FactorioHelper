using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FactorioHelper
{
    class ItemIdImageConverter : IValueConverter
    {
        private const string PicturePathFormat = @"C:\Users\LPI\Desktop\factorio\pictures\items\{0}.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(string.Format(PicturePathFormat, value));
            image.EndInit();
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
