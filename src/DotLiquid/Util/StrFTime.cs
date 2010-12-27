using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Util
{
    public static class StrFTime
    {
		private static string GetSpecifierValue(DateTime dateTime, string specifier)
		{
			switch (specifier)
			{
				case "a":
					return dateTime.ToString("ddd", CultureInfo.CurrentCulture);
				case "A":
					return dateTime.ToString("dddd", CultureInfo.CurrentCulture);
				case "b":
					return dateTime.ToString("MMM", CultureInfo.CurrentCulture);
				case "B":
					return dateTime.ToString("MMMM", CultureInfo.CurrentCulture);
				case "c":
					return dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", CultureInfo.CurrentCulture);
				case "d":
					return dateTime.ToString("dd", CultureInfo.CurrentCulture);
				case "H":
					return dateTime.ToString("HH", CultureInfo.CurrentCulture);
				case "I":
					return dateTime.ToString("hh", CultureInfo.CurrentCulture);
				case "j":
					return dateTime.DayOfYear.ToString().PadLeft(3, '0');
				case "m":
					return dateTime.ToString("MM", CultureInfo.CurrentCulture);
				case "M":
					return dateTime.Minute.ToString().PadLeft(2, '0');
				case "p":
					return dateTime.ToString("tt", CultureInfo.CurrentCulture);
				case "S":
					return dateTime.ToString("ss", CultureInfo.CurrentCulture);
				case "U":
					return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString().PadLeft(2, '0');
				case "W":
					return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday).ToString().PadLeft(2, '0');
				case "w":
					return ((int)dateTime.DayOfWeek).ToString();
				case "x":
					return dateTime.ToString("d", CultureInfo.CurrentCulture);
				case "X":
					return dateTime.ToString("T", CultureInfo.CurrentCulture);
				case "y":
					return dateTime.ToString("yy", CultureInfo.CurrentCulture);
				case "Y":
					return dateTime.ToString("yyyy", CultureInfo.CurrentCulture);
				case "Z":
					return dateTime.ToString("zzz", CultureInfo.CurrentCulture);
				case "%":
					return "%";
				default:
					return "%" + specifier;
			}
		}

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
						? GetSpecifierValue(dateTime, pattern.Substring(++n, 1))
						: s;
				n++;
			}

			return output;
		}
	}
}