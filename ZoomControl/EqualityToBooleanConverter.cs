﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Denxorz.ZoomControl
{
    public class EqualityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return parameter;
            }

            return Binding.DoNothing;
        }
    }
}