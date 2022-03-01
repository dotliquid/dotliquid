using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// Extended DotLiquid Filters. Not registered by default.
    /// </summary>
    public static class ExtendedFilters
    {
        /// <summary>
        /// Capitalize all the words in the input sentence
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string Titleize(Context context, string input)
        {
            return input.IsNullOrWhiteSpace()
                ? input
#if CORE
                : Regex.Replace(input, @"\b(\w)", m => m.Value.ToUpper(), RegexOptions.None, Template.RegexTimeOut);
#else
                : context.CurrentCulture.TextInfo.ToTitleCase(input);
#endif
        }

        /// <summary>
        /// Converts just the first character to uppercase
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string UpcaseFirst(Context context, string input)
        {
            if (input.IsNullOrWhiteSpace())
                return input;

            var trimmed = input.TrimStart();
            return input.Substring(0, input.Length - trimmed.Length) + char.ToUpper(trimmed[0]) + trimmed.Substring(1);
        }

        /// <summary>
        /// Convert a millisecond UNIX Epoch timestamp or date-time string into a target timezone as a formatted date string.
        /// </summary>
        /// <remarks>
        /// If you have a UNIX Epoch timestamp in seconds use the Date filter, or divide your timestamp by 1000 using the DividedBy filter.
        /// </remarks>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed to a string</param>
        /// <param name="format">.NET or Ruby StrFTime format specifying output formatting</param>
        /// <param name="convertToTimezoneId">Windows timezone ID to convert to, if null either the input's timezone, or system default will be used</param>
        public static object ConvertTime(Context context, object input, string format, string convertToTimezoneId = null)
        {
            if (input == null || format.IsNullOrWhiteSpace())
                return input;

            if (input is DateTimeOffset dateTimeOffset)
            { }
            else if (input is DateTime dateTime)
            {
                dateTimeOffset = new DateTimeOffset(dateTime);
            }
            else if ((input is decimal) || (input is double) || (input is float) || (input is int) || (input is uint) || (input is long) || (input is ulong) || (input is short) || (input is ushort))
            {
                dateTimeOffset = CreateDateTimeOffsetFromUnixTimestamp(Convert.ToInt64(input));
            }
            else if (input is string stringInput)
            {
                if (string.Equals(stringInput, "now", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(stringInput, "today", StringComparison.OrdinalIgnoreCase))
                    dateTimeOffset = DateTimeOffset.Now; // special word
                else if (long.TryParse(stringInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp))
                    dateTimeOffset = CreateDateTimeOffsetFromUnixTimestamp(timestamp);
                else if (!DateTimeOffset.TryParse(stringInput, out dateTimeOffset))
                    return input; // not a date literal string
            }
            else
            {
                return input; // cannot be converted to a DateTimeOffset
            }

            // If a target timezone is specified attempt to convert the date-time
            if (!string.IsNullOrEmpty(convertToTimezoneId))
                try
                {
                    TimeZoneInfo destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(convertToTimezoneId);
                    dateTimeOffset = TimeZoneInfo.ConvertTime(dateTimeOffset, destinationTimeZone);
                }
                catch
                {
                    throw new SyntaxException(message: "TimeZoneNotAvailableException", args: convertToTimezoneId);
                }

            return context.UseRubyDateFormat ? dateTimeOffset.ToStrFTime(format, context.CurrentCulture) : dateTimeOffset.ToString(format);
        }

        /// <summary>
        /// Create a DateTimeOffset from the provided UNIX Epoch timestamp, inferring whether the
        /// timestamp is seconds or milliseconds.
        /// </summary>
        /// <return cref="DateTimeOffset">A Date in UTC</return>
        private static DateTimeOffset CreateDateTimeOffsetFromUnixTimestamp(long milliseconds)
        {
#if NET45
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(milliseconds);
#else
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
#endif
        }
    }
}
