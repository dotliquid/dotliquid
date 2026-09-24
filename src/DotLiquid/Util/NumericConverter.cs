using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace DotLiquid.Util
{
    /// <summary>
    /// ExtensionMethods for converting to numeric values.
    /// </summary>
    public static class NumericConverter
    {
        /// <summary>
        /// Check if the object is a floating point (real) type.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is decimal, float or double; false otherwise.</returns>
        public static bool IsReal(object value) => value is double || value is float || value is decimal;

        /// <summary>
        /// Check if the object is an integer type.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is an integer; false otherwise.</returns>
        public static bool IsInteger(object value) =>
            value is int || value is uint || value is long || value is ulong ||
            value is short || value is ushort || value is byte || value is sbyte;

        /// <summary>
        /// Check if the object is a numeric type.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is an integer or floating point number; false otherwise.</returns>
        public static bool IsNumeric(object value) => IsReal(value) || IsInteger(value);

        /// <summary>
        /// A dictionary of numeric types and their allowable conversions.
        /// Based on the promotion table at
        /// https://docs.microsoft.com/dotnet/standard/base-types/conversion-tables
        /// </summary>
        public static readonly IReadOnlyDictionary<Type, Type[]> NumericTypePromotions = new ReadOnlyDictionary<Type, Type[]>(new Dictionary<Type, Type[]>
        {
            { typeof(Byte), new Type[] { typeof(UInt16), typeof(Int16), typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(SByte), new Type[] { typeof(Int16), typeof(Int32), typeof(Int64), typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(Int16), new Type[] { typeof(Int32), typeof(Int64), typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(UInt16), new Type[] { typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(Char), new Type[] { typeof(UInt16), typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(Int32), new Type[] { typeof(Int64), typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(UInt32), new Type[] { typeof(Int64), typeof(UInt64), typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(Int64), new Type[] { typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(UInt64), new Type[] { typeof(Decimal), typeof(Single), typeof(Double) } },
            { typeof(Single), new Type[] { typeof(Double) } },
            { typeof(Decimal), new Type[] { typeof(Single), typeof(Double) } },
            { typeof(Double), new Type[] { } },
        });

        /// <summary>
        /// Get the return type for an operation between the two numbers of the specified types
        /// </summary>
        /// <param name="left">Type of the left parameter</param>
        /// <param name="right">Type of the right parameter</param>
        /// <returns>The return type</returns>
        /// <exception cref="ArgumentNullException">Thrown if a parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if a parameter is not a supported numeric type.</exception>
        public static Type GetBinaryResultType(Type left, Type right)
        {
            if (left == null)
                throw new ArgumentNullException(paramName: nameof(left));
            if (right == null)
                throw new ArgumentNullException(paramName: nameof(right));

            if (!NumericConverter.NumericTypePromotions.TryGetValue(left, out Type[] leftTypes))
                throw new ArgumentException(message: "Argument is not numeric", paramName: nameof(left));
            if (!NumericConverter.NumericTypePromotions.TryGetValue(right, out Type[] rightTypes))
                throw new ArgumentException(message: "Argument is not numeric", paramName: nameof(right));

            if (left == right)
                return left;

            // Test left to right promotion
            if (rightTypes.Contains(left))
                return left;
            if (leftTypes.Contains(right))
                return right;
            return rightTypes.First(_type => leftTypes.Contains(_type));
        }

        /// <summary>
        /// Coerce an object into a numeric type.
        /// </summary>
        /// <param name="value">The string to coerce.</param>
        /// <param name="formatProvider">The format provider for converting floating point numbers.</param>
        /// <param name="defaultValue">The value to return if coercion fails.</param>
        /// <returns>The coerced value as int, long, double or decimal type, or <paramref name="defaultValue"/> if coercion fails.</returns>
        public static object CoerceToNumericType(object value, IFormatProvider formatProvider, object defaultValue)
        {
            if (NumericConverter.TryCoerceToNumericType(value, formatProvider, out object convertedValue))
            {
                return convertedValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Try to coerce an object into a numeric type.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="formatProvider">The format provider for converting floating point numbers.</param>
        /// <param name="convertedValue">The coerced value as int, long, double or decimal type, or null if parsing fails.</param>
        /// <returns>true if parsing was successful; Otherwise, false.</returns>
        public static bool TryCoerceToNumericType(object value, IFormatProvider formatProvider, out object convertedValue)
        {
            if (value != null)
            {
                if (NumericConverter.IsNumeric(value))
                {
                    convertedValue = value;
                    return true;
                }
                else if (value is string stringValue)
                {
                    return NumericConverter.TryParseToNumericType(stringValue, formatProvider, out convertedValue);
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
        public static bool TryParseToNumericType(string value, IFormatProvider formatProvider, out object convertedValue)
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
