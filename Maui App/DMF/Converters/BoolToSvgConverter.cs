using System.Globalization;

namespace DMF.Converters
{
    public class BoolToSvgConverter : IValueConverter
    {
        public string TrueIcon { get; set; } = string.Empty;
        public string FalseIcon { get; set; } = string.Empty;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return TrueIcon;

            return FalseIcon;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
