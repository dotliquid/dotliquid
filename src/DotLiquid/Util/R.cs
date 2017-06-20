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
            return C(string.Format(format, args));
        }

        /// <summary>
        /// All regexes in DotLiquid use fixed known patterns and are used repeatedly, many times repeatedly within a single template.
        /// Compiled regexes should be used in these cases to avoid calling the Regex constructor needlessly.
        /// The .NET Regex constructor contains a static cache lookup that requires acquisition of a lock, so in a multithreaded system
        /// under high load, there will be severe lock contention if regexes are constantly being constructed by different threads.
        /// Using compiled regexes and making them "static readonly" completely avoids this problem.
        /// In recent versions of .NET, compiled regexes also generally perform measurably faster than uncompiled ones (when used a large number of times).
        /// There is of course an initial cost for compilation, but the benefits for high-scale applications far outweigh the initial cost and
        /// low-scale applications are unlikely to care about the difference anyway.
        /// </summary>
        /// <param name="pattern">regex pattern</param>
        /// <param name="options">regex options; use the default (Compiled) unless there is a good reason not to</param>
        /// <returns>the regex</returns>
        public static Regex C(
            string pattern,
#if CORE
            // Compiled Regex are not available under .NET Core
            RegexOptions options = RegexOptions.None
#else
            RegexOptions options = RegexOptions.Compiled
#endif
            )
        {
            var regex = new Regex(pattern, options, Template.RegexTimeOut);

            // execute once to trigger the lazy compilation (not strictly necessary, but avoids the first real execution taking a longer time than subsequent ones)
            regex.IsMatch(string.Empty);

            return regex;
        }

        /// <summary>
        /// Scan the input text finding all matches for the given regex.
        /// Passing in the regex instead of the pattern avoids problems with Regex construction.
        /// See associated comments above for the C() method.
        /// </summary>
        /// <param name="input">input text</param>
        /// <param name="regex">regex</param>
        /// <returns>matches</returns>
        public static List<string> Scan(string input, Regex regex)
        {
            return regex.Matches(input)
                .Cast<Match>()
                .Select(m => (m.Groups.Count == 2) ? m.Groups[1].Value : m.Value)
                .ToList();
        }

        /// <summary>
        /// Deprecated for performance reasons. New code should not use this.
        /// See comments for Scan(string, Regex) above.
        /// </summary>
        /// <param name="input">input text</param>
        /// <param name="pattern">regex pattern</param>
        /// <returns>matches</returns>
        [Obsolete("Use Scan(string, Regex) instead.")]
        public static List<string> Scan(string input, string pattern)
        {
            return Scan(input, new Regex(pattern, RegexOptions.None, Template.RegexTimeOut));
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
            foreach (Match match in Regex.Matches(input, pattern, RegexOptions.None, Template.RegexTimeOut))
                callback(match.Groups[1].Value, match.Groups[2].Value);
        }
    }
}
