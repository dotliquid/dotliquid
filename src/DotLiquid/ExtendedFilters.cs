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
        /// Convert a UNIX Epoch timestamp in milliseconds into a UTC DateTimeOffset.
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">string/long containing milliseconds</param>
        public static object UnixMs(Context context, object input)
        {
            if ((input is double) || (input is long) || (input is ulong))
            {
                return CreateDateTimeOffsetFromUnixMilliseconds(Convert.ToInt64(input, context.FormatProvider));
            }
            else if (input is string stringInput &&
                long.TryParse(stringInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp))
            {
                return CreateDateTimeOffsetFromUnixMilliseconds(timestamp);
            }

            return input;
        }

        /// <summary>
        /// Convert a date-time input to a different timezone.
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">DateTimeOffset, DateTime (unspecified, local or UTC) or a date string</param>
        /// <param name="convertToTimezoneId">Windows timezone ID to convert to</param>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.timezoneinfo.findsystemtimezonebyid?view=netcore-3.1#remarks"/>
        public static object TimeZone(Context context, object input, string convertToTimezoneId)
        {
            // Convert the input to a DateTimeOffset
            if (input is DateTimeOffset dateTimeOffset ||
                (input is string stringInput && DateTimeOffset.TryParse(stringInput, out dateTimeOffset)))
            {
                // Accept a DateTimeOffset or date-string
            }
            else if (input is DateTime dateTime)
            {
                dateTimeOffset = new DateTimeOffset(dateTime);
            }
            else
            {
                return input; // not a supported input data type
            }

            // Identify the target timezone
            TimeZoneInfo destinationTimeZone = null;
            if (string.Equals(convertToTimezoneId, "Local", StringComparison.OrdinalIgnoreCase))
                destinationTimeZone = TimeZoneInfo.Local;
            else if (string.Equals(convertToTimezoneId, "UTC", StringComparison.OrdinalIgnoreCase))
                destinationTimeZone = TimeZoneInfo.Utc;
            else
                // Attempt to retrieve a timezone by ID
                try
                {
                    destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(convertToTimezoneId);
                }
                catch
                {
                    throw new SyntaxException(message: "TimeZoneNotAvailableException", args: convertToTimezoneId);
                }

            // Convert and return date in target timezone
            return TimeZoneInfo.ConvertTime(dateTimeOffset, destinationTimeZone);
        }

        /// <summary>
        /// Create a DateTimeOffset from the provided UNIX Epoch timestamp in milliseconds.
        /// </summary>
        /// <return cref="DateTimeOffset">A Date in UTC</return>
        private static DateTimeOffset CreateDateTimeOffsetFromUnixMilliseconds(long milliseconds)
        {
#if NET45
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(milliseconds);
#else
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
#endif
        }
    }
}
