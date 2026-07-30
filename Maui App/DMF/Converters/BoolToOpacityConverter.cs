using System.Globalization;

namespace DMF.Converters
{
    // true -> fully opaque, false -> dimmed. Used to visually disable the Model
    // dropdown until a Brand is chosen.
    public class BoolToOpacityConverter : IValueConverter
    {
        public double EnabledOpacity { get; set; } = 1.0;
        public double DisabledOpacity { get; set; } = 0.4;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? EnabledOpacity : DisabledOpacity;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
