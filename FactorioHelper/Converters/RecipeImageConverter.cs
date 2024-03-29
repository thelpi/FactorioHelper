﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace FactorioHelper.Converters
{
    internal class RecipeImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageHelper.GetBitmapImage("Recipes", (int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
