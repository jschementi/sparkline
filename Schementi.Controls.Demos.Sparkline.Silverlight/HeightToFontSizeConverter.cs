using System;
using System.Globalization;
using System.Windows;
using SystemConvert = System.Convert;
using System.Windows.Data;

namespace Schementi.Controls.Demos.Sparkline.Silverlight {
    public class HeightToFontSizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var intValue = SystemConvert.ToDouble(value);
            if (intValue <= 0) return DependencyProperty.UnsetValue;
            return intValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
