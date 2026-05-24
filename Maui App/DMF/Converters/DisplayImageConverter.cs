using System.Globalization;

namespace DMF.Converters
{
    public class DisplayImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is List<string> images && images.Count > 0)
            {
                return images[0];
            }
            return null;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
