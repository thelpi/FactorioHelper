using System;
using System.Windows.Media.Imaging;

namespace FactorioHelper
{
    static class ImageHelper
    {
        internal static BitmapImage GetBitmapImage(string subFolder, int value)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(string.Format(@"{0}\{1}\{2}.png",
                Properties.Settings.Default.picturesPath,
                subFolder,
                value));
            image.EndInit();
            return image;
        }
    }
}
