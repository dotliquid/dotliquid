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

        private delegate string DateTimeDelegate(DateTime dateTime);
        private delegate string DateTimeOffsetDelegate(DateTimeOffset dateTimeOffset);

        private static readonly Dictionary<string, DateTimeDelegate> Formats = new Dictionary<string, DateTimeDelegate>
        {
            { "a", (dateTime) => dateTime.ToString("ddd", CultureInfo.CurrentCulture) },
            { "A", (dateTime) => dateTime.ToString("dddd", CultureInfo.CurrentCulture) },
            { "b", (dateTime) => dateTime.ToString("MMM", CultureInfo.CurrentCulture) },
            { "B", (dateTime) => dateTime.ToString("MMMM", CultureInfo.CurrentCulture) },
            { "c", (dateTime) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", CultureInfo.CurrentCulture) },
            { "C", (dateTime) => ((int)Math.Floor(Convert.ToDouble(dateTime.ToString("yyyy", CultureInfo.CurrentCulture))/100)).ToString(CultureInfo.CurrentCulture) },
            { "d", (dateTime) => dateTime.ToString("dd", CultureInfo.CurrentCulture) },
            { "D", (dateTime) => dateTime.ToString("MM/dd/yy", CultureInfo.CurrentCulture) },
            { "e", (dateTime) => dateTime.ToString("%d", CultureInfo.CurrentCulture).PadLeft(2, ' ') },
            // E - not specified
            // f - not specified
            { "F", (dateTime) => dateTime.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture) },
            { "g", (dateTime) => dateTime.GetIso8601WeekOfYear("g") },
            { "G", (dateTime) => dateTime.GetIso8601WeekOfYear("G") },
            { "h", (dateTime) => dateTime.ToString("MMM", CultureInfo.CurrentCulture) },
            { "H", (dateTime) => dateTime.ToString("HH", CultureInfo.CurrentCulture) },
            // i - not specified
            { "I", (dateTime) => dateTime.ToString("hh", CultureInfo.CurrentCulture) },
            { "j", (dateTime) => dateTime.DayOfYear.ToString(CultureInfo.CurrentCulture).PadLeft(3, '0') },
            // J - not specified
            { "k", (dateTime) => dateTime.ToString("%H", CultureInfo.CurrentCulture) },
            // K - not specified
            { "l", (dateTime) => dateTime.ToString("%h", CultureInfo.CurrentCulture).PadLeft(2, ' ') },
            { "L", (dateTime) => dateTime.ToString("fff", CultureInfo.CurrentCulture) },
            { "m", (dateTime) => dateTime.ToString("MM", CultureInfo.CurrentCulture) },
            { "M", (dateTime) => dateTime.Minute.ToString(CultureInfo.CurrentCulture).PadLeft(2, '0') },
            { "n", (dateTime) => "\n" },
            { "N", (dateTime) => dateTime.ToString("ffffff", CultureInfo.CurrentCulture) }, //The Ruby spec states default=nanoseconds, but nanosecond precision is not supported by a C# DateTime
            { "3N", (dateTime) => dateTime.ToString("fff", CultureInfo.CurrentCulture) },
            { "6N", (dateTime) => dateTime.ToString("ffffff", CultureInfo.CurrentCulture) },
            // 9N - not implemented
            // o - not specified
            // O - not specified
            { "p", (dateTime) => dateTime.ToString("tt", CultureInfo.CurrentCulture).ToUpper() },
            { "P", (dateTime) => dateTime.ToString("tt", CultureInfo.CurrentCulture).ToLower() },
            // q - not specified
            // Q - not specified
            { "r", (dateTime) => dateTime.ToString("hh:mm:ss tt", CultureInfo.CurrentCulture).ToUpper() },
            { "R", (dateTime) => dateTime.ToString("HH:mm", CultureInfo.CurrentCulture) },
            { "s", (dateTime) => ((int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString(CultureInfo.CurrentCulture) },
            { "S", (dateTime) => dateTime.ToString("ss", CultureInfo.CurrentCulture) },
            { "t", (dateTime) => "\t" },
            { "T", (dateTime) => dateTime.ToString("HH:mm:ss", CultureInfo.CurrentCulture) },
            { "u", (dateTime) => ((int)(dateTime.DayOfWeek) == 0 ? ((int)(dateTime).DayOfWeek) + 7 : ((int)(dateTime).DayOfWeek)).ToString(CultureInfo.CurrentCulture) },
            { "U", (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString(CultureInfo.CurrentCulture).PadLeft(2, '0') },
            { "v", (dateTime) => dateTime.ToString("%d-MMM-yyyy", CultureInfo.CurrentCulture).ToUpper().PadLeft(11, ' ') },
            { "V", (dateTime) => dateTime.GetIso8601WeekOfYear("V") },
            { "w", (dateTime) => ((int) dateTime.DayOfWeek).ToString(CultureInfo.CurrentCulture) },
            { "W", (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday).ToString(CultureInfo.CurrentCulture).PadLeft(2, '0') },
            { "x", (dateTime) => dateTime.ToString("MM/dd/yy", CultureInfo.CurrentCulture) },
            { "X", (dateTime) => dateTime.ToString("HH:mm:ss", CultureInfo.CurrentCulture) },
            { "y", (dateTime) => dateTime.ToString("yy", CultureInfo.CurrentCulture) },
            { "Y", (dateTime) => dateTime.ToString("yyyy", CultureInfo.CurrentCulture) },
            { "z", (dateTime) => dateTime.ToString("%K", CultureInfo.CurrentCulture).Replace(":", string.Empty) },
            { ":z", (dateTime) => dateTime.ToString("%K", CultureInfo.CurrentCulture) },
            // ::z - not implemented
            { "Z", (dateTime) => dateTime.ToString("zzz", CultureInfo.CurrentCulture) },
            { "%", (dateTime) => "%" } // A % sign
        };

        private static readonly Dictionary<string, DateTimeOffsetDelegate> OffsetFormats = new Dictionary<string, DateTimeOffsetDelegate>
        {
            { "s", (dateTimeOffset) => ((long)(dateTimeOffset - new DateTimeOffset(1970, 1, 1, 0,0,0, TimeSpan.Zero)).TotalSeconds).ToString(CultureInfo.CurrentCulture) },
            { "z", (dateTimeOffset) => dateTimeOffset.ToString("%K", CultureInfo.CurrentCulture).Replace(":", string.Empty) },
            { ":z", (dateTimeOffset) => dateTimeOffset.ToString("%K", CultureInfo.CurrentCulture) },
            { "Z", (dateTimeOffset) => dateTimeOffset.ToString("zzz", CultureInfo.CurrentCulture) },
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
        /// <returns>a string version of date-time matching pattern.</returns>
        public static string ToStrFTime(this DateTime dateTime, string format)
        {
            return Regex.Replace(input: format, pattern: SPECIFIER_REGEX,
                evaluator: specifier => SpecifierEvaluator(
                    specifier: specifier.Groups[0].Value,
                    flags: specifier.Groups[GROUP_FLAGS].Captures.Cast<Capture>().Select(capture => capture.Value).ToList(),
                    width: specifier.Groups[GROUP_WIDTH].Captures.Cast<Capture>().Select(capture => (int?)Convert.ToInt32(capture.Value)).FirstOrDefault(),
                    directive: specifier.Groups[GROUP_DIRECTIVE].Captures.Cast<Capture>().Select(capture => capture.Value).FirstOrDefault(),
                    source: dateTime
                    ));
        }

        /// <summary>
        /// Formats a date using a ruby date format string
        /// </summary>
        /// <param name="dateTimeOffset">date-time object to be formatted</param>
        /// <param name="format">the required format</param>
        /// <returns>a string version of date-time matching pattern.</returns>
        public static string ToStrFTime(this DateTimeOffset dateTimeOffset, string format)
        {
            return Regex.Replace(input: format, pattern: SPECIFIER_REGEX,
                evaluator: specifier => SpecifierEvaluator(
                    specifier: specifier.Groups[0].Value,
                    flags: specifier.Groups[GROUP_FLAGS].Captures.Cast<Capture>().Select(capture => capture.Value).ToList(),
                    width: specifier.Groups[GROUP_WIDTH].Captures.Cast<Capture>().Select(capture => (int?)Convert.ToInt32(capture.Value)).FirstOrDefault(),
                    directive: specifier.Groups[GROUP_DIRECTIVE].Captures.Cast<Capture>().Select(capture => capture.Value).FirstOrDefault(),
                    source: dateTimeOffset
                    ));
        }

        /// <summary>
        /// Formats a date-time for a single specifier within the requested format. For ease of processing
        /// the specifier is broken down into it's consituent parts of: flags, width, directive.
        ///
        /// A specifier consists of a percent (%) character, zero or more flags, optional minimum field width and a conversion directive as follows:
        /// </summary>
        /// <param name="specifier">the entire specifier, such as %Y for a 4-digit year</param>
        /// <param name="flags">zero or more flags to change formatting, such as '0' (zero padding).</param>
        /// <param name="width">optional minimum field width</param>
        /// <param name="directive">The required data value, such as Y for a 4-digit year</param>
        /// <param name="source">the source date-time object</param>
        private static String SpecifierEvaluator(String specifier, IEnumerable<String> flags, int? width, String directive, object source)
        {
            var result = specifier;
            directive = PreProcessDirective(directive, flags, width);

            if (OffsetFormats.ContainsKey(directive) && source is DateTimeOffset dateTimeOffset1)
                result = OffsetFormats[directive].Invoke(dateTimeOffset1);
            else if (Formats.ContainsKey(directive))
                result = Formats[directive].Invoke(source is DateTimeOffset dateTimeOffset2 ? dateTimeOffset2.DateTime : (DateTime)source);
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
        /// </summary>
        private static string GetIso8601WeekOfYear(this DateTime dateTime, string directive)
        {
            // If its Monday, Tuesday or Wednesday, then it'll be the same week
            // as whatever Thursday, Friday or Saturday are.
            DayOfWeek day = CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(dateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                dateTime = dateTime.AddDays(3);
            }

            switch (directive)
            {
                case "G":
                    return dateTime.ToString("yyyy", CultureInfo.CurrentCulture);
                case "g":
                    return dateTime.ToString("yy", CultureInfo.CurrentCulture);
                case "V":
                    // Return the week number of our adjusted day
                    return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString().PadLeft(2, '0');
                default:
                    throw new ArgumentException(message: "Invalid directive passed to GetIso8601WeekOfYear", paramName: "directive");
            }
        }
    }
}
