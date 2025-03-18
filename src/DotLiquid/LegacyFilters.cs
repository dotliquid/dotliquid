using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DotLiquid
{
    /// <summary>
    /// A collection of filters implementing the behavior of a previous version of DotLiquid
    /// </summary>
    public static class LegacyFilters
    {
        /// <summary>
        /// capitalize words in the input sentence
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid20)]
        public static string Capitalize(Context context, string input) => ExtendedFilters.Titleize(context, input);

        /// <summary>
        /// capitalize words in the input sentence
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        [LiquidFilter(Name = nameof(Capitalize), MinVersion = SyntaxCompatibility.DotLiquid21, MaxVersion = SyntaxCompatibility.DotLiquid21)]
        public static string CapitalizeV21(string input) => ExtendedFilters.UpcaseFirst(input);

        /// <summary>
        /// Sort elements of the array
        /// </summary>
        /// <param name="input">The object to sort</param>
        /// <param name="property">Optional property with which to sort an array of hashes or drops</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid21)]
        public static IEnumerable Sort(object input, string property = null) => StandardFilters.SortInternal(StringComparer.OrdinalIgnoreCase, input, property);

        /// <summary>
        /// Remove the first occurrence of a substring
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">String to be removed from input</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid21)]
        public static string RemoveFirst(string input, string @string)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : ReplaceFirst(input: input, @string: @string, replacement: string.Empty);
        }

        /// <summary>
        /// Replaces every occurrence of the first argument in a string with the second argument
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">Substring to be replaced</param>
        /// <param name="replacement">Replacement string to be inserted</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid20)]
        public static string Replace(string input, string @string, string replacement = "")
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(@string))
                return input;

            return ExtendedFilters.RegexReplace(input: input, pattern: @string, replacement: replacement);
        }

        /// <summary>
        /// Replace the first occurrence of a string with another
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">Substring to be replaced</param>
        /// <param name="replacement">Replacement string to be inserted</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid21)]
        public static string ReplaceFirst(string input, string @string, string replacement = "")
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(@string))
                return input;

            bool doneReplacement = false;
            return Regex.Replace(input, @string, m =>
            {
                if (doneReplacement)
                    return m.Value;

                doneReplacement = true;
                return replacement;
            }, RegexOptions.None, Template.RegexTimeOut);
        }            

        /// <summary>
        /// Addition
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="operand">Number to be added to input</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid20)]
        public static object Plus(Context context, object input, object operand)
        {
            return input is string
                ? string.Concat(input, operand)
                : StandardFilters.DoMathsOperation(context, input, operand, Expression.AddChecked);
        }

        /// <summary>
        /// Return a Part of a String
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="start">start position of string</param>
        /// <param name="len">optional length of slice to be returned</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid22)]
        public static object Slice(object input, int start, int len = 1)
        {
            if (input == null)
                return null;

            if (input is string inputString)
            {
                if (start > inputString.Length)
                    return null;

                if (start < 0)
                {
                    start += inputString.Length;
                    if (start < 0)
                    {
                        len = Math.Max(0, len + start);
                        start = 0;
                    }
                }
                if (start + len > inputString.Length)
                {
                    len = inputString.Length - start;
                }
                return inputString.Substring(Convert.ToInt32(start), Convert.ToInt32(len));
            }
            else if (input is IEnumerable enumerableInput)
            {
                return StandardFilters.Slice(input, start, len);
            }

            return input;
        }

        /// <summary>
        /// Split input string into an array of substrings separated by given pattern.
        /// </summary>
        /// <remarks>
        /// If the pattern is empty the input string is converted to an array of 1-char
        /// strings (as specified in the Liquid Reverse filter example).
        /// </remarks>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="pattern">separator string</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid22a)]
        public static string[] Split(string input, string pattern)
        {
            if (input.IsNullOrWhiteSpace())
                return new[] { input };

            return StandardFilters.Split(input, pattern);
        }

        /// <summary>
        /// Multiplication
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="operand">Number to multiple input by</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid20)]
        public static object Times(Context context, object input, object operand)
        {
            return input is string @string && (operand is int || operand is long)
                ? Enumerable.Repeat(@string, Convert.ToInt32(operand))
                : StandardFilters.DoMathsOperation(context, input, operand, Expression.MultiplyChecked);
        }

        /// <summary>
        /// Truncate a string down to x words
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="words">optional maximum number of words in returned string, defaults to 15</param>
        /// <param name="truncateString">Optional suffix to append when string is truncated, defaults to ellipsis(...)</param>
        [LiquidFilter(MaxVersion = SyntaxCompatibility.DotLiquid22a)]
        public static string TruncateWords(string input, int words = 15, string truncateString = "...")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (words <= 0)
            {
                return truncateString;
            }

            var wordArray = input.Split(' ');
            return wordArray.Length > words
                ? string.Join(separator: " ", values: wordArray.Take(words)) + truncateString
                : input;
        }
    }
}
