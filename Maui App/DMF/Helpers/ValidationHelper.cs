using System.Text.RegularExpressions;

namespace DMF.Helpers
{
    public static class ValidationHelper
    {
        // Pragmatic email check: local@domain.tld with no spaces. Good enough to
        // reject the obviously-invalid input the form used to accept, without the
        // false negatives of an over-strict RFC regex.
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public static bool IsValidEmail(string? email) =>
            !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());

        // A place name (city/town): starts with a letter, then letters, spaces and
        // basic punctuation only — rejects digits and stray symbols.
        private static readonly Regex PlaceRegex = new(
            @"^[A-Za-z][A-Za-z\s.\-']{1,49}$", RegexOptions.Compiled);

        public static bool IsValidPlaceName(string? name) =>
            !string.IsNullOrWhiteSpace(name) && PlaceRegex.IsMatch(name.Trim());
    }
}
