using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotLiquid.Util
{
    /// <summary>
    /// ExtensionMethods for converting to numeric values.
    /// </summary>
    public static class NumericConverter
    {
        private static bool IsReal(object o) => o is double || o is float || o is decimal;
        private static bool IsInteger(object o) => o is int || o is uint || o is long || o is ulong || o is short || o is ushort || o is byte || o is sbyte;
        private static bool IsNumeric(object o) => IsReal(o) || IsInteger(o);

        /// <summary>
        /// Coerce an object into a numeric type.
        /// </summary>
        /// <param name="value">The string to coerce.</param>
        /// <param name="formatProvider">The format provider for converting floating point numbers.</param>
        /// <param name="defaultValue">The value to return if coercion fails.</param>
        /// <returns>The coerced value as int, long, double or decimal type, or <paramref name="defaultValue"/> if coercion fails.</returns>
        public static object CoerceToNumericType(this object value, IFormatProvider formatProvider, object defaultValue)
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
        /// Try to parse an object into a numeric type.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="formatProvider">The format provider for converting floating point numbers.</param>
        /// <param name="convertedValue">The coerced value as int, long, double or decimal type, or null if parsing fails.</param>
        /// <returns>true if parsing was successful; Otherwise, false.</returns>
        public static bool TryParseToNumericType(this object value, IFormatProvider formatProvider, out object convertedValue)
        {
            if (value != null)
            {
                if (IsNumeric(value))
                {
                    convertedValue = value;
                    return true;
                }
                else if (value is string stringValue)
                {
                    return stringValue.TryParseToNumericType(formatProvider, out convertedValue);
                }
            }

            convertedValue = null;
            return false;
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
            if (value != null)
            {
                if (int.TryParse(value, NumberStyles.Integer | NumberStyles.AllowThousands, formatProvider, out int intValue))
                {
                    convertedValue = intValue;
                    return true;
                }
                else if (long.TryParse(value, NumberStyles.Integer | NumberStyles.AllowThousands, formatProvider, out long longValue))
                {
                    convertedValue = longValue;
                    return true;
                }
                else if (decimal.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, formatProvider, out decimal decimalValue))
                {
                    convertedValue = decimalValue;
                    return true;
                }
                else if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, formatProvider, out double doubleValue))
                {
                    convertedValue = doubleValue;
                    return true;
                }
                else if (formatProvider != CultureInfo.InvariantCulture)
                {
                    // Fall back to Invariant FormatProvider
                    if (int.TryParse(value, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out intValue))
                    {
                        convertedValue = intValue;
                        return true;
                    }
                    else if (long.TryParse(value, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out longValue))
                    {
                        convertedValue = longValue;
                        return true;
                    }
                    else if (decimal.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out decimalValue))
                    {
                        convertedValue = decimalValue;
                        return true;
                    }
                    else if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out doubleValue))
                    {
                        convertedValue = doubleValue;
                        return true;
                    }
                }
            }

            convertedValue = null;
            return false;
        }
    }
}
