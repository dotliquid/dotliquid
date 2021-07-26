using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// Standard Liquid filters
    /// </summary>
    /// <see href="https://shopify.github.io/liquid/filters/"/>
    public static class StandardFilters
    {

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
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// Return a Part of a String
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string Slice(string input, long start, long len = 1)
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
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Downcase(string input)
        {
            return input == null ? input : input.ToLower();
        }

        /// <summary>
        /// convert a input string to UPCASE
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Upcase(string input)
        {
            return input == null
                ? input
                : input.ToUpper();
        }

        /// <summary>
        /// convert a input string to URLENCODE
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UrlEncode(string input)
        {
            return input == null
                ? input
                : System.Net.WebUtility.UrlEncode(input);
        }

        /// <summary>
        /// convert a input string to URLDECODE
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UrlDecode(string input)
        {
            return input == null
                ? input
                : System.Net.WebUtility.UrlDecode(input);
        }

        /// <summary>
        /// capitalize words in the input sentence
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <param name="truncateString"></param>
        /// <returns></returns>
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
        /// <param name="input"></param>
        /// <param name="words"></param>
        /// <param name="truncateString"></param>
        /// <returns></returns>
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
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
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
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHtml(string input)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : Regex.Replace(input, @"<.*?>", string.Empty, RegexOptions.None, Template.RegexTimeOut);
        }

        /// <summary>
        /// Strip all whitespace from input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Strip(string input)
        {
            return input?.Trim();
        }

        /// <summary>
        /// Strip all leading whitespace from input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Lstrip(string input)
        {
            return input?.TrimStart();
        }

        /// <summary>
        /// Strip all trailing whitespace from input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Rstrip(string input)
        {
            return input?.TrimEnd();
        }

        /// <summary>
        /// Converts the input object into a formatted currency as specified by the culture info.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string Currency(object input, string cultureInfo = null)
        {

            if (decimal.TryParse(input.ToString(), out decimal amount))
            {
                if (cultureInfo.IsNullOrWhiteSpace())
                {
                    cultureInfo = CultureInfo.CurrentCulture.Name;
                }

                var culture = new CultureInfo(cultureInfo);

                return amount.ToString("C", culture);
            }

            return input.ToString();
        }

        /// <summary>
        /// Remove all newlines from the string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripNewlines(string input)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : Regex.Replace(input, @"(\r?\n)", string.Empty, RegexOptions.None, Template.RegexTimeOut);
        }

        /// <summary>
        /// Join elements of the array with a certain character between them
        /// </summary>
        /// <param name="input"></param>
        /// <param name="glue"></param>
        /// <returns></returns>
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
            else if ((ary.All(o => o is IDictionary)) && (ary.Any(o => ((IDictionary)o).Contains(property))))
            {
                ary.Sort((a, b) => comparer.Compare(((IDictionary)a)[property], ((IDictionary)b)[property]));
            }
            else if (ary.All(o => o.RespondTo(property)))
            {
                ary.Sort((a, b) => comparer.Compare(a.Send(property), b.Send(property)));
            }

            return ary;
        }

        /// <summary>
        /// Map/collect on a given property
        /// </summary>
        /// <param name="enumerableInput">The enumerable.</param>
        /// <param name="property">The property to map.</param>
        /// <returns></returns>
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

            return listedInput
                .Select(selector: element =>
                {
                    if (element == null)
                        return null;

                    var indexable = element as IIndexable;
                    if (indexable == null)
                    {
                        var type = element.GetType();
                        var safeTypeTransformer = Template.GetSafeTypeTransformer(type);
                        if (safeTypeTransformer != null)
                            indexable = safeTypeTransformer(element) as DropBase;
                        else
                        {
                            var liquidTypeAttribute = type
                                .GetTypeInfo()
                                .GetCustomAttributes(attributeType: typeof(LiquidTypeAttribute), inherit: false)
                                .FirstOrDefault() as LiquidTypeAttribute;
                            if (liquidTypeAttribute != null)
                            {
                                indexable = new DropProxy(element, liquidTypeAttribute.AllowedMembers);
                            }
                            else if (TypeUtility.IsAnonymousType(type))
                            {
                                return element.RespondTo(property) ? element.Send(property) : element;
                            }
                        }
                    }

                    return (indexable?.ContainsKey(property) ?? false) ? indexable[property] : null;
                });
        }

        /// <summary>
        /// Replace occurrences of a string with another
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Replace(Context context, string input, string @string, string replacement = "")
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(@string))
                return input;

            if (context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid21)
                return input.Replace(@string, replacement);

            return Regex.Replace(input, @string, replacement, RegexOptions.None, Template.RegexTimeOut);
        }

        /// <summary>
        /// Replace the first occurence of a string with another
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
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
        /// Remove a substring
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Remove(string input, string @string)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : input.Replace(@string, string.Empty);
        }

        /// <summary>
        /// Remove the first occurrence of a substring
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string RemoveFirst(Context context, string input, string @string)
        {
            return input.IsNullOrWhiteSpace()
                ? input
                : ReplaceFirst(context: context, input: input, @string: @string, replacement: string.Empty);
        }

        /// <summary>
        /// Add one string to another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Append(string input, string @string)
        {
            return input == null
                ? input
                : input + @string;
        }

        /// <summary>
        /// Prepend a string to another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Prepend(string input, string @string)
        {
            return input == null
                ? input
                : @string + input;
        }

        /// <summary>
        /// Add <br /> tags in front of all newlines in input string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NewlineToBr(string input)
        {
            return input.IsNullOrWhiteSpace()
                    ? input
                    : Regex.Replace(input, @"(\r?\n)", "<br />$1", RegexOptions.None, Template.RegexTimeOut);
        }

        /// <summary>
        /// Formats a date using a .NET date format string
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Date(Context context, object input, string format)
        {
            if (input == null)
                return null;

            if (input is DateTime date)
            {
                if (format.IsNullOrWhiteSpace())
                    return date.ToString();

                return Liquid.UseRubyDateFormat
                    ? context.SyntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid21 ? new DateTimeOffset(date).ToStrFTime(format) : date.ToStrFTime(format)
                    : date.ToString(format);
            }

            if (context.SyntaxCompatibilityLevel == SyntaxCompatibility.DotLiquid20)
                return DateLegacyParsing(input.ToString(), format);

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
                else if (!DateTimeOffset.TryParse(value, out dateTimeOffset))
                {
                    return value;
                }
            }

            return Liquid.UseRubyDateFormat ? dateTimeOffset.ToStrFTime(format) : dateTimeOffset.ToString(format);
        }

        private static string DateLegacyParsing(string value, string format)
        {
            DateTime date;

            if (string.Equals(value, "now", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "today", StringComparison.OrdinalIgnoreCase))
            {
                date = DateTime.Now;

                if (format.IsNullOrWhiteSpace())
                    return date.ToString();
            }
            else if (!DateTime.TryParse(value, out date))
            {
                return value;
            }

            if (format.IsNullOrWhiteSpace())
                return value;

            return Liquid.UseRubyDateFormat ? date.ToStrFTime(format) : date.ToString(format);
        }

        /// <summary>
        /// Get the first element of the passed in array
        ///
        /// Example:
        ///   {{ product.images | first | to_img }}
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        public static object Last(IEnumerable array)
        {
            if (array == null)
                return null;

            return array.Cast<object>().LastOrDefault();
        }

        /// <summary>
        /// Addition
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
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
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Minus(Context context, object input, object operand)
        {
            return DoMathsOperation(context, input, operand, Expression.SubtractChecked);
        }

        /// <summary>
        /// Multiplication
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
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
        /// <param name="input"></param>
        /// <param name="places"></param>
        /// <returns>The rounded value; null if an exception have occured</returns>
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
        /// <param name="input"></param>
        /// <returns>The rounded value; null if an exception have occured</returns>
        public static object Ceil(object input)
        {
            if (decimal.TryParse(input.ToString(), out decimal d))
                return Math.Ceiling(d);
            else
                return null;
        }

        /// <summary>
        /// Rounds a decimal value down to an integer, removing all decimal places 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The rounded value; null if an exception have occured</returns>
        public static object Floor(object input)
        {
            if (decimal.TryParse(input.ToString(), out decimal d))
                return Math.Floor(d);
            else
                return null;
        }

        /// <summary>
        /// Division
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object DividedBy(Context context, object input, object operand)
        {
            return DoMathsOperation(context, input, operand, Expression.Divide);
        }

        /// <summary>
        /// Performs an arithmetic remainder operation on the input
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Modulo(Context context, object input, object operand)
        {
            return DoMathsOperation(context, input, operand, Expression.Modulo);
        }

        /// <summary>
        /// If a value isn't set for a variable in the template, allow the user to specify a default value for that variable
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
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
            // This avoids rounding errors in financial arithmetics.
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
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// <param name="input"></param>
        /// <returns></returns>
        public static double Abs(object input)
        {
            Double n;
            return Double.TryParse(input.ToString(), System.Globalization.NumberStyles.Number, CultureInfo.CurrentCulture, out n) ? Math.Abs(n) : 0;
        }

        /// <summary>
        /// Limits a number to a minimum value.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="atLeast"></param>
        /// <returns></returns>
        public static object AtLeast(object input, object atLeast)
        {
            double n;
            var inputNumber = Double.TryParse(input.ToString(), System.Globalization.NumberStyles.Number, CultureInfo.CurrentCulture, out n);

            double min;
            var atLeastNumber = Double.TryParse(atLeast.ToString(), System.Globalization.NumberStyles.Number, CultureInfo.CurrentCulture, out min);

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
        /// <param name="input"></param>
        /// <param name="atMost"></param>
        /// <returns></returns>
        public static object AtMost(object input, object atMost)
        {
            double n;
            var inputNumber = Double.TryParse(input.ToString(), System.Globalization.NumberStyles.Number, CultureInfo.CurrentCulture, out n);

            double max;
            var atMostNumber = Double.TryParse(atMost.ToString(), System.Globalization.NumberStyles.Number, CultureInfo.CurrentCulture, out max);

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
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// <returns></returns>
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
        /// <returns></returns>
        private static bool HasMatchingProperty(this object any, string propertyName, object targetValue)
        {
            // Check if the 'any' object has a propertyName
            object propertyValue = null;
            if (any is IDictionary dictionary && dictionary.Contains(key: propertyName))
            {
                propertyValue = dictionary[propertyName];
            }
            else if (any != null && any.RespondTo(propertyName))
            {
                propertyValue = any.Send(propertyName);
            }

            return targetValue == null || propertyValue == null
                ? propertyValue.IsTruthy()
                : propertyValue.SafeTypeInsensitiveEqual(targetValue);
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
        /// <param name="input"/>
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
