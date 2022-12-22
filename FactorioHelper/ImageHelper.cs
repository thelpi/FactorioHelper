using System;
using System.Windows.Media.Imaging;

namespace FactorioHelper
{
    internal static class ImageHelper
    {
        internal static BitmapImage GetBitmapImage(string subFolder, object value)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(string.Format(@"{0}/{1}/{2}.png",
                "Pictures",
                subFolder,
                value), UriKind.Relative);
            image.EndInit();
            return image;
        }
    }
}
