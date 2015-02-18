using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotLiquid.Util
{
    public static class StrFTime
    {
        public delegate string DateTimeDelegate(DateTime dateTime);

        private static readonly Dictionary<string, DateTimeDelegate> Formats = new Dictionary<string, DateTimeDelegate>
        {
            { "a", (dateTime) => dateTime.ToString("ddd", CultureInfo.CurrentCulture) },
            { "A", (dateTime) => dateTime.ToString("dddd", CultureInfo.CurrentCulture) },
            { "b", (dateTime) => dateTime.ToString("MMM", CultureInfo.CurrentCulture) },
            { "B", (dateTime) => dateTime.ToString("MMMM", CultureInfo.CurrentCulture) },
            { "c", (dateTime) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", CultureInfo.CurrentCulture) },
            { "d", (dateTime) => dateTime.ToString("dd", CultureInfo.CurrentCulture) },
            { "e", (dateTime) => dateTime.ToString("%d", CultureInfo.CurrentCulture).PadLeft(2, ' ') },
            { "H", (dateTime) => dateTime.ToString("HH", CultureInfo.CurrentCulture) },
            { "I", (dateTime) => dateTime.ToString("hh", CultureInfo.CurrentCulture) },
            { "j", (dateTime) => dateTime.DayOfYear.ToString().PadLeft(3, '0') },
            { "m", (dateTime) => dateTime.ToString("MM", CultureInfo.CurrentCulture) },
            { "M", (dateTime) => dateTime.Minute.ToString().PadLeft(2, '0') },
            { "p", (dateTime) => dateTime.ToString("tt", CultureInfo.CurrentCulture) },
            { "S", (dateTime) => dateTime.ToString("ss", CultureInfo.CurrentCulture) },
            { "U", (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString().PadLeft(2, '0') },
            { "W", (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday).ToString().PadLeft(2, '0') },
            { "w", (dateTime) => ((int) dateTime.DayOfWeek).ToString() },
            { "x", (dateTime) => dateTime.ToString("d", CultureInfo.CurrentCulture) },
            { "X", (dateTime) => dateTime.ToString("T", CultureInfo.CurrentCulture) },
            { "y", (dateTime) => dateTime.ToString("yy", CultureInfo.CurrentCulture) },
            { "Y", (dateTime) => dateTime.ToString("yyyy", CultureInfo.CurrentCulture) },
            { "Z", (dateTime) => dateTime.ToString("zzz", CultureInfo.CurrentCulture) },
            { "%", (dateTime) => "%" }
        };

        public static string ToStrFTime(this DateTime dateTime, string pattern)
        {
            // This doesn't implement the ':', or '#'flags.
            return Regex.Replace(pattern, @"%(?<flag>[-_0^])*(?<width>[1-9][0-9]*)?(?<directive>[a-zA-Z%\+])",
                x => StrFTimeMatchEvaluator(
                    x.Groups[0].Value,
                    x.Groups["flag"].Captures.Cast<Capture>().Select(y => y.Value).ToList(),
                    x.Groups["width"].Captures.Cast<Capture>().Select(y => (int?) Convert.ToInt32(y.Value)).FirstOrDefault(),
                    x.Groups["directive"].Captures.Cast<Capture>().Select(y => y.Value).FirstOrDefault(),
                    dateTime
                    ));
        }

        private static String StrFTimeMatchEvaluator(String orig, IEnumerable<String> flags, int? width, String directive,
            DateTime datetime)
        {
            if (!Formats.ContainsKey(directive)) return orig;

            var result = Formats[directive].Invoke(datetime);

            return flags.ToList().Aggregate(result, (current, flag) => ApplyFlag(flag, width, current));
        }

        private static String ApplyFlag(String flag, int? padwidth, String str)
        {
            var result = str;
            switch (flag)
            {
                case "-":
                    result = str.TrimStart('0');
                    break;
                case "^":
                    result = str.ToUpper();
                    break;
                case "_":
                    result = PadLeft(str.TrimStart('0'), padwidth, ' ');
                    break;
                case "0":
                    result = PadLeft(str.TrimStart('0'), padwidth, '0');
                    break;
                // default: do nothing.
            }

            return result;
        }

        private static string PadLeft(string str, int? padwidth, char padChar)
        {
            var padCharsToAdd = (padwidth ?? 2) - str.Length;
            if (padCharsToAdd < 1)
            {
                return str;
            }
            return new String(padChar, padCharsToAdd) + str;
        }
    }
}
