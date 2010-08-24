using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;
using DotLiquid.Util;

namespace DotLiquid
{
    public static class StandardFilters
    {
        /// <summary>
        /// Return the size of an array or of an string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int Size(object input)
        {
            if (input is string)
                return ((string)input).Length;
            if (input is IEnumerable)
                return ((IEnumerable)input).Cast<object>().Count();
            return 0;
        }

        /// <summary>
        /// convert a input string to DOWNCASE
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Downcase(string input)
        {
            if (input == null)
                return string.Empty;
            return input.ToLower();
        }

        /// <summary>
        /// convert a input string to UPCASE
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Upcase(string input)
        {
            if (input == null)
                return string.Empty;
            return input.ToUpper();
        }

        /// <summary>
        /// capitalize words in the input sentence
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Capitalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string result = char.ToUpper(input[0]).ToString();
            if (input.Length > 1)
                result += input.Substring(1);
            return result;
        }

        public static string Escape(string input)
        {
            try
            {
                return HttpUtility.HtmlEncode(input);
            }
            catch
            {
                return input;
            }
        }

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
                return input;
            int l = length - truncateString.Length;
            if (l < 0)
                l = 0;
            return (input.Length > length)
                ? input.Substring(0, l) + truncateString
                : input;
        }

        public static string TruncateWords(string input, int words = 15, string truncateString = "...")
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string[] wordList = input.Split(' ');
            int l = words;
            if (l < 0)
                l = 0;
            return (wordList.Length > l)
                ? string.Join(" ", wordList.Take(l)) + truncateString
                : input;
        }

        public static string StripHtml(string input)
        {
            if (input == null)
                return string.Empty;
            return Regex.Replace(input, @"<.*?>", string.Empty);
        }

        /// <summary>
        /// Remove all newlines from the string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripNewlines(string input)
        {
            return Regex.Replace(input, Environment.NewLine, string.Empty);
        }

        /// <summary>
        /// Join elements of the array with a certain character between them
        /// </summary>
        /// <param name="input"></param>
        /// <param name="glue"></param>
        /// <returns></returns>
        public static string Join(IEnumerable input, string glue = " ")
        {
            IEnumerable<object> castInput = input.Cast<object>();
            return string.Join(glue, castInput);
        }

        /// <summary>
        /// Sort elements of the array
        /// provide optional property with which to sort an array of hashes or drops
        /// </summary>
        /// <param name="input"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IEnumerable Sort(object input, string property = null)
        {
            List<object> ary;
            if (input is IEnumerable)
                ary = ((IEnumerable)input).Flatten().Cast<object>().ToList();
            else
                ary = new List<object>(new[] { input });
            if (!ary.Any())
                return ary;

            if (string.IsNullOrEmpty(property))
                ary.Sort();
            else if ((ary.All(o => o is IDictionary)) && ((IDictionary)ary.First()).Contains(property))
                ary.Sort((a, b) => Comparer.Default.Compare(((IDictionary)a)[property], ((IDictionary)b)[property]));
            else if (ary.All(o => o.RespondTo(property)))
                ary.Sort((a, b) => Comparer.Default.Compare(a.Send(property), b.Send(property)));

            return ary;
        }

        /// <summary>
        /// Map/collect on a given property
        /// </summary>
        /// <param name="input"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IEnumerable Map(IEnumerable input, string property)
        {
            List<object> ary = input.Cast<object>().ToList();
            if (!ary.Any())
                return ary;

            if ((ary.All(o => o is IDictionary)) && ((IDictionary)ary.First()).Contains(property))
                return ary.Select(e => ((IDictionary)e)[property]);
            if (ary.All(o => o.RespondTo(property)))
                return ary.Select(e => e.Send(property));

            return ary;
        }

        /// <summary>
        /// Replace occurrences of a string with another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Replace(string input, string @string, string replacement = "")
        {
            return Regex.Replace(input, @string, replacement);
        }

        /// <summary>
        /// Replace the first occurence of a string with another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceFirst(string input, string @string, string replacement = "")
        {
            bool doneReplacement = false;
            return Regex.Replace(input, @string, m =>
            {
                if (doneReplacement)
                    return m.Value;

                doneReplacement = true;
                return replacement;
            });
        }

        /// <summary>
        /// Remove a substring
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Remove(string input, string @string)
        {
            return input.Replace(@string, string.Empty);
        }

        /// <summary>
        /// Remove the first occurrence of a substring
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string RemoveFirst(string input, string @string)
        {
            return ReplaceFirst(input, @string, string.Empty);
        }

        /// <summary>
        /// Add one string to another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Append(string input, string @string)
        {
            return input + @string;
        }

        /// <summary>
        /// Prepend a string to another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Prepend(string input, string @string)
        {
            return @string + input;
        }

        /// <summary>
        /// Add <br /> tags in front of all newlines in input string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NewlineToBr(string input)
        {
            return Regex.Replace(input, Environment.NewLine, "<br />" + Environment.NewLine);
        }

        /// <summary>
        /// Formats a date using a .NET date format string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Date(string input, string format)
        {
            try
            {
                DateTime date = DateTime.Parse(input);
                return Date(date, format);
            }
            catch
            {
                return input;
            }
        }

        /// <summary>
        /// Formats a date using a .NET date format string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Date(DateTime input, string format)
        {
            return input.ToString(format);
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
            return array.Cast<object>().LastOrDefault();
        }

        /// <summary>
        /// Addition
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Plus(object input, object operand)
        {
            if (input is string)
                return string.Concat(input, operand);
            return DoMathsOperation(input, operand, Expression.Add);
        }

        /// <summary>
        /// Subtraction
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Minus(object input, object operand)
        {
            return DoMathsOperation(input, operand, Expression.Subtract);
        }

        /// <summary>
        /// Multiplication
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Times(object input, object operand)
        {
            if (input is string && operand is int)
                return Enumerable.Repeat((string)input, (int)operand);
            return DoMathsOperation(input, operand, Expression.Multiply);
        }

        /// <summary>
        /// Division
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object DividedBy(object input, object operand)
        {
            return DoMathsOperation(input, operand, Expression.Divide);
        }

        private static object DoMathsOperation(object input, object operand, Func<Expression, Expression, BinaryExpression> operation)
        {
            if (input == null || operand == null)
                return null;

            return ExpressionUtility.CreateExpression(operation, input.GetType(), operand.GetType(), input.GetType(), true)
                .DynamicInvoke(input, operand);
        }
    }
}