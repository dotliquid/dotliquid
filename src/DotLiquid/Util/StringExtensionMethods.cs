using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotLiquid.Util
{
    public static class StringExtensionMethods
    {
        private static readonly Regex IntegerRegex = R.C(R.Q(@"^([+-]?\d+)$"));
        private static readonly Regex NumericRegex = R.C(R.Q(@"^([+-]?\d[\d\.|\,]+)$"));

        public static bool TryParseToNumericType(this string value, IFormatProvider formatProvider, out object convertedValue)
        {
            if (value == null)
            {
                convertedValue = null;
                return false;
            }

            // Integer.
            Match match = IntegerRegex.Match(value);
            if (match.Success)
            {
                try
                {
                    convertedValue = Convert.ToInt32(match.Groups[1].Value);
                    return true;
                }
                catch (OverflowException)
                {
                    try
                    {
                        convertedValue = Convert.ToInt64(match.Groups[1].Value);
                        return true;
                    }
                    catch (OverflowException)
                    {
                        convertedValue = Convert.ToDecimal(match.Groups[1].Value);
                        return true;
                    }
                }
            }

            // Floating point numbers.
            match = NumericRegex.Match(value);
            if (match.Success)
            {
                // For cultures with "," as the decimal separator, allow
                // both "," and "." to be used as the separator.
                // First try to parse using current culture.
                // If that fails, try to parse using invariant culture.
                // Also, first try higher precision decimal.
                // If that fails, try to parse as double (precision float).
                // Double is less precise but has a larger range.
                if (decimal.TryParse(match.Groups[1].Value, NumberStyles.Number | NumberStyles.Float, formatProvider, out decimal parsedDecimalCurrentCulture))
                {
                    convertedValue = parsedDecimalCurrentCulture;
                    return true;
                }
                if (decimal.TryParse(match.Groups[1].Value, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsedDecimalInvariantCulture))
                {
                    convertedValue = parsedDecimalInvariantCulture;
                    return true;
                }
                if (double.TryParse(match.Groups[1].Value, NumberStyles.Number | NumberStyles.Float, formatProvider, out double parsedDouble))
                {
                    convertedValue = parsedDouble;
                    return true;
                }
                convertedValue = double.Parse(match.Groups[1].Value, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture);
                return true;
            }

            convertedValue = null;
            return false;
        }
    }
}
