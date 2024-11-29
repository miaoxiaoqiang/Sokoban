using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sokoban.Converters
{
    internal sealed class DifficultyToCNConveter : IValueConverter
    {
        private static DifficultyToCNConveter _instance;
        public static DifficultyToCNConveter Instance => _instance ??= new DifficultyToCNConveter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            string difflevel = value.ToString();
            if (!System.Text.RegularExpressions.Regex.IsMatch(difflevel, @"^([1-9]|1[0-2])$", System.Text.RegularExpressions.RegexOptions.Compiled))
            {
                return DependencyProperty.UnsetValue;
            }

            return difflevel switch
            {
                "1" => "垃圾",
                "2" => "小case",
                "3" => "忽悠我",
                "4" => "隔靴搔痒",
                "5" => "打屁股针",
                "6" => "难受",
                "7" => "好痛哦",
                "8" => "分娩啦",
                "9" => "如喝臭水沟",
                "10" => "比登天还难",
                "11" => "神阿救我",
                "12" => "上帝不存在",
                _ => DependencyProperty.UnsetValue,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
