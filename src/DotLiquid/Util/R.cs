using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotLiquid.Util
{
	public static class R
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

			return R.C(pattern);
		}

		/// <summary>
		/// All regexes in DotLiquid use fixed known patterns and are used repeatedly, many times repeatedly within a single template.
		/// Compiled regexes should be used in these cases to avoid calling the Regex constructor needlessly.
		/// The .NET Regex constructor contains a static cache lookup that requires acquisition of a lock, so in a multithreaded system
		/// under high load, there will be severe lock contention if regexes are constantly being constructed by different threads.
		/// Using compiled regexes and making them "static readonly" completely avoids this problem.
		/// In recent versions of .NET, compiled regexes also generally perform measurably faster than uncompiled ones when used repeatedly.
		/// </summary>
		/// <param name="pattern">regex pattern</param>
		/// <param name="options">regex options; use the default (Compiled) unless there is a good reason not to</param>
		/// <returns>the regex</returns>
		public static Regex C(string pattern, RegexOptions options = RegexOptions.Compiled)
		{
			return new Regex(pattern, options);
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
		public static void Scan(string input, string pattern, Action<string, string> callback)
		{
			foreach (Match match in Regex.Matches(input, pattern))
				callback(match.Groups[1].Value, match.Groups[2].Value);
		}
	}
}