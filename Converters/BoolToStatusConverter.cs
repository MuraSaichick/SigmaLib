// BoolToStatusConverter.cs
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;


namespace SigmaLib.Converters;
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBlocked)
            {
                string param = parameter as string;

                if (param == "Color")
                {
                    return isBlocked ? Brushes.Red : Brushes.Green;
                }
                else if (param == "Icon")
                {
                    return isBlocked ? "🔓" : "🔒";
                }
                else if (param == "Tooltip")
                {
                    return isBlocked ? "Разблокировать" : "Заблокировать";
                }
                else
                {
                    return isBlocked ? "Заблокирован" : "Активен";
                }
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }