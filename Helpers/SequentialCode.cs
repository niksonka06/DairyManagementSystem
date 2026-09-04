using System.Globalization;
using System.Text.RegularExpressions;

namespace DairyManagementSystem.Helpers
{
    public static class SequentialCode
    {
        public const string FarmerPrefix = "FRM";
        public const string SocietyPrefix = "SOC";
        public const string OperatorPrefix = "OPR";

        private static readonly Regex TrailingDigits = new(@"(\d+)$", RegexOptions.CultureInvariant);

        public static string Next(string prefix, IEnumerable<string> existingCodes, int digits = 3, bool prefixOrNumericOnly = false)
        {
            var max = 0;
            foreach (var code in existingCodes)
            {
                if (TryReadNumber(code, prefix, prefixOrNumericOnly, out var n))
                {
                    max = Math.Max(max, n);
                }
            }

            return prefix + (max + 1).ToString($"D{digits}", CultureInfo.InvariantCulture);
        }

        private static bool TryReadNumber(string? code, string prefix, bool prefixOrNumericOnly, out int number)
        {
            number = 0;
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var compact = code.Trim().Replace(" ", "", StringComparison.Ordinal);

            if (int.TryParse(compact, NumberStyles.None, CultureInfo.InvariantCulture, out number) && number > 0)
            {
                return true;
            }

            if (compact.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var rest = compact[prefix.Length..];
                return int.TryParse(rest, NumberStyles.None, CultureInfo.InvariantCulture, out number) && number > 0;
            }

            if (prefixOrNumericOnly)
            {
                return false;
            }

            var match = TrailingDigits.Match(compact);
            if (!match.Success)
            {
                return false;
            }

            return int.TryParse(match.Value, NumberStyles.None, CultureInfo.InvariantCulture, out number) && number > 0;
        }
    }
}
