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

        private delegate string DateObjectDelegate(object dateTime, CultureInfo culture);
        private delegate string DateTimeDelegate(DateTime dateTime, CultureInfo culture);
        
        private static readonly Dictionary<string, DateObjectDelegate> Formats = new Dictionary<string, DateObjectDelegate>
        {
            { "a", (dateTime, culture) => string.Format(culture, "{0:ddd}", dateTime) },
            { "A", (dateTime, culture) => string.Format(culture, "{0:dddd}", dateTime) },
            { "b", (dateTime, culture) => string.Format(culture, "{0:MMM}", dateTime) },
            { "B", (dateTime, culture) => string.Format(culture, "{0:MMMM}", dateTime) },
            { "c", (dateTime, culture) => string.Format(culture, "{0:ddd MMM dd HH:mm:ss yyyy}", dateTime) },
            { "C", (dateTime, culture) => ((int)Math.Floor(Convert.ToDouble(string.Format(culture, "{0:yyyy}", dateTime))/100)).ToString(culture) },
            { "d", (dateTime, culture) => string.Format(culture, "{0:dd}", dateTime) },
            { "D", (dateTime, culture) => string.Format(culture, "{0:MM/dd/yy}", dateTime) },
            { "e", (dateTime, culture) => string.Format(culture, "{0:%d}", dateTime).PadLeft(2, ' ') },
            // E - not specified
            // f - not specified
            { "F", (dateTime, culture) => string.Format(culture, "{0:yyyy-MM-dd}", dateTime) },
            { "h", (dateTime, culture) => string.Format(culture, "{0:MMM}", dateTime) },
            { "H", (dateTime, culture) => string.Format(culture, "{0:HH}", dateTime) },
            // i - not specified
            { "I", (dateTime, culture) => string.Format(culture, "{0:hh}", dateTime) },
            // J - not specified
            { "k", (dateTime, culture) => string.Format(culture, "{0:%H}", dateTime) },
            // K - not specified
            { "l", (dateTime, culture) => string.Format(culture, "{0:%h}", dateTime).PadLeft(2, ' ') },
            { "L", (dateTime, culture) => string.Format(culture, "{0:fff}", dateTime) },
            { "m", (dateTime, culture) => string.Format(culture, "{0:MM}", dateTime) },
            { "M", (dateTime, culture) => string.Format(culture, "{0:mm}", dateTime) },
            { "n", (dateTime, culture) => "\n" },
            { "N", (dateTime, culture) => string.Format(culture, "{0:ffffff}", dateTime) }, //The Ruby spec states default=nanoseconds, but nanosecond precision is not supported by a C# DateTime
            { "3N", (dateTime, culture) => string.Format(culture, "{0:fff}", dateTime) },
            { "6N", (dateTime, culture) => string.Format(culture, "{0:ffffff}", dateTime) },
            // 9N - not implemented
            // o - not specified
            // O - not specified
            { "p", (dateTime, culture) => string.Format(culture, "{0:tt}", dateTime).ToUpper() },
            { "P", (dateTime, culture) => string.Format(culture, "{0:tt}", dateTime).ToLower() },
            // q - not specified
            // Q - not specified
            { "r", (dateTime, culture) => string.Format(culture, "{0:hh:mm:ss tt}", dateTime).ToUpper() },
            { "R", (dateTime, culture) => string.Format(culture, "{0:HH:mm}", dateTime) },
            { "s", (dateTime, culture) =>
            {
                if (dateTime is DateTimeOffset dto)
                    return ((long)(dto - new DateTimeOffset(1970, 1, 1, 0,0,0, TimeSpan.Zero)).TotalSeconds).ToString(culture);
                if (dateTime is DateTime dt)
                    return ((int)(dt - new DateTime(1970, 1, 1)).TotalSeconds).ToString(culture);
                throw new DotLiquid.Exceptions.DateFormatInvalidException();
            }},
            { "S", (dateTime, culture) => string.Format(culture, "{0:ss}", dateTime) },
            { "t", (dateTime, culture) => "\t" },
            { "T", (dateTime, culture) => string.Format(culture, "{0:HH:mm:ss}", dateTime) },
            { "v", (dateTime, culture) => string.Format(culture, "{0:%d-MMM-yyyy}", dateTime).ToUpper().PadLeft(11, ' ') },
            { "x", (dateTime, culture) => string.Format(culture, "{0:MM/dd/yy}", dateTime) },
            { "X", (dateTime, culture) => string.Format(culture, "{0:HH:mm:ss}", dateTime) },
            { "y", (dateTime, culture) => string.Format(culture, "{0:yy}", dateTime) },
            { "Y", (dateTime, culture) => string.Format(culture, "{0:yyyy}", dateTime) },
            { "z", (dateTime, culture) => string.Format(culture, "{0:%K}", dateTime).Replace(":", string.Empty) },
            { ":z", (dateTime, culture) => string.Format(culture, "{0:%K}", dateTime) },
            // ::z - not implemented
            { "Z", (dateTime, culture) => string.Format(culture, "{0:zzz}", dateTime) },
            { "%", (dateTime, culture) => "%" } // A % sign
        };

        private static readonly Dictionary<string, DateTimeDelegate> DateFormats = new Dictionary<string, DateTimeDelegate>
        {
            { "g", (dateTime, culture) => dateTime.GetIso8601WeekOfYear("g", culture) },
            { "G", (dateTime, culture) => dateTime.GetIso8601WeekOfYear("G", culture) },
            { "j", (dateTime, culture) => dateTime.DayOfYear.ToString(culture).PadLeft(3, '0') },
            { "u", (dateTime, culture) => ((int)(dateTime.DayOfWeek) == 0 ? ((int)(dateTime).DayOfWeek) + 7 : ((int)(dateTime).DayOfWeek)).ToString(culture) },
            { "U", (dateTime, culture) => culture.Calendar.GetWeekOfYear(dateTime, culture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString(culture).PadLeft(2, '0') },
            { "V", (dateTime, culture) => dateTime.GetIso8601WeekOfYear("V", culture) },
            { "w", (dateTime, culture) => ((int) dateTime.DayOfWeek).ToString(culture) },
            { "W", (dateTime, culture) => culture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday).ToString(culture).PadLeft(2, '0') }
        };

