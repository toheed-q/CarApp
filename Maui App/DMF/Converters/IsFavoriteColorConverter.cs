using System.Globalization;

namespace DMF.Converters
{
    public class IsFavoriteColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Return real Color values (a FontImageSource.Color can't resolve a
            // resource-key string like "DmfRed").
            if (value is bool isFavorite)
                return isFavorite ? Color.FromArgb("#CA2F49") : Colors.White;

            return Colors.White;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
