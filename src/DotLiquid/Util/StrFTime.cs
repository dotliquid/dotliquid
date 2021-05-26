using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotLiquid.Util
{
    public static class StrFTime
    {
        private delegate string DateTimeDelegate(DateTime dateTime);
        private delegate string DateTimeOffsetDelegate(DateTimeOffset dateTimeOffset);

        private static readonly Dictionary<string, DateTimeDelegate> Formats = new Dictionary<string, DateTimeDelegate>
        {
            { "a", (dateTime) => dateTime.ToString("ddd", CultureInfo.CurrentCulture) },
            { "A", (dateTime) => dateTime.ToString("dddd", CultureInfo.CurrentCulture) },
            { "b", (dateTime) => dateTime.ToString("MMM", CultureInfo.CurrentCulture) },
            { "B", (dateTime) => dateTime.ToString("MMMM", CultureInfo.CurrentCulture) },
            { "c", (dateTime) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", CultureInfo.CurrentCulture) },
            { "C", (dateTime) => ((int)Math.Floor(Convert.ToDouble(dateTime.ToString("yyyy"))/100)).ToString() },
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
            { "j", (dateTime) => dateTime.DayOfYear.ToString().PadLeft(3, '0') },
            // J - not specified
            { "k", (dateTime) => dateTime.ToString("%H", CultureInfo.CurrentCulture) },
            // K - not specified
            { "l", (dateTime) => dateTime.ToString("%h", CultureInfo.CurrentCulture).PadLeft(2, ' ') },
            { "L", (dateTime) => dateTime.ToString("fff", CultureInfo.CurrentCulture) },
            { "m", (dateTime) => dateTime.ToString("MM", CultureInfo.CurrentCulture) },
            { "M", (dateTime) => dateTime.Minute.ToString().PadLeft(2, '0') },
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
            { "s", (dateTime) => ((int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString() },
            { "S", (dateTime) => dateTime.ToString("ss", CultureInfo.CurrentCulture) },
            { "t", (dateTime) => "\t" },
            { "T", (dateTime) => dateTime.ToString("HH:mm:ss", CultureInfo.CurrentCulture) },
            { "u", (dateTime) => ((int)(dateTime.DayOfWeek) == 0 ? ((int)(dateTime).DayOfWeek) + 7 : ((int)(dateTime).DayOfWeek)).ToString() },
            { "U", (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString().PadLeft(2, '0') },
            { "v", (dateTime) => dateTime.ToString("%d-MMM-yyyy", CultureInfo.CurrentCulture).ToUpper().PadLeft(11, ' ') },
            { "V", (dateTime) => dateTime.GetIso8601WeekOfYear("V") },
            { "w", (dateTime) => ((int) dateTime.DayOfWeek).ToString() },
            { "W", (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday).ToString().PadLeft(2, '0') },
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
            { "s", (dateTimeOffset) => ((long)(dateTimeOffset - new DateTimeOffset(1970, 1, 1, 0,0,0, TimeSpan.Zero)).TotalSeconds).ToString() },
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
        /// <param name="dateTime"/>
        /// <param name="pattern"/>
        /// <returns>a string version of dateTime matching pattern.</returns>
        public static string ToStrFTime(this DateTime dateTime, string pattern)
        {
            return Regex.Replace(pattern, @"%(?<flag>[-_0^:#])*(?<width>[1-9][0-9]*)?(?<directive>[a-zA-Z%\+])",
                x => StrFTimeMatchEvaluator(
                    x.Groups[0].Value,
                    x.Groups["flag"].Captures.Cast<Capture>().Select(y => y.Value).ToList(),
                    x.Groups["width"].Captures.Cast<Capture>().Select(y => (int?)Convert.ToInt32(y.Value)).FirstOrDefault(),
                    x.Groups["directive"].Captures.Cast<Capture>().Select(y => y.Value).FirstOrDefault(),
                    dateTime
                    ));
        }

        /// <summary>
        /// Formats a date using a ruby date format string
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string ToStrFTime(this DateTimeOffset dateTimeOffset, string pattern)
        {
            return Regex.Replace(pattern, @"%(?<flag>[-_0^:#])*(?<width>[1-9][0-9]*)?(?<directive>[a-zA-Z%\+])",
                x => StrFTimeMatchEvaluator(
                    x.Groups[0].Value,
                    x.Groups["flag"].Captures.Cast<Capture>().Select(y => y.Value).ToList(),
                    x.Groups["width"].Captures.Cast<Capture>().Select(y => (int?)Convert.ToInt32(y.Value)).FirstOrDefault(),
                    x.Groups["directive"].Captures.Cast<Capture>().Select(y => y.Value).FirstOrDefault(),
                    dateTimeOffset
                    ));
        }

        private static String StrFTimeMatchEvaluator(String orig, IEnumerable<String> flags, int? width, String directive, object obj)
        {
            var result = orig;
            directive = PreProcessDirective(directive, flags, width);

            bool isDateTimeOffset = obj is DateTimeOffset dateTimeOffset;
            if (OffsetFormats.ContainsKey(directive) && isDateTimeOffset)
                result = OffsetFormats[directive].Invoke(dateTimeOffset);
            else if (Formats.ContainsKey(directive))
                result = Formats[directive].Invoke(isDateTimeOffset ? dateTimeOffset.DateTime : (DateTime)obj);
            else
                return orig; // This is an unspecified directive

            return flags.ToList().Aggregate(result, (current, flag) => ApplyFlag(flag, (width ?? 2), current));
        }

        // Pre-process the directive for some of the quirkier cases, such as '%:z' and '%3N'.
        private static String PreProcessDirective(String directive, IEnumerable<String> flags, int? width)
        {
            String result = directive;
            String flagString = String.Join("", flags);
            if ("z".Equals(directive) && ":".Equals(flagString))
            {
                result = flagString + directive;
            }
            else if ("N".Equals(directive) && (width == 3 || width == 6))
            {
                result = "" + width + directive;
            }
            return result;
        }

        private static String ApplyFlag(String flag, int padwidth, String str)
        {
            var result = str;
            switch (flag)
            {
                case "-": //don't pad a numerical output
                    result = str.TrimStart('0');
                    break;
                case "_": //use spaces for padding
                    result = str.TrimStart('0').PadLeft(padwidth, ' ');
                    break;
                case "0": //use zeros for padding
                    result = str.TrimStart('0').PadLeft(padwidth, '0');
                    break;
                case "^": //upcase the result string
                    result = str.ToUpper();
                    break;
                case ":": // handled by PreProcessDirective
                    break;
                case "#": // not implemented
                    break;
                    // default: do nothing.
            }
            return result;
        }

        /// <summary>
        /// ISO 8601 week-based year and week number:
        /// - The week 1 of YYYY starts with a Monday and includes YYYY-01-04.
        /// - The days in the year before the first week are in the last week of the previous year.
        /// For example 2012-12-31 is in the first week of 2013 (according to ISO-8601)
        /// <param name="dateTime"></param>
        /// <param name="directive"></param>
        /// </summary>
        private static string GetIso8601WeekOfYear(this DateTime dateTime, string directive)
        {
            // If its Monday, Tuesday or Wednesday, then it'll be the same week
            // as whatever Thursday, Friday or Saturday are.
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                dateTime = dateTime.AddDays(3);
            }

            switch (directive)
            {
                case "G":
                    return dateTime.ToString("yyyy");
                case "g":
                    return dateTime.ToString("yy");
                case "V":
                    // Return the week number of our adjusted day
                    return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString().PadLeft(2, '0');
                default:
                    throw new ArgumentException(message: "Invalid directive passed to GetIso8601WeekOfYear", paramName: "directive");
            }
        }
    }
}
