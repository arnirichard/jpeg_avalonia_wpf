using DAW.FilterDesign;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DAW.Converters
{
    internal class FilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(v => v == DependencyProperty.UnsetValue) || values.Length != 2)
                return Visibility.Collapsed;

            string str = (string)values[0];
            string filter= (string)values[1];

            if (string.IsNullOrWhiteSpace(filter) || str.ToLower().Contains(filter.Trim().ToLower()))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
