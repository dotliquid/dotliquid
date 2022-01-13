using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotLiquid.Util
{
    public static class StrFTime
    {
        // Group names and Regex to capture all supported specifiers from a provided format string
        private const string GROUP_FLAGS = "flags";
        private const string GROUP_WIDTH = "width";
        private const string GROUP_DIRECTIVE = "directive";
        private const string SPECIFIER_REGEX = "%(?<" + GROUP_FLAGS + ">[-_0^:#])*"
                                             + "(?<" + GROUP_WIDTH + ">[1-9][0-9]*)?"
                                             + "(?<" + GROUP_DIRECTIVE + @">[a-zA-Z%])";

        private delegate string DateTimeDelegate(DateTime dateTime, CultureInfo culture);
        private delegate string DateTimeOffsetDelegate(DateTimeOffset dateTimeOffset, CultureInfo culture);

        private static readonly Dictionary<string, DateTimeDelegate> Formats = new Dictionary<string, DateTimeDelegate>
        {
            { "a", (dateTime, culture) => dateTime.ToString("ddd", culture) },
            { "A", (dateTime, culture) => dateTime.ToString("dddd", culture) },
            { "b", (dateTime, culture) => dateTime.ToString("MMM", culture) },
            { "B", (dateTime, culture) => dateTime.ToString("MMMM", culture) },
            { "c", (dateTime, culture) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", culture) },
            { "C", (dateTime, culture) => ((int)Math.Floor(Convert.ToDouble(dateTime.ToString("yyyy", culture))/100)).ToString(culture) },
            { "d", (dateTime, culture) => dateTime.ToString("dd", culture) },
            { "D", (dateTime, culture) => dateTime.ToString("MM/dd/yy", culture) },
            { "e", (dateTime, culture) => dateTime.ToString("%d", culture).PadLeft(2, ' ') },
            // E - not specified
            // f - not specified
            { "F", (dateTime, culture) => dateTime.ToString("yyyy-MM-dd", culture) },
            { "g", (dateTime, culture) => dateTime.GetIso8601WeekOfYear("g", culture) },
            { "G", (dateTime, culture) => dateTime.GetIso8601WeekOfYear("G", culture) },
            { "h", (dateTime, culture) => dateTime.ToString("MMM", culture) },
            { "H", (dateTime, culture) => dateTime.ToString("HH", culture) },
            // i - not specified
            { "I", (dateTime, culture) => dateTime.ToString("hh", culture) },
            { "j", (dateTime, culture) => dateTime.DayOfYear.ToString(culture).PadLeft(3, '0') },
            // J - not specified
            { "k", (dateTime, culture) => dateTime.ToString("%H", culture) },
            // K - not specified
            { "l", (dateTime, culture) => dateTime.ToString("%h", culture).PadLeft(2, ' ') },
            { "L", (dateTime, culture) => dateTime.ToString("fff", culture) },
            { "m", (dateTime, culture) => dateTime.ToString("MM", culture) },
            { "M", (dateTime, culture) => dateTime.Minute.ToString(culture).PadLeft(2, '0') },
            { "n", (dateTime, culture) => "\n" },
            { "N", (dateTime, culture) => dateTime.ToString("ffffff", culture) }, //The Ruby spec states default=nanoseconds, but nanosecond precision is not supported by a C# DateTime
            { "3N", (dateTime, culture) => dateTime.ToString("fff", culture) },
            { "6N", (dateTime, culture) => dateTime.ToString("ffffff", culture) },
            // 9N - not implemented
            // o - not specified
            // O - not specified
            { "p", (dateTime, culture) => dateTime.ToString("tt", culture).ToUpper() },
            { "P", (dateTime, culture) => dateTime.ToString("tt", culture).ToLower() },
            // q - not specified
            // Q - not specified
            { "r", (dateTime, culture) => dateTime.ToString("hh:mm:ss tt", culture).ToUpper() },
            { "R", (dateTime, culture) => dateTime.ToString("HH:mm", culture) },
            { "s", (dateTime, culture) => ((int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString(culture) },
            { "S", (dateTime, culture) => dateTime.ToString("ss", culture) },
            { "t", (dateTime, culture) => "\t" },
            { "T", (dateTime, culture) => dateTime.ToString("HH:mm:ss", culture) },
            { "u", (dateTime, culture) => ((int)(dateTime.DayOfWeek) == 0 ? ((int)(dateTime).DayOfWeek) + 7 : ((int)(dateTime).DayOfWeek)).ToString(culture) },
            { "U", (dateTime, culture) => culture.Calendar.GetWeekOfYear(dateTime, culture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString(culture).PadLeft(2, '0') },
            { "v", (dateTime, culture) => dateTime.ToString("%d-MMM-yyyy", culture).ToUpper().PadLeft(11, ' ') },
            { "V", (dateTime, culture) => dateTime.GetIso8601WeekOfYear("V", culture) },
            { "w", (dateTime, culture) => ((int) dateTime.DayOfWeek).ToString(culture) },
            { "W", (dateTime, culture) => culture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday).ToString(culture).PadLeft(2, '0') },
            { "x", (dateTime, culture) => dateTime.ToString("MM/dd/yy", culture) },
            { "X", (dateTime, culture) => dateTime.ToString("HH:mm:ss", culture) },
            { "y", (dateTime, culture) => dateTime.ToString("yy", culture) },
            { "Y", (dateTime, culture) => dateTime.ToString("yyyy", culture) },
            { "z", (dateTime, culture) => dateTime.ToString("%K", culture).Replace(":", string.Empty) },
            { ":z", (dateTime, culture) => dateTime.ToString("%K", culture) },
            // ::z - not implemented
            { "Z", (dateTime, culture) => dateTime.ToString("zzz", culture) },
            { "%", (dateTime, culture) => "%" } // A % sign
        };

        private static readonly Dictionary<string, DateTimeOffsetDelegate> OffsetFormats = new Dictionary<string, DateTimeOffsetDelegate>
        {
            { "s", (dateTimeOffset, culture) => ((long)(dateTimeOffset - new DateTimeOffset(1970, 1, 1, 0,0,0, TimeSpan.Zero)).TotalSeconds).ToString(culture) },
            { "z", (dateTimeOffset, culture) => dateTimeOffset.ToString("%K", culture).Replace(":", string.Empty) },
            { ":z", (dateTimeOffset, culture) => dateTimeOffset.ToString("%K", culture) },
            { "Z", (dateTimeOffset, culture) => dateTimeOffset.ToString("zzz", culture) }
        };

        /// <summary>
        /// Applies formatting consistent to the rules specified by the Ruby Time.strftime function.
        /// The following exceptions apply;
        /// - `%N` (Not standard) - DateTime does not support nanoseconds precision, so the default is microsecond (6dp).
        /// - `%9N` (Not implemented) - DateTime does not support nanoseconds precision.
        /// - `%Z` (Not standard) - DotLiquid returns an offset (+00:00), Ruby states this should return a timezone description (e.g. 'GMT')
        /// <see href="https://help.shopify.com/themes/liquid/filters/additional-filters#date"/>
        /// <see href="https://ruby-doc.org/core-3.0.0/Time.html#method-i-strftime"/>
        /// </summary>
        /// <param name="dateTime">date-time object to be formatted</param>
        /// <param name="format">the required format</param>
        /// <param name="culture">the CurrentCulture to be used when formatting</param>
        /// <returns>a string version of date-time matching pattern.</returns>
        public static string ToStrFTime(this DateTime dateTime, string format, CultureInfo culture)
        {
            culture = culture ?? throw new ArgumentException(message: "CultureInfo is mandatory", paramName: "culture");
            return Regex.Replace(input: format, pattern: SPECIFIER_REGEX,
                evaluator: specifier => SpecifierEvaluator(
                    specifier: specifier.Groups[0].Value,
                    flags: specifier.Groups[GROUP_FLAGS].Captures.Cast<Capture>().Select(capture => capture.Value).ToList(),
                    width: specifier.Groups[GROUP_WIDTH].Captures.Cast<Capture>().Select(capture => (int?)Convert.ToInt32(capture.Value)).FirstOrDefault(),
                    directive: specifier.Groups[GROUP_DIRECTIVE].Captures.Cast<Capture>().Select(capture => capture.Value).FirstOrDefault(),
                    source: dateTime,
                    culture: culture
                    ));
        }

        /// <summary>
        /// Formats a date using a ruby date format string
        /// </summary>
        /// <param name="dateTimeOffset">date-time object to be formatted</param>
        /// <param name="format">the required format</param>
        /// <param name="culture">the CurrentCulture to be used when formatting</param>
        /// <returns>a string version of date-time matching pattern.</returns>
        public static string ToStrFTime(this DateTimeOffset dateTimeOffset, string format, CultureInfo culture)
        {
            culture = culture ?? throw new ArgumentException(message: "CultureInfo is mandatory", paramName: "culture");
            return Regex.Replace(input: format, pattern: SPECIFIER_REGEX,
                evaluator: specifier => SpecifierEvaluator(
                    specifier: specifier.Groups[0].Value,
                    flags: specifier.Groups[GROUP_FLAGS].Captures.Cast<Capture>().Select(capture => capture.Value).ToList(),
                    width: specifier.Groups[GROUP_WIDTH].Captures.Cast<Capture>().Select(capture => (int?)Convert.ToInt32(capture.Value)).FirstOrDefault(),
                    directive: specifier.Groups[GROUP_DIRECTIVE].Captures.Cast<Capture>().Select(capture => capture.Value).FirstOrDefault(),
                    source: dateTimeOffset,
                    culture: culture
                    ));
        }

        /// <summary>
        /// Formats a date-time for a single specifier within the requested format. For ease of processing
        /// the specifier is broken down into it's constituent parts of: flags, width, directive.
        ///
        /// A specifier consists of a percent (%) character, zero or more flags, optional minimum field width and a conversion directive as follows:
        /// </summary>
        /// <param name="specifier">the entire specifier, such as %Y for a 4-digit year</param>
        /// <param name="flags">zero or more flags to change formatting, such as '0' (zero padding).</param>
        /// <param name="width">optional minimum field width</param>
        /// <param name="directive">The required data value, such as Y for a 4-digit year</param>
        /// <param name="source">the source date-time object</param>
        /// <param name="culture">the CurrentCulture to be used when formatting</param>
        private static String SpecifierEvaluator(String specifier, IEnumerable<String> flags, int? width, String directive, object source, CultureInfo culture)
        {
            var result = specifier;
            directive = PreProcessDirective(directive, flags, width);

            if (OffsetFormats.ContainsKey(directive) && source is DateTimeOffset dateTimeOffset1)
                result = OffsetFormats[directive].Invoke(dateTimeOffset1, culture);
            else if (Formats.ContainsKey(directive))
                result = Formats[directive].Invoke(source is DateTimeOffset dateTimeOffset2 ? dateTimeOffset2.DateTime : (DateTime)source, culture);
            else
                return specifier; // This is an unconfigured specifier

            return flags.ToList().Aggregate(result, (current, flag) => ApplyFlag(flag, (width ?? 2), current));
        }

        // Pre-process the directive for some of the quirkier cases, such as '%:z' and '%3N'.
        private static String PreProcessDirective(String directive, IEnumerable<String> flags, int? width)
        {
            var flagString = String.Concat(flags);
            if ("z".Equals(directive, StringComparison.Ordinal) && ":".Equals(flagString, StringComparison.Ordinal))
                return flagString + directive;
            else if ("N".Equals(directive, StringComparison.Ordinal) && (width == 3 || width == 6))
                return "" + width + directive;

            return directive;
        }

        private static String ApplyFlag(String flag, int padwidth, String str)
        {
            switch (flag)
            {
                case "-": //don't pad a numerical output
                    return str.TrimStart('0');
                case "_": //use spaces for padding
                    return str.TrimStart('0').PadLeft(padwidth, ' ');
                case "0": //use zeros for padding
                    return str.TrimStart('0').PadLeft(padwidth, '0');
                case "^": //upcase the result string
                    return str.ToUpper();
                case ":": // handled by PreProcessDirective
                case "#": // not implemented
                    return str;
                default: // unexpected flag, the regex must be wrong.
                    throw new ArgumentException(message: "Invalid flag passed to ApplyFlag", paramName: "flag");
            }
        }

        /// <summary>
        /// ISO 8601 week-based year and week number:
        /// - The week 1 of YYYY starts with a Monday and includes YYYY-01-04.
        /// - The days in the year before the first week are in the last week of the previous year.
        /// For example 2012-12-31 is in the first week of 2013 (according to ISO-8601)
        /// <param name="dateTime">date-time object to be formatted</param>
        /// <param name="directive">The required data value, such as Y for a 4-digit year</param>
        /// <param name="culture">the CurrentCulture to be used when formatting</param>
        /// </summary>
        private static string GetIso8601WeekOfYear(this DateTime dateTime, string directive, CultureInfo culture)
        {
            // If its Monday, Tuesday or Wednesday, then it'll be the same week
            // as whatever Thursday, Friday or Saturday are.
            DayOfWeek day = culture.Calendar.GetDayOfWeek(dateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                dateTime = dateTime.AddDays(3);
            }

            switch (directive)
            {
                case "G":
                    return dateTime.ToString("yyyy", culture);
                case "g":
                    return dateTime.ToString("yy", culture);
                case "V":
                    // Return the week number of our adjusted day
                    return culture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString().PadLeft(2, '0');
                default:
                    throw new ArgumentException(message: "Invalid directive passed to GetIso8601WeekOfYear", paramName: "directive");
            }
        }
    }
}
