using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Blasig.Labirint.GUI
{
    class TimeLeftConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                TimeSpan timeLeft = (TimeSpan)value;
                if (timeLeft == new TimeSpan(0, 0, 0))
                    return 1; 
                else if (timeLeft <= new TimeSpan(0, 10, 0))
                    return 2;
                else return 3;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
