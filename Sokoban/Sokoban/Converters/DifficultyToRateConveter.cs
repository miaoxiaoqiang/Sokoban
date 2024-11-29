using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Sokoban.Converters
{
    internal sealed class DifficultyToRateConveter : IValueConverter
    {
        private static DifficultyToRateConveter _instance;
        public static DifficultyToRateConveter Instance => _instance ??= new DifficultyToRateConveter();

        private static readonly IDictionary<byte, BitmapImage> _pathDict;

        static DifficultyToRateConveter()
        {
            _pathDict = new Dictionary<byte, BitmapImage>
            {
                { 0, new BitmapImage(new Uri("pack://application:,,,/resources/Images/rate_empty.png", UriKind.RelativeOrAbsolute)) },
                { 1, new BitmapImage(new Uri("pack://application:,,,/resources/Images/rate_half.png", UriKind.RelativeOrAbsolute)) },
                { 2, new BitmapImage(new Uri("pack://application:,,,/resources/Images/rate_full.png", UriKind.RelativeOrAbsolute)) }
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return DependencyProperty.UnsetValue;
            }

            byte number = System.Convert.ToByte(value);
            byte @params = System.Convert.ToByte(parameter.ToString());
            byte round = System.Convert.ToByte(number / 2);
            byte mod = System.Convert.ToByte(number % 2);

            if (@params >= 1 && @params <= 6)
            {
                if (round >= @params)
                {
                    return _pathDict[2];
                }
                else
                {
                    if (round + 1 == @params && mod != 0)
                    {
                        return _pathDict[System.Convert.ToByte(number % 2)];
                    }
                    else
                    {
                        return _pathDict[0];
                    }
                }
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
