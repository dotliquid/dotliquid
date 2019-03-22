using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotLiquid.Util
{
    public static class StrFTime
    {
        public delegate string DateTimeDelegate(DateTime dateTime);

        private static readonly Dictionary<char, DateTimeDelegate> Formats = new Dictionary<char, DateTimeDelegate>
        {
            { 'a', (dateTime) => dateTime.ToString("ddd", CultureInfo.CurrentCulture) },
            { 'A', (dateTime) => dateTime.ToString("dddd", CultureInfo.CurrentCulture) },
            { 'b', (dateTime) => dateTime.ToString("MMM", CultureInfo.CurrentCulture) },
            { 'B', (dateTime) => dateTime.ToString("MMMM", CultureInfo.CurrentCulture) },
            { 'c', (dateTime) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", CultureInfo.CurrentCulture) },
			{ 'C', (dateTime) => ((int)Math.Floor(Convert.ToDouble(dateTime.ToString("yyyy"))/100)).ToString() },
            { 'd', (dateTime) => dateTime.ToString("dd", CultureInfo.CurrentCulture) },
            { 'e', (dateTime) => dateTime.ToString("%d", CultureInfo.CurrentCulture).PadLeft(2, ' ') },
			{ 'h', (dateTime) => dateTime.ToString("MMM", CultureInfo.CurrentCulture) },
            { 'H', (dateTime) => dateTime.ToString("HH", CultureInfo.CurrentCulture) },
            { 'I', (dateTime) => dateTime.ToString("hh", CultureInfo.CurrentCulture) },
            { 'j', (dateTime) => dateTime.DayOfYear.ToString().PadLeft(3, '0') },
            { 'm', (dateTime) => dateTime.ToString("MM", CultureInfo.CurrentCulture) },
			{ 'k', (dateTime) => dateTime.ToString("%H", CultureInfo.CurrentCulture) },
			{ 'l', (dateTime) => dateTime.ToString("%h", CultureInfo.CurrentCulture) },
            { 'M', (dateTime) => dateTime.Minute.ToString().PadLeft(2, '0') },
			{ 'P', (dateTime) => dateTime.ToString("tt", CultureInfo.CurrentCulture).ToLower() },
            { 'p', (dateTime) => dateTime.ToString("tt", CultureInfo.CurrentCulture).ToUpper() },
			{ 's', (dateTime) => ((int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString() },
            { 'S', (dateTime) => dateTime.ToString("ss", CultureInfo.CurrentCulture) },
			{ 'u', (dateTime) => ((int)(dateTime.DayOfWeek) == 0 ? ((int)(dateTime).DayOfWeek) + 7 : ((int)(dateTime).DayOfWeek)).ToString() },
            { 'U', (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString().PadLeft(2, '0') },
            { 'W', (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday).ToString().PadLeft(2, '0') },
            { 'w', (dateTime) => ((int) dateTime.DayOfWeek).ToString() },
            { 'x', (dateTime) => dateTime.ToString("d", CultureInfo.CurrentCulture) },
            { 'X', (dateTime) => dateTime.ToString("T", CultureInfo.CurrentCulture) },
            { 'y', (dateTime) => dateTime.ToString("yy", CultureInfo.CurrentCulture) },
            { 'Y', (dateTime) => dateTime.ToString("yyyy", CultureInfo.CurrentCulture) },
            { 'Z', (dateTime) => dateTime.ToString("zzz", CultureInfo.CurrentCulture) },
            { '%', (dateTime) => "%" }
        };

        public static string ToStrFTime(this DateTime dateTime, string pattern)
        {
            StringBuilder output = new StringBuilder();

            int n = 0;

            while (n < pattern.Length)
            {
                char c = pattern[n];

                if (n + 1 >= pattern.Length)
                    output.Append(c);
                else if (c == '%')
                {
                    if (Formats.TryGetValue(pattern[++n], out DateTimeDelegate fDateTime))
                        output.Append(fDateTime(dateTime));
                    else
                    {
                        output.Append('%');
                        output.Append(pattern[n]);
                    }
                }
                else
                    output.Append(c);
                n++;
            }
            return output.ToString();
        }
    }
}
