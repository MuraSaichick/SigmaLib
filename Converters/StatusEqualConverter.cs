using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Converters
{
    public class StatusEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is string status && parameter is string expectedStatus))
            {
                bool result = status == expectedStatus;

                if (expectedStatus == "Returned" && status == "Accepted")
                    result = true;

                if (expectedStatus == "Returned" && status == "Returned")
                    result = false;

                return result;
            }
            if(value is string Status)
            {
                return !(Status == "Pending");
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
