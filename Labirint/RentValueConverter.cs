using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Blasig.Labirint.DataModel;

namespace Blasig.Labirint.GUI
{
    class RentValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visitor visitor = value as Visitor;
            if (visitor != null)
                return "Арендованные номерки";
            else
                return "Свободные номерки";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
