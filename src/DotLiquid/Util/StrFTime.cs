using System;
using System.Collections.Generic;
using System.Globalization;

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
            { "e", (dateTime) => dateTime.ToString("%d", CultureInfo.CurrentCulture).PadLeft(2, ' ') },
            { "h", (dateTime) => dateTime.ToString("MMM", CultureInfo.CurrentCulture) },
            { "H", (dateTime) => dateTime.ToString("HH", CultureInfo.CurrentCulture) },
            { "I", (dateTime) => dateTime.ToString("hh", CultureInfo.CurrentCulture) },
            { "j", (dateTime) => dateTime.DayOfYear.ToString().PadLeft(3, '0') },
            { "m", (dateTime) => dateTime.ToString("MM", CultureInfo.CurrentCulture) },
            { "k", (dateTime) => dateTime.ToString("%H", CultureInfo.CurrentCulture) },
            { "l", (dateTime) => dateTime.ToString("%h", CultureInfo.CurrentCulture) },
            { "M", (dateTime) => dateTime.Minute.ToString().PadLeft(2, '0') },
            { "P", (dateTime) => dateTime.ToString("tt", CultureInfo.CurrentCulture).ToLower() },
            { "p", (dateTime) => dateTime.ToString("tt", CultureInfo.CurrentCulture).ToUpper() },
            { "s", (dateTime) => ((int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString() },
            { "S", (dateTime) => dateTime.ToString("ss", CultureInfo.CurrentCulture) },
            { "u", (dateTime) => ((int)(dateTime.DayOfWeek) == 0 ? ((int)(dateTime).DayOfWeek) + 7 : ((int)(dateTime).DayOfWeek)).ToString() },
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

        private static readonly Dictionary<string, DateTimeOffsetDelegate> OffsetFormats = new Dictionary<string, DateTimeOffsetDelegate>
        {
            { "s", (dateTimeOffset) => ((long)(dateTimeOffset - new DateTimeOffset(1970, 1, 1, 0,0,0, TimeSpan.Zero)).TotalSeconds).ToString() },
            { "Z", (dateTimeOffset) => dateTimeOffset.ToString("zzz", CultureInfo.CurrentCulture) },
        };

        public static string ToStrFTime(this DateTime dateTime, string pattern)
        {
            string output = "";

            int n = 0;

            while (n < pattern.Length)
            {
                string s = pattern.Substring(n, 1);

                if (n + 1 >= pattern.Length)
                    output += s;
                else
                    output += s == "%"
                        ? Formats.ContainsKey(pattern.Substring(++n, 1)) ? Formats[pattern.Substring(n, 1)].Invoke(dateTime) : "%" + pattern.Substring(n, 1)
                        : s;
                n++;
            }

            return output;
        }

        /// <summary>
        /// Formats a date using a ruby date format string
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string ToStrFTime(this DateTimeOffset dateTime, string pattern)
        {
            var output = new System.Text.StringBuilder();

            for (int n = 0; n < pattern.Length; n++)
            {
                string s = pattern.Substring(n, 1);

                if (s == "%" && pattern.Length > n + 1)
                    if (OffsetFormats.ContainsKey(pattern.Substring(++n, 1)))
                        output.Append(OffsetFormats[pattern.Substring(n, 1)].Invoke(dateTime));
                    else if (Formats.ContainsKey(pattern.Substring(n, 1)))
                        output.Append(Formats[pattern.Substring(n, 1)].Invoke(dateTime.DateTime));
                    else
                        output.Append("%" + pattern.Substring(n, 1));
                else
                    output.Append(s);
            }

            return output.ToString();
        }
    }
}
