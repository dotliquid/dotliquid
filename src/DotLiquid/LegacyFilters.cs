using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
    }
}
