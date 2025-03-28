using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotLiquid.Util
{
    /// <summary>
    /// ExtensionMethods for Strings.
    /// </summary>
    public static class StringExtensionMethods
    {
        private static readonly Regex IntegerRegex = R.C(R.Q(@"^([+-]?\d+)$"));
        private static readonly Regex NumericRegex = R.C(R.Q(@"^([+-]?\d[\d\.|\,]+)$"));

        /// <summary>
        /// Coerce the string into a numeric type.
        /// </summary>
        /// <param name="value">The string to coerce.</param>
        /// <param name="formatProvider">The format provider for converting floating point numbers.</param>
        /// <param name="defaultValue">The value to return if coercion fails.</param>
        /// <returns>The coerced value as int, long, double or decimal type, or <paramref name="defaultValue"/> if coercion fails.</returns>
        public static object CoerceToNumericType(this string value, IFormatProvider formatProvider, object defaultValue)
        {
            object result = defaultValue;
            if (value != null)
            {
                bool converted = value.TryParseToNumericType(formatProvider, out object convertedValue);
                if (converted)
                {
                    result = convertedValue;
                }
            }
            return result;
        }

        /// <summary>
        /// Try to parse the string into a numeric type.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="formatProvider">The format provider for converting floating point numbers.</param>
        /// <param name="convertedValue">The coerced value as int, long, double or decimal type, or null if parsing fails.</param>
        /// <returns>true if parsing was successful; Otherwise, false.</returns>
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
