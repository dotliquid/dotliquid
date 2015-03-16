using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
#if !NET35
using System.Net;
#endif
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
#if NET35
using System.Web;
#endif
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
				return ((string) input).Length;
			if (input is IEnumerable)
				return ((IEnumerable) input).Cast<object>().Count();
			return 0;
		}

        /// <summary>
        /// Return a Part of a String
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string Slice(string input, int start, int len = 1)
        {
            if (input == null || start > input.Length)
                return null;
            if (start < 0)
                start += input.Length;
            if (start + len > input.Length)
                len = input.Length - start;
            return input.Substring(start, len);
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
		/// capitalize words in the input sentence
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string Capitalize(string input)
		{
			if (input.IsNullOrWhiteSpace())
				return input;

			return string.IsNullOrEmpty(input)
				? input
				: CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
		}

		public static string Escape(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			try
			{
#if NET35
                return HttpUtility.HtmlEncode(input);
#else
				return WebUtility.HtmlEncode(input);
#endif
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


#if NET35
	/// <summary>
	/// Truncates a string down to 15 characters
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
        public static string Truncate(string input)
        {
            return Truncate(input, 15, "...");
        }

        /// <summary>
        /// Truncates a string down to <paramref name="length"/> characters
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Truncate(string input, int length)
        {
            return Truncate(input, length, "...");
        }

        /// <summary>
        /// Truncates a string down to x characters
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <param name="truncateString"></param>
        /// <returns></returns>
        public static string Truncate(string input, int length, string truncateString)
#else
		/// <summary>
		/// Truncates a string down to x characters
		/// </summary>
		/// <param name="input"></param>
		/// <param name="length"></param>
		/// <param name="truncateString"></param>
		/// <returns></returns>
		public static string Truncate(string input, int length = 50, string truncateString = "...")
#endif
		{
			if (string.IsNullOrEmpty(input))
				return input;

			int l = length - truncateString.Length;

			return input.Length > length
				? input.Substring(0, l < 0 ? 0 : l) + truncateString
				: input;
		}

#if NET35
        public static string TruncateWords(string input)
        {
            return TruncateWords(input, 15);
        }

        public static string TruncateWords(string input, int words)
        {
            return TruncateWords(input, words, "...");
        }

        public static string TruncateWords(string input, int words, string truncateString)
#else
		public static string TruncateWords(string input, int words = 15, string truncateString = "...")
#endif
		{
			if (string.IsNullOrEmpty(input))
				return input;

			var wordList = input.Split(' ').ToList();
			int l = words < 0 ? 0 : words;

			return wordList.Count > l
				? string.Join(" ", wordList.Take(l).ToArray()) + truncateString
				: input;
		}

		/// <summary>
		/// Split input string into an array of substrings separated by given pattern.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static string[] Split(string input, string pattern)
		{
			return input.IsNullOrWhiteSpace()
				? new[] { input }
				: input.Split(new[] { pattern }, StringSplitOptions.RemoveEmptyEntries);
		}

		public static string StripHtml(string input)
		{
			return input.IsNullOrWhiteSpace()
				? input
				: Regex.Replace(input, @"<.*?>", string.Empty);
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
                : Regex.Replace(input, @"(\r?\n)", String.Empty);
                
                //: Regex.Replace(input, Environment.NewLine, string.Empty);
		}

#if NET35
	/// <summary>
	/// Join elements of the array with a certain character between them
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
        public static string Join(IEnumerable input)
        {
            return Join(input, " ");
        }

        /// <summary>
        /// Join elements of the array with a certain character between them
        /// </summary>
        /// <param name="input"></param>
        /// <param name="glue"></param>
        /// <returns></returns>
        public static string Join(IEnumerable input, string glue)
#else
		/// <summary>
		/// Join elements of the array with a certain character between them
		/// </summary>
		/// <param name="input"></param>
		/// <param name="glue"></param>
		/// <returns></returns>
		public static string Join(IEnumerable input, string glue = " ")
#endif
		{
			if (input == null)
				return null;

			IEnumerable<object> castInput = input.Cast<object>();
#if NET35
            return string.Join(glue, castInput.Select(o => o.ToString()).ToArray());
#else
			return string.Join(glue, castInput);
#endif
		}

#if NET35
	/// <summary>
	/// Sort elements of the array
	/// provide optional property with which to sort an array of hashes or drops
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
        public static IEnumerable Sort(object input)
        {
            return Sort(input, null);
        }

        /// <summary>
        /// Sort elements of the array
        /// provide optional property with which to sort an array of hashes or drops
        /// </summary>
        /// <param name="input"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IEnumerable Sort(object input, string property)
#else
		/// <summary>
		/// Sort elements of the array
		/// provide optional property with which to sort an array of hashes or drops
		/// </summary>
		/// <param name="input"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static IEnumerable Sort(object input, string property = null)
#endif
		{
			List<object> ary;
			if (input is IEnumerable)
				ary = ((IEnumerable) input).Flatten().Cast<object>().ToList();
			else
				ary = new List<object>(new[] { input });
			if (!ary.Any())
				return ary;

			if (string.IsNullOrEmpty(property))
				ary.Sort();
			else if ((ary.All(o => o is IDictionary)) && ((IDictionary) ary.First()).Contains(property))
				ary.Sort((a, b) => Comparer.Default.Compare(((IDictionary) a)[property], ((IDictionary) b)[property]));
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

			if ((ary.All(o => o is IDictionary)) && ((IDictionary) ary.First()).Contains(property))
				return ary.Select(e => ((IDictionary) e)[property]);
			if (ary.All(o => o.RespondTo(property)))
				return ary.Select(e => e.Send(property));

			return ary;
		}

#if NET35
	/// <summary>
	/// Replace occurrences of a string with another
	/// </summary>
	/// <param name="input"></param>
	/// <param name="string"></param>
	/// <returns></returns>
        public static string Replace(string input, string @string)
        {
            return Replace(input, @string, " ");
        }

        /// <summary>
        /// Replace occurrences of a string with another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Replace(string input, string @string, string replacement)
#else
		/// <summary>
		/// Replace occurrences of a string with another
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string Replace(string input, string @string, string replacement = "")
#endif
		{
			if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(@string))
				return input;

			return string.IsNullOrEmpty(input)
				? input
				: Regex.Replace(input, @string, replacement);
		}

#if NET35
	/// <summary>
	/// Replace the first occurence of a string with another
	/// </summary>
	/// <param name="input"></param>
	/// <param name="string"></param>
	/// <param name="replacement"></param>
	/// <returns></returns>
        public static string ReplaceFirst(string input, string @string)
        {
            return ReplaceFirst(input, @string, "");
        }

        /// <summary>
        /// Replace the first occurence of a string with another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceFirst(string input, string @string, string replacement)
#else
		/// <summary>
		/// Replace the first occurence of a string with another
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string ReplaceFirst(string input, string @string, string replacement = "")
#endif
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
			return input.IsNullOrWhiteSpace()
				? input
				: input.Replace(@string, string.Empty);
		}

		/// <summary>
		/// Remove the first occurrence of a substring
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string RemoveFirst(string input, string @string)
		{
			return input.IsNullOrWhiteSpace()
				? input
				: ReplaceFirst(input, @string, string.Empty);
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
                    : Regex.Replace(input, @"(\r?\n)", "<br />$1");
		}

		/// <summary>
		/// Formats a date using a .NET date format string
		/// </summary>
		/// <param name="input"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static string Date(object input, string format)
		{
			if (input == null)
				return null;

			if (format.IsNullOrWhiteSpace())
				return input.ToString();

			DateTime date;

			return DateTime.TryParse(input.ToString(), out date)
				? Liquid.UseRubyDateFormat ? date.ToStrFTime(format) : date.ToString(format)
				: input.ToString();
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
		/// <param name="input"></param>
		/// <param name="operand"></param>
		/// <returns></returns>
		public static object Plus(object input, object operand)
		{
			return input is string
				? string.Concat(input, operand)
				: DoMathsOperation(input, operand, Expression.Add);
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
			return input is string && operand is int
				? Enumerable.Repeat((string) input, (int) operand)
				: DoMathsOperation(input, operand, Expression.Multiply);
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

		public static object Modulo(object input, object operand)
		{
			return DoMathsOperation(input, operand, Expression.Modulo);
		}

		private static object DoMathsOperation(object input, object operand, Func<Expression, Expression, BinaryExpression> operation)
		{
			return input == null || operand == null
				? null
				: ExpressionUtility.CreateExpression(operation, input.GetType(), operand.GetType(), input.GetType(), true)
					.DynamicInvoke(input, operand);
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