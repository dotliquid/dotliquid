using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotLiquid.Util
{
	internal static class R
	{
		public static string Q(string regex)
		{
			return string.Format("(?-mix:{0})", regex);
		}

		public static Regex B(string format, params string[] args)
		{
			string pattern = string.Format(format, args);

			//bool hasGAnchor;
			//pattern = RegexpTransformer.Transform(pattern, out hasGAnchor);

			return new Regex(pattern, RegexOptions.Compiled);
		}

		public static List<string> Scan(string input, Regex regex)
		{
			return regex.Matches(input)
				.Cast<Match>()
				.Select(m => (m.Groups.Count == 2) ? m.Groups[1].Value : m.Value)
				.ToList();
		}

		/// <summary>
		/// Overload that only works when the pattern contains two groups. The callback
		/// is called for each match, passing the two group values.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="pattern"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static void Scan(string input, Regex regex, Action<string, string> callback)
		{
			foreach (Match match in regex.Matches(input))
				callback(match.Groups[1].Value, match.Groups[2].Value);
		}
	}
}