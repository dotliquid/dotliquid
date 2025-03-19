using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotLiquid
{
    /// <summary>
    /// Extended DotLiquid Filters. Not registered by default.
    /// </summary>
    public static class ExtendedFilters
    {
        /// <summary>
        /// Capitalize all the words in the input sentence
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Titleize(Context context, string input)
        {
            return input.IsNullOrWhiteSpace()
                ? input
#if CORE
                : Regex.Replace(input, @"\b(\w)", m => m.Value.ToUpper(), RegexOptions.None, Template.RegexTimeOut);
#else
                : context.CurrentCulture.TextInfo.ToTitleCase(input);
#endif
        }

        /// <summary>
        /// Converts just the first character to uppercase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UpcaseFirst(string input)
        {
            if (input.IsNullOrWhiteSpace())
                return input;

            var trimmed = input.TrimStart();
            return input.Substring(0, input.Length - trimmed.Length) + char.ToUpper(trimmed[0]) + trimmed.Substring(1);
        }

        /// <summary>
        /// Replaces all strings that match a specified regular expression with a specified replacement string.
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="replacement">The replacement string</param>
        public static string RegexReplace(string input, string pattern, string replacement = "")
        {
            return Regex.Replace(input: input, pattern: pattern, replacement: replacement, options: RegexOptions.None, matchTimeout: Template.RegexTimeOut);
        }

        /// <summary>
        /// Split input string into an array of substrings separated by given pattern.
        /// </summary>
        /// <remarks>
        /// <para>If <paramref name="input"/> is null or empty, an empty array is returned.</para>
        /// <para>If <paramref name="pattern"/> is null or empty, the input string is converted to an array of single-character strings.</para>
        /// <para>If <paramref name="pattern"/> is a space, the input string is split using any whitespace character, and all empty entries are removed.</para>
        /// <para>If <paramref name="pattern"/> is any other value, the input string is split using the pattern, and empty entries at the end are removed.</para>
        /// </remarks>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="pattern">separator string</param>
        public static string[] RubySplit(string input, string pattern)
        {
            if (string.IsNullOrEmpty(input))
                return new string[] { };

            // If the pattern is empty convert to an array as specified in the Liquid Reverse filter example.
            // See: https://shopify.github.io/liquid/filters/reverse/
            if (string.IsNullOrEmpty(pattern))
                return input.ToCharArray().Select(character => character.ToString()).ToArray();

            // Ruby docs: If pattern is a single space, str is split on whitespace, with leading and trailing whitespace and runs of contiguous whitespace characters ignored.
            if (pattern == " ")
                return input.Split(Liquid.AsciiWhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            // Ruby docs: When field_sep is a string different from ' ' and limit is 0, the split occurs at each occurrence of field_sep; trailing empty substrings are not returned.
            var parts = input.Split(new[] { pattern }, StringSplitOptions.None);
            int indexTillTrailingEmpty = parts.Length;

            while (indexTillTrailingEmpty > 0 && string.IsNullOrEmpty(parts[indexTillTrailingEmpty - 1]))
            {
                indexTillTrailingEmpty--;
            }

            return indexTillTrailingEmpty < parts.Length ? parts.Take(indexTillTrailingEmpty).ToArray() : parts;
        }
    }
}
