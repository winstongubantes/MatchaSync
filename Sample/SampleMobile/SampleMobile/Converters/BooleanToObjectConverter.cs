using System;
using System.Globalization;
using Xamarin.Forms;

namespace SampleMobile.Converters
{
    public class BooleanToObjectConverter<T> : IValueConverter
    {
        public T FalseObject { set; get; }

        public T TrueObject { set; get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? this.TrueObject : this.FalseObject;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((T) value).Equals(this.TrueObject);
        }
    }
}
