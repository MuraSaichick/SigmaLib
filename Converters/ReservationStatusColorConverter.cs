using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
namespace SigmaLib.Converters
{
    public class ReservationStatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Pending" => Brushes.Orange,   // Ожидание
                    "Accepted" => Brushes.Green,    // Принят
                    "Rejected" => Brushes.Red,      // Отклонён
                    "Returned" => Brushes.Blue,     // Возвращён
                    _ => Brushes.Gray      // Неизвестный
                };
            }
            return Brushes.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
