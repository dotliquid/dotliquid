using System.Text.RegularExpressions;
using System.Globalization;

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
                : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
#endif
        }

        /// <summary>
        /// Converts just the first character to uppercase
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UpcaseFirst(Context context, string input)
        {
            if (input.IsNullOrWhiteSpace())
                return input;

            var trimmed = input.TrimStart();
            return input.Substring(0, input.Length - trimmed.Length) + char.ToUpper(trimmed[0]) + trimmed.Substring(1);
        }
    }
}
