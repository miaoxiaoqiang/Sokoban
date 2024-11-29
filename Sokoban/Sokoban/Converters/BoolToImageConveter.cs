using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Sokoban.Converters
{
    internal sealed class BoolToImageConveter : IValueConverter
    {
        private static BoolToImageConveter _instance;
        public static BoolToImageConveter Instance => _instance ??= new BoolToImageConveter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            bool cleared = System.Convert.ToBoolean(value);
            string @param = parameter.ToString();

            if (@param == "Visibility")
            {
                return cleared ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (@param == "Big")
            {
                return cleared ? Visibility.Visible : Visibility.Hidden;
            }
            else if (@param == "Small")
            {
                return cleared ? Visibility.Hidden : Visibility.Visible;
            }
            else
            {
                if (cleared)
                {
                    return (BitmapImage)Application.Current.Resources["StageUnLock"];
                }
                else
                {
                    return (BitmapImage)Application.Current.Resources["StageLock"];
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
