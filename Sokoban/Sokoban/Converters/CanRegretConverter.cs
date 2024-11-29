using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sokoban.Converters
{
    internal sealed class CanRegretConverter : IValueConverter
    {
        private static CanRegretConverter _instance;
        public static CanRegretConverter Instance => _instance ??= new CanRegretConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            int steps = System.Convert.ToInt32(value);

            return steps > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
