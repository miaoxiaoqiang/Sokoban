using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Sokoban.Converters
{
    internal sealed class NullToIntConveter : IValueConverter
    {
        private static NullToIntConveter _instance;
        public static NullToIntConveter Instance => _instance ??= new NullToIntConveter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "--";
            }
            
            string _value = value.ToString();
            if (Regex.IsMatch(_value, "^(?!0)([1-9]\\d*)$"))
            {
                return _value;
            }
            return "--";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "--";
        }
    }
}
