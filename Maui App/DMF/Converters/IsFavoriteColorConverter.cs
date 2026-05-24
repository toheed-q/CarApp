using System.Globalization;

namespace DMF.Converters
{
    public class IsFavoriteColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                return isFavorite ? "DmfRed" : "White";
            }
            return "White";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
