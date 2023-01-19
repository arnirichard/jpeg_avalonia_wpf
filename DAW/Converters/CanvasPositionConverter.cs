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
    internal class CanvasPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //if (values.Any(v => v == DependencyProperty.UnsetValue))
            //    return 0;

            ZeroPole? zp = values[0] as ZeroPole;
            if (zp == null)
                return 0;
            //Console.WriteLine(value.ToString() + " " + parameter);
            //double width = (double)values[1];
            bool isReal = (string)parameter == "R";
            double value = isReal ? zp.Position.Real : zp.Position.Imaginary; 
            return 145 + (isReal ? 1 : -1) * value *125;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
