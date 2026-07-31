using System.Globalization;

namespace DMF.Converters
{
    /// <summary>
    /// Resolves a stored ProfileImage value to an image source: a real uploaded
    /// photo (http/https URL) is used directly; anything else (null, "default.png",
    /// legacy values) falls back to the simple generic user silhouette.
    /// </summary>
    public class ProfileImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var url = value as string;
            return !string.IsNullOrWhiteSpace(url) &&
                   url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? url
                : "profile";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
