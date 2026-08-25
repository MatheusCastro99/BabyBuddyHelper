using BabyBuddyHelper.Models;
using System.Globalization;

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