#if NET6_0_OR_GREATER
        /// <summary>
        /// Applies formatting consistent to the rules specified by the Ruby Time.strftime function.
        /// The following exceptions apply;
        /// - `%N` (Not standard) - DateTime does not support nanoseconds precision, so the default is microsecond (6dp).
        /// - `%9N` (Not implemented) - DateTime does not support nanoseconds precision.
        /// - `%Z` (Not standard) - DotLiquid returns an offset (+00:00), Ruby states this should return a timezone description (e.g. 'GMT')
        /// <see href="https://help.shopify.com/themes/liquid/filters/additional-filters#date"/>
        /// <see href="https://ruby-doc.org/core-3.0.0/Time.html#method-i-strftime"/>
        /// </summary>
        /// <param name="dateOnly">date-only object to be formatted</param>
        /// <param name="format">the required format</param>
        /// <param name="culture">the CurrentCulture to be used when formatting</param>
        /// <returns>a string version of date-time matching pattern.</returns>
        public static string ToStrFTime(this DateOnly dateOnly, string format, CultureInfo culture)
            => ObjectToStrFTime(dateOnly, format, culture);

        /// <summary>
        /// Applies formatting consistent to the rules specified by the Ruby Time.strftime function.
        /// The following exceptions apply;
        /// - `%N` (Not standard) - DateTime does not support nanoseconds precision, so the default is microsecond (6dp).
        /// - `%9N` (Not implemented) - DateTime does not support nanoseconds precision.
        /// - `%Z` (Not standard) - DotLiquid returns an offset (+00:00), Ruby states this should return a timezone description (e.g. 'GMT')
        /// <see href="https://help.shopify.com/themes/liquid/filters/additional-filters#date"/>
        /// <see href="https://ruby-doc.org/core-3.0.0/Time.html#method-i-strftime"/>
        /// </summary>
        /// <param name="timeOnly">time-only object to be formatted</param>
        /// <param name="format">the required format</param>
        /// <param name="culture">the CurrentCulture to be used when formatting</param>
        /// <returns>a string version of date-time matching pattern.</returns>
        public static string ToStrFTime(this TimeOnly timeOnly, string format, CultureInfo culture)
            => ObjectToStrFTime(timeOnly, format, culture);
