using System.Globalization;

namespace DMF.Converters
{
    public class TabEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var valueString = value.ToString();
            var parameterString = parameter.ToString();

            if (valueString == null || parameterString == null)
                return false;

            return valueString.Equals(parameterString, StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
