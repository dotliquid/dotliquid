using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid
{
    /// <summary>
    /// Extended DotLiquid Filters. Not registered by default.
    /// </summary>
    public static class ExtendedFilters
    {
        private static readonly string[] ISO_8601_FORMATS = new[] { "yyyy-MM-ddTHH:mm:sszzz", "yyyy-MM-ddTHH:mm:ssZ" };

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
                return Convert.ToInt64(input, context.FormatProvider).CreateDateTimeOffsetFromUnixMilliseconds();
            }
            else if (input is string stringInput &&
                long.TryParse(stringInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp))
            {
                return timestamp.CreateDateTimeOffsetFromUnixMilliseconds();
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
                (input is string stringInput && DateTimeOffset.TryParseExact(input: stringInput, formats: ISO_8601_FORMATS, formatProvider: context.CurrentCulture, styles: DateTimeStyles.None, result: out dateTimeOffset)))
            {
                // Accept a DateTimeOffset or ISO-8601 date-string with explicit time-zone
            }
            else
            {
                return input; // not a supported input data type
            }

            // Identify the target timezone
            TimeZoneInfo destinationTimeZone = null;
            if (string.Equals(convertToTimezoneId, "UTC", StringComparison.OrdinalIgnoreCase))
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
    }
}
