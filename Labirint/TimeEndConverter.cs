using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Blasig.Labirint.GUI
{
    class TimeEndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(values[0] is DateTime && values[1] is int)
            {
                DateTime enterTime = (DateTime)values[0];
                int paidTime = (int)values[1];
                return enterTime.AddMinutes(paidTime);
            }
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
