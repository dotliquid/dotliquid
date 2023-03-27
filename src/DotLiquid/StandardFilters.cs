using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// Standard Liquid filters
    /// </summary>
    /// <see href="https://shopify.github.io/liquid/filters/"/>
    public static class StandardFilters
    {
        private static readonly Lazy<Regex> StripHtmlBlocks = new Lazy<Regex>(() => R.C(@"<script.*?</script>|<!--.*?-->|<style.*?</style>", RegexOptions.Singleline | RegexOptions.IgnoreCase), LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<Regex> StripHtmlTags = new Lazy<Regex>(() => R.C(@"<.*?>", RegexOptions.Singleline), LazyThreadSafetyMode.ExecutionAndPublication);

#if NETSTANDARD1_3
        private class StringAwareObjectComparer : IComparer
        {
            private readonly StringComparer _stringComparer;

            public StringAwareObjectComparer(StringComparer stringComparer)
            {
                _stringComparer = stringComparer;
            }

            public int Compare(Object x, Object y)
            {
                if (x == y)
                    return 0;
                if (x == null)
                    return -1;
                if (y == null)
                    return 1;

                if (x is string textX && y is string textY)
                    return _stringComparer.Compare(textX, textY);

                return Comparer<object>.Default.Compare(x, y);
            }
        }
#endif

        /// <summary>
        /// Return the size of an array or of an string
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static int Size(object input)
        {
            if (input is string stringInput)
            {
                return stringInput.Length;
            }
            if (input is IEnumerable enumerableInput)
            {
                return enumerableInput.Cast<object>().Count();
            }
            return 0;
        }

        /// <summary>
        /// Returns a substring of one character or series of array items beginning at the index specified by the first argument.
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">The input to be sliced</param>
        /// <param name="offset">zero-based start position of string or array, negative values count back from the end of the string/array.</param>
        /// <param name="length">An optional argument specifies the length of the substring or number of array items to be returned</param>
        public static object Slice(Context context, object input, int offset, int length = 1)
        {
            if (context.SyntaxCompatibilityLevel < SyntaxCompatibility.DotLiquid22a && input is string inputString)
            {
                return SliceString(input: inputString, start: offset, len: length);
            }
            else if (input is IEnumerable enumerableInput)
            {
                var inputSize = Size(input);
                var skip = offset;
                var take = length;

                // Check if the offset is specified from the end of the string/array
                if (offset < 0)
                {
                    if (Math.Abs(offset) < inputSize)
                    {
                        skip = inputSize + offset;
                    }
                    else
                    {
                        // the required slice starts before element zero of the string/array
                        skip = 0;
                        take = inputSize + offset + length;
                    }
                }

                return enumerableInput.Cast<object>().Skip(skip).Take<object>(take);
            }

            return (context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid22a && input == null) ? string.Empty : input;
        }

        /// <summary>
        /// Return a Part of a String
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="start">start position of string</param>
        /// <param name="len">optional length of slice to be returned</param>
        private static string SliceString(string input, long start, long len)
        {
            if (input == null || start > input.Length)
                return null;

            if (start < 0)
            {
                start += input.Length;
                if (start < 0)
                {
                    len = Math.Max(0, len + start);
                    start = 0;
                }
            }
            if (start + len > input.Length)
            {
                len = input.Length - start;
            }
            return input.Substring(Convert.ToInt32(start), Convert.ToInt32(len));
        }

        /// <summary>
        /// convert a input string to DOWNCASE
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string Downcase(string input)
        {
            return input == null ? input : input.ToLower();
        }

        /// <summary>
        /// convert a input string to UPCASE
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string Upcase(string input)
        {
            return input == null
                ? input
                : input.ToUpper();
        }

        /// <summary>
        /// convert a input string to URLENCODE
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string UrlEncode(string input)
        {
            return input == null
                ? input
                : System.Net.WebUtility.UrlEncode(input);
        }

        /// <summary>
        /// convert a input string to URLDECODE
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string UrlDecode(string input)
        {
            return input == null
                ? input
                : System.Net.WebUtility.UrlDecode(input);
        }

        /// <summary>
        /// capitalize words in the input sentence
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string Capitalize(Context context, string input)
        {
            if (context.SyntaxCompatibilityLevel < SyntaxCompatibility.DotLiquid22)
            {
                if (context.SyntaxCompatibilityLevel == SyntaxCompatibility.DotLiquid21)
                    return ExtendedFilters.UpcaseFirst(context, input);
                return ExtendedFilters.Titleize(context, input);
            }

            if (input.IsNullOrWhiteSpace())
                return input;

            var trimmed = input.TrimStart();
            return input.Substring(0, input.Length - trimmed.Length) + char.ToUpper(trimmed[0]) + trimmed.Substring(1).ToLower();
        }

        /// <summary>
        /// Escape html chars
        /// </summary>
        /// <param name="input">String to escape</param>
        /// <returns>Escaped string</returns>
        /// <remarks>Alias of H</remarks>
        public static string Escape(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            try
            {
                return WebUtility.HtmlEncode(input);
            }
            catch
            {
                return input;
            }
        }

        /// <summary>
        /// Escapes a string without changing existing escaped entities.
        /// It doesn’t change strings that don’t have anything to escape.
        /// </summary>
        /// <param name="input">String to escape</param>
        /// <returns>Escaped string</returns>
        /// <see href="https://shopify.github.io/liquid/filters/escape_once/"/>
        public static string EscapeOnce(string input)
        {
            return string.IsNullOrEmpty(input) ? input : WebUtility.HtmlEncode(WebUtility.HtmlDecode(input));
        }

        /// <summary>
        /// Escape html chars
        /// </summary>
        /// <param name="input">String to escape</param>
        /// <returns>Escaped string</returns>
        /// <remarks>Alias of Escape</remarks>
        public static string H(string input)
        {
            return Escape(input);
        }

        /// <summary>
        /// Truncates a string down to x characters
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="length">optional maximum length of returned string, defaults to 50</param>
        /// <param name="truncateString">Optional suffix to append when string is truncated, defaults to ellipsis(...)</param>
        public static string Truncate(string input, int length = 50, string truncateString = "...")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (length < 0)
            {
                return truncateString;
            }

            var lengthExcludingTruncateString = length - truncateString.Length;
            return input.Length > length
                ? input.Substring(startIndex: 0, length: lengthExcludingTruncateString < 0 ? 0 : lengthExcludingTruncateString) + truncateString
                : input;
        }

        /// <summary>
        /// Truncate a string down to x words
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="words">optional maximum number of words in returned string, defaults to 15</param>
        /// <param name="truncateString">Optional suffix to append when string is truncated, defaults to ellipsis(...)</param>
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

        /// <summary>
        /// Split input string into an array of substrings separated by given pattern.
        /// </summary>
        /// <remarks>
        /// If the pattern is empty the input string is converted to an array of 1-char
        /// strings (as specified in the Liquid Reverse filter example).
        /// </remarks>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="pattern">separator string</param>
        public static string[] Split(string input, string pattern)
        {
            if (input.IsNullOrWhiteSpace())
                return new[] { input };

            // If the pattern is empty convert to an array as specified in the Liquid Reverse filter example.
            // See: https://shopify.github.io/liquid/filters/reverse/
            return string.IsNullOrEmpty(pattern)
                ? input.ToCharArray().Select(character => character.ToString()).ToArray()
                : input.Split(new[] { pattern }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Strip all html nodes from input
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string StripHtml(string input)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : StripHtmlTags.Value.Replace(StripHtmlBlocks.Value.Replace(input, string.Empty), string.Empty);
        }

        /// <summary>
        /// Strip all whitespace from input
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string Strip(string input)
        {
            return input?.Trim();
        }

        /// <summary>
        /// Strip all leading whitespace from input
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string Lstrip(string input)
        {
            return input?.TrimStart();
        }

        /// <summary>
        /// Strip all trailing whitespace from input
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string Rstrip(string input)
        {
            return input?.TrimEnd();
        }

        /// <summary>
        /// Converts the input object into a formatted currency as specified by the context cuture, or languageTag parameter (if provided).
        /// </summary>
        /// <remarks>
        /// If the input is a string it is ALWAYS parsed using the context culture, the optional languageTag parameter is only applied for rendering.
        /// </remarks>
        /// <param name="context">default source of culture information</param>
        /// <param name="input">value to be parsed and formatted as a Currency</param>
        /// <param name="languageTag">optional override language for rendering, for example 'fr-FR'</param>
        /// <seealso href="https://shopify.dev/api/liquid/filters/money-filters#money">Shopify Money filter</seealso>
        public static string Currency(Context context, object input, string languageTag = null)
        {
            // Check for null input, return null
            if (input == null) return null;

            // Check for null only, allow an empty string as it represent the InvariantCulture
            var culture = languageTag == null ? context.CurrentCulture : new CultureInfo(languageTag.Trim());

            // Attempt to convert to a currency using the context current culture.
            if (IsReal(input))
                return Convert.ToDecimal(input).ToString("C", culture);
            if (decimal.TryParse(input.ToString(), NumberStyles.Currency, context.CurrentCulture, out decimal amount))
                return amount.ToString("C", culture);

            return input.ToString();
        }

        /// <summary>
        /// Remove all newlines from the string
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string StripNewlines(string input)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : Regex.Replace(input, @"(\r?\n)", string.Empty, RegexOptions.None, Template.RegexTimeOut);
        }

        /// <summary>
        /// Join elements of the array with a certain character between them
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="glue">separator to be inserted between array elements</param>
        public static string Join(IEnumerable input, string glue = " ")
        {
            if (input == null)
                return null;

            IEnumerable<object> castInput = input.Cast<object>();

            return string.Join(glue, castInput);
        }

        /// <summary>
        /// Sort elements of the array
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">The object to sort</param>
        /// <param name="property">Optional property with which to sort an array of hashes or drops</param>
        public static IEnumerable Sort(Context context, object input, string property = null)
        {
            if (input == null)
                return null;

            if (context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid22)
                return SortInternal(StringComparer.Ordinal, input, property);
            else
                return SortInternal(StringComparer.OrdinalIgnoreCase, input, property);
        }

        /// <summary>
        /// Sort elements of the array in case-insensitive order
        /// </summary>
        /// <param name="input">The object to sort</param>
        /// <param name="property">Optional property with which to sort an array of hashes or drops</param>
        public static IEnumerable SortNatural(object input, string property = null)
        {
            if (input == null)
                return null;

            return SortInternal(StringComparer.OrdinalIgnoreCase, input, property);
        }

        private static IEnumerable SortInternal(StringComparer stringComparer, object input, string property = null)
        {
            List<object> ary;
            if (input is IEnumerable<Hash> enumerableHash && !string.IsNullOrEmpty(property))
                ary = enumerableHash.Cast<object>().ToList();
            else if (input is IEnumerable enumerableInput)
                ary = enumerableInput.Flatten().Cast<object>().ToList();
            else
            {
                ary = new List<object>(new[] { input });
            }

            if (!ary.Any())
                return ary;

#if NETSTANDARD1_3
            var comparer = new StringAwareObjectComparer(stringComparer);
#else
            var comparer = stringComparer;
#endif 

            if (string.IsNullOrEmpty(property))
            {
                ary.Sort((a, b) => comparer.Compare(a, b));
            }
            else
            {
                ary.Sort((a, b) =>
                {
                    var aPropertyValue = ResolveObjectPropertyValue(a, property);
                    var bPropertyValue = ResolveObjectPropertyValue(b, property);
                    return comparer.Compare(aPropertyValue, bPropertyValue);
                });
            }

            return ary;
        }

        /// <summary>
        /// Map/collect on a given property
        /// </summary>
        /// <param name="enumerableInput">The enumerable.</param>
        /// <param name="property">The property to map.</param>
        public static IEnumerable Map(IEnumerable enumerableInput, string property)
        {
            if (enumerableInput == null)
                return null;

            // Enumerate to a list so we can repeatedly parse through the collection.
            List<object> listedInput = enumerableInput.Cast<object>().ToList();

            // If the list happens to be empty we are done already.
            if (!listedInput.Any())
                return listedInput;

            // Note that liquid assumes that contained complex elements are all following the same schema.
            // Hence here we only check if the first element has the property requested for the map.
            if (listedInput.All(element => element is IDictionary)
                && ((IDictionary)listedInput.First()).Contains(key: property))
                return listedInput.Select(element => ((IDictionary)element)[property]);

            return listedInput.Select(element => ResolveObjectPropertyValue(element, property));
        }

        /// <summary>
        /// Replaces every occurrence of the first argument in a string with the second argument
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">Substring to be replaced</param>
        /// <param name="replacement">Replacement string to be inserted</param>
        public static string Replace(Context context, string input, string @string, string replacement = "")
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(@string))
                return input;

            if (context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid21)
                return input.Replace(@string, replacement);

            return ExtendedFilters.RegexReplace(input: input, pattern: @string, replacement: replacement);
        }

        /// <summary>
        /// Replace the first occurrence of a string with another
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">Substring to be replaced</param>
        /// <param name="replacement">Replacement string to be inserted</param>
        public static string ReplaceFirst(Context context, string input, string @string, string replacement = "")
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(@string))
                return input;

            if (context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid21)
            {
                int position = input.IndexOf(@string);
                return position < 0 ? input : input.Remove(position, @string.Length).Insert(position, replacement);
            }

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
        /// Removes every occurrence of the specified substring from a string.
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">String to be removed from input</param>
        public static string Remove(string input, string @string)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : input.Replace(@string, string.Empty);
        }

        /// <summary>
        /// Remove the first occurrence of a substring
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">String to be removed from input</param>
        public static string RemoveFirst(Context context, string input, string @string)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : ReplaceFirst(context: context, input: input, @string: @string, replacement: string.Empty);
        }

        /// <summary>
        /// Add one string to another
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">String to be added to the end of input</param>
        public static string Append(string input, string @string)
        {
            return input == null
                ? input
                : input + @string;
        }

        /// <summary>
        /// Prepend a string to another
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="string">String to be added to the beginning of input</param>
        public static string Prepend(string input, string @string)
        {
            return input == null
                ? input
                : @string + input;
        }

        /// <summary>
        /// Add <br /> tags in front of all newlines in input string
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static string NewlineToBr(string input)
        {
            return input.IsNullOrWhiteSpace()
                    ? input
                    : Regex.Replace(input, @"(\r?\n)", "<br />$1", RegexOptions.None, Template.RegexTimeOut);
        }

        /// <summary>
        /// Formats a date using a .NET date format string
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="format">Date format to be applied</param>
        /// <see cref="Liquid.UseRubyDateFormat">See UseRubyFormat for guidance on .NET vs. Ruby format support</see>
        public static string Date(Context context, object input, string format)
        {
            if (input == null)
                return null;

            if (input is DateTime date)
            {
                if (format.IsNullOrWhiteSpace())
                    return date.ToString(context.CurrentCulture);

                return context.UseRubyDateFormat
                    ? context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid21 ? new DateTimeOffset(date).ToStrFTime(format, context.CurrentCulture) : date.ToStrFTime(format, context.CurrentCulture)
                    : date.ToString(format, context.CurrentCulture);
            }

            if (context.SyntaxCompatibilityLevel == SyntaxCompatibility.DotLiquid20)
                return DateLegacyParsing(context, input.ToString(), format);

            if (format.IsNullOrWhiteSpace())
                return input.ToString();

            DateTimeOffset dateTimeOffset;
            if (input is DateTimeOffset inputOffset)
            {
                dateTimeOffset = inputOffset;
            }
            else if ((input is decimal) || (input is double) || (input is float) || (input is int) || (input is uint) || (input is long) || (input is ulong) || (input is short) || (input is ushort))
            {
#if CORE
                dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(input)).ToLocalTime();
#else
                dateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(Convert.ToDouble(input)).ToLocalTime();
#endif
            }
            else
            {
                string value = input.ToString();

                if (string.Equals(value, "now", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "today", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeOffset = DateTimeOffset.Now;
                }
                else if (!DateTimeOffset.TryParse(value, context.CurrentCulture, DateTimeStyles.None, out dateTimeOffset))
                {
                    return value;
                }
            }

            return context.UseRubyDateFormat
                ? dateTimeOffset.ToStrFTime(format, context.CurrentCulture)
                : dateTimeOffset.ToString(format, context.CurrentCulture);
        }

        private static string DateLegacyParsing(Context context, string value, string format)
        {
            DateTime date;

            if (string.Equals(value, "now", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "today", StringComparison.OrdinalIgnoreCase))
            {
                date = DateTime.Now;

                if (format.IsNullOrWhiteSpace())
                    return date.ToString(context.CurrentCulture);
            }
            else if (!DateTime.TryParse(value, context.CurrentCulture, DateTimeStyles.None, out date))
            {
                return value;
            }

            if (format.IsNullOrWhiteSpace())
                return value;

            return context.UseRubyDateFormat ? date.ToStrFTime(format, context.CurrentCulture) : date.ToString(format, context.CurrentCulture);
        }

        /// <summary>
        /// Get the first element of the passed in array
        ///
        /// Example:
        ///   {{ product.images | first | to_img }}
        /// </summary>
        /// <param name="array"></param>
        public static object First(IEnumerable array)
        {
            if (array == null)
                return null;

            return array.Cast<object>().FirstOrDefault();
        }

        /// <summary>
        /// Get the last element of the passed in array
        ///
        /// Example:
        ///   {{ product.images | last | to_img }}
        /// </summary>
        /// <param name="array"></param>
        public static object Last(IEnumerable array)
        {
            if (array == null)
                return null;

            return array.Cast<object>().LastOrDefault();
        }

        /// <summary>
        /// Addition
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="operand">Number to be added to input</param>
        public static object Plus(Context context, object input, object operand)
        {
            if (context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid21)
                return DoMathsOperation(context, input, operand, Expression.AddChecked);

            return input is string
                ? string.Concat(input, operand)
                : DoMathsOperation(context, input, operand, Expression.AddChecked);
        }

        /// <summary>
        /// Subtraction
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="operand">Number to be subtracted from input</param>
        public static object Minus(Context context, object input, object operand)
        {
            return DoMathsOperation(context, input, operand, Expression.SubtractChecked);
        }

        /// <summary>
        /// Multiplication
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="operand">Number to multiple input by</param>
        public static object Times(Context context, object input, object operand)
        {
            if (context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid21)
                return DoMathsOperation(context, input, operand, Expression.MultiplyChecked);

            return input is string && (operand is int || operand is long)
                ? Enumerable.Repeat((string)input, Convert.ToInt32(operand))
                : DoMathsOperation(context, input, operand, Expression.MultiplyChecked);
        }

        /// <summary>
        /// Rounds a decimal value to the specified places
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="places">Number of decimal places for rounding</param>
        /// <returns>The rounded value; null if an exception have occurred</returns>
        public static object Round(object input, object places = null)
        {
            try
            {
                var p = places == null ? 0 : Convert.ToInt32(places);
                var i = Convert.ToDecimal(input);
                return Math.Round(i, p);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Rounds a decimal value up to the next integer, unless already the integer value, removing all decimal places 
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <returns>The rounded value; null if an exception have occurred</returns>
        public static object Ceil(Context context, object input)
        {
            if (decimal.TryParse(input.ToString(), NumberStyles.Any, context.CurrentCulture, out decimal d))
                return Math.Ceiling(d);
            else
                return null;
        }

        /// <summary>
        /// Rounds a decimal value down to an integer, removing all decimal places 
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <returns>The rounded value; null if an exception have occurred</returns>
        public static object Floor(Context context, object input)
        {
            if (decimal.TryParse(input.ToString(), NumberStyles.Any, context.CurrentCulture, out decimal d))
                return Math.Floor(d);
            else
                return null;
        }

        /// <summary>
        /// Division
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="operand">Number to divide input by</param>
        public static object DividedBy(Context context, object input, object operand)
        {
            return DoMathsOperation(context, input, operand, Expression.Divide);
        }

        /// <summary>
        /// Performs an arithmetic remainder operation on the input
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="operand">Number to divide input by</param>
        public static object Modulo(Context context, object input, object operand)
        {
            return DoMathsOperation(context, input, operand, Expression.Modulo);
        }

        /// <summary>
        /// If a value isn't set for a variable in the template, allow the user to specify a default value for that variable
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="defaultValue">value to apply if input is nil, false or empty.</param>
        public static string Default(string input, string @defaultValue)
        {
            return !string.IsNullOrWhiteSpace(input) ? input : defaultValue;
        }

        private static bool IsReal(object o) => o is double || o is float || o is decimal;

        private static object DoMathsOperation(Context context, object input, object operand, Func<Expression, Expression, BinaryExpression> operation)
        {
            if (input == null || operand == null)
                return null;

            // NOTE(David Burg): Try for maximal precision if the input and operand fit the decimal's range.
            // This avoids rounding errors in financial arithmetic.
            // E.g.: 0.1 | Plus 10 | Minus 10 to remain 0.1, not 0.0999999999999996
            // Otherwise revert to maximum range (possible precision loss).
            var shouldConvertStrings = context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid21 && ((input is string) || (operand is string));
            if (IsReal(input) || IsReal(operand) || shouldConvertStrings)
            {
                try
                {
                    input = Convert.ToDecimal(input);
                    operand = Convert.ToDecimal(operand);

                    return ExpressionUtility
                        .CreateExpression(
                            body: operation,
                            leftType: input.GetType(),
                            rightType: operand.GetType())
                        .DynamicInvoke(input, operand);
                }
                catch (Exception ex) when (ex is OverflowException || ex is DivideByZeroException || (ex is TargetInvocationException && (ex?.InnerException is OverflowException || ex?.InnerException is DivideByZeroException)))
                {
                    input = Convert.ToDouble(input);
                    operand = Convert.ToDouble(operand);
                }
            }

            try
            {
                return ExpressionUtility
                    .CreateExpression(
                        body: operation,
                        leftType: input.GetType(),
                        rightType: operand.GetType())
                    .DynamicInvoke(input, operand);
            }
            catch (TargetInvocationException ex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        /// <summary>
        /// Removes any duplicate elements in an array.
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static IEnumerable Uniq(object input)
        {
            if (input == null)
                return null;

            List<object> ary;
            if (input is IEnumerable)
                ary = ((IEnumerable)input).Flatten().Cast<object>().ToList();
            else
            {
                ary = new List<object>(new[] { input });
            }

            if (!ary.Any())
                return ary;

            return ary.Distinct().ToList();
        }

        /// <summary>
        /// Returns the absolute value of a number.
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        public static double Abs(Context context, object input)
        {
            Double n;
            return Double.TryParse(input.ToString(), NumberStyles.Number, context.CurrentCulture, out n) ? Math.Abs(n) : 0;
        }

        /// <summary>
        /// Limits a number to a minimum value.
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="atLeast">Value to apply if more than input</param>
        public static object AtLeast(Context context, object input, object atLeast)
        {
            double n;
            var inputNumber = Double.TryParse(input.ToString(), NumberStyles.Number, context.CurrentCulture, out n);

            double min;
            var atLeastNumber = Double.TryParse(atLeast.ToString(), NumberStyles.Number, context.CurrentCulture, out min);

            if (inputNumber && atLeastNumber)
            {
                return (double)((double)min > (double)n ? min : n);
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Limits a number to a maximum value.
        /// </summary>
        /// <param name="context">The DotLiquid context</param>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <param name="atMost">Value to apply if less than input</param>
        public static object AtMost(Context context, object input, object atMost)
        {
            double n;
            var inputNumber = Double.TryParse(input.ToString(), NumberStyles.Number, context.CurrentCulture, out n);

            double max;
            var atMostNumber = Double.TryParse(atMost.ToString(), NumberStyles.Number, context.CurrentCulture, out max);

            if (inputNumber && atMostNumber)
            {
                return (double)((double)max < (double)n ? max : n);
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Removes any nil values from an array.
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        public static IEnumerable Compact(object input)
        {
            if (input == null)
                return null;

            List<object> ary;
            if (input is IEnumerable)
                ary = ((IEnumerable)input).Flatten().Cast<object>().ToList();
            else
            {
                ary = new List<object>(new[] { input });
            }

            if (!ary.Any())
                return ary;

            ary.RemoveAll(item => item == null);
            return ary;
        }

        /// <summary>
        /// Creates an array including only the objects with a given property value, or any truthy value by default.
        /// </summary>
        /// <param name="input">an array to be filtered</param>
        /// <param name="propertyName">The name of the property to filter by</param>
        /// <param name="targetValue">Value to retain, if null object containing this property are retained</param>
        public static IEnumerable Where(IEnumerable input, string propertyName, object targetValue = null)
        {
            if (input == null)
                return null;

            if (propertyName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(paramName: nameof(propertyName), message: $"'{nameof(propertyName)}' cannot be null or empty.");

            return input.Cast<object>().Where(source => source.HasMatchingProperty(propertyName, targetValue));
        }

        /// <summary>
        /// Checks if the given object has a matching property name.
        /// * If targetValue is provided, then the propertyValue is compared to targetValue
        /// * If targetValue is null, then the property is checked for "Truthyness".
        /// </summary>
        /// <param name="any">an object to be assessed</param>
        /// <param name="propertyName">The name of the property to test for</param>
        /// <param name="targetValue">target property value</param>
        private static bool HasMatchingProperty(this object any, string propertyName, object targetValue)
        {
            var propertyValue = ResolveObjectPropertyValue(any, propertyName);
            return targetValue == null || propertyValue == null
                ? propertyValue.IsTruthy()
                : propertyValue.SafeTypeInsensitiveEqual(targetValue);
        }

        private static object ResolveObjectPropertyValue(this object obj, string propertyName)
        {
            if (obj == null)
                return null;
            if (obj is IDictionary dictionary && dictionary.Contains(key: propertyName))
                return dictionary[propertyName];
            if (obj is IDictionary<string, object> dictionaryObject && dictionaryObject.ContainsKey(propertyName))
                return dictionaryObject[propertyName];
            var indexable = obj as IIndexable;
            if (indexable == null)
            {
                var type = obj.GetType();
                var safeTypeTransformer = Template.GetSafeTypeTransformer(type);
                if (safeTypeTransformer != null)
                    indexable = safeTypeTransformer(obj) as DropBase;
                else
                {
                    var liquidTypeAttribute = type
                        .GetTypeInfo()
                        .GetCustomAttributes(attributeType: typeof(LiquidTypeAttribute), inherit: false)
                        .FirstOrDefault() as LiquidTypeAttribute;
                    if (liquidTypeAttribute != null)
                    {
                        indexable = new DropProxy(obj, liquidTypeAttribute.AllowedMembers);
                    }
                    else if (TypeUtility.IsAnonymousType(type) && obj.GetType().GetRuntimeProperty(propertyName) != null)
                    {
                        return type.GetRuntimeProperty(propertyName).GetValue(obj, null);
                    }
                }
            }

            return (indexable?.ContainsKey(propertyName) ?? false) ? indexable[propertyName] : null;
        }

        /// <summary>
        /// Concatenates (joins together) multiple arrays.
        /// The resulting array contains all the items from the input arrays.
        /// </summary>
        /// <remarks>
        /// Will not remove duplicate entries from the concatenated array
        /// unless you also use the uniq filter.
        /// </remarks>
        /// <param name="left">left hand (start) of the new concatenated array</param>
        /// <param name="right">array to be appended to left</param>
        /// <see href="https://shopify.github.io/liquid/filters/concat/"/>
        public static IEnumerable Concat(IEnumerable left, IEnumerable right)
        {
            // If either side is null, return the other side.
            if (left == null)
                return right;
            else if (right == null)
                return left;

            return left.Cast<object>().ToList().Concat(right.Cast<object>());
        }

        /// <summary>
        /// Reverses the order of the items in an array. `reverse` cannot reverse a string.
        /// </summary>
        /// <param name="input">Input to be transformed by this filter</param>
        /// <see href="https://shopify.github.io/liquid/filters/reverse/"/>
        public static IEnumerable Reverse(IEnumerable input)
        {
            if (input == null || input is string)
                return input;

            var inputList = input.Cast<object>().ToList();
            inputList.Reverse();
            return inputList;
        }
    }

    internal static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrEmpty(s) || s.Trim().Length == 0;
        }
    }
}
