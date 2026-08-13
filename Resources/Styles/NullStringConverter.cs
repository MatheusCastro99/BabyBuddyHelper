using BabyBuddyHelper.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BabyBuddyHelper.Resources.Styles
{
    internal class IsAppointmentModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return value is AppointmentModel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