#endif

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
            => ObjectToStrFTime(dateTime, format, culture);

        /// <summary>
        /// Formats a date using a ruby date format string
        /// </summary>
        /// <param name="dateTimeOffset">date-time object to be formatted</param>
        /// <param name="format">the required format</param>
        /// <param name="culture">the CurrentCulture to be used when formatting</param>
        /// <returns>a string version of date-time matching pattern.</returns>
        public static string ToStrFTime(this DateTimeOffset dateTimeOffset, string format, CultureInfo culture)
            => ObjectToStrFTime(dateTimeOffset, format, culture);

        private static string ObjectToStrFTime(object source, string format, CultureInfo culture)
        {
            culture = culture ?? throw new ArgumentException(message: Liquid.ResourceManager.GetString("DateFilterCultureRequired"), paramName: nameof(culture));

            try
            {
                return Regex.Replace(input: format, pattern: SPECIFIER_REGEX,
                    evaluator: specifier => SpecifierEvaluator(
                        specifier: specifier.Groups[0].Value,
                        flags: specifier.Groups[GROUP_FLAGS].Captures.Cast<Capture>().Select(capture => capture.Value).ToList(),
                        width: specifier.Groups[GROUP_WIDTH].Captures.Cast<Capture>().Select(capture => (int?)Convert.ToInt32(capture.Value)).FirstOrDefault(),
                        directive: specifier.Groups[GROUP_DIRECTIVE].Captures.Cast<Capture>().Select(capture => capture.Value).FirstOrDefault(),
                        source: source,
                        culture: culture
                        ));
            }
            catch (DotLiquid.Exceptions.DateFormatInvalidException)
            {
                throw new FormatException(string.Format(Liquid.ResourceManager.GetString("DateFilterFormatNotSupported"), format, source.GetType().Name));
            }
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

            if (Formats.ContainsKey(directive))
                result = Formats[directive].Invoke(source, culture);
            else if (DateFormats.ContainsKey(directive) && source is DateTimeOffset dateTimeOffset)
                result = DateFormats[directive].Invoke(dateTimeOffset.DateTime, culture);
            else if (DateFormats.ContainsKey(directive) && source is DateTime dateTime)
                result = DateFormats[directive].Invoke(dateTime, culture);
#if NET6_0_OR_GREATER
            else if (DateFormats.ContainsKey(directive) && source is DateOnly dateOnly)
                result = DateFormats[directive].Invoke(dateOnly.ToDateTime(TimeOnly.MinValue), culture);
            else if (DateFormats.ContainsKey(directive) && source is TimeOnly)
                throw new DotLiquid.Exceptions.DateFormatInvalidException();
#endif
            else
                return specifier; // This is an unconfigured specifier

            return flags.Aggregate(result, (current, flag) => ApplyFlag(flag, (width ?? 2), current));
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

        internal static string ApplyFlag(string flag, int padwidth, string str)
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
                    throw new ArgumentException(message: string.Format(Liquid.ResourceManager.GetString("DateFilterRubyDirectiveInvalid"), "ApplyFlag"), paramName: nameof(flag));
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
        internal static string GetIso8601WeekOfYear(this DateTime dateTime, string directive, CultureInfo culture)
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
                    return string.Format(culture, "{0:yyyy}", dateTime);
                case "g":
                    return string.Format(culture, "{0:yy}", dateTime);
                case "V":
                    // Return the week number of our adjusted day
                    return culture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString().PadLeft(2, '0');
                default:
                    throw new ArgumentException(message: string.Format(Liquid.ResourceManager.GetString("DateFilterRubyDirectiveInvalid"), "GetIso8601WeekOfYear"), paramName: nameof(directive));
            }
        }
    }
}
