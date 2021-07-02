using DotLiquid.Exceptions;
using DotLiquid.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DotLiquid
{
    /// <summary>
    /// Tokenizer uses a mix of Regular Expressions and text parsing as part of the compilation of strings into `blocks`.
    /// </summary>
    internal static class Tokenizer
    {
        private static readonly HashSet<char> SearchSingleQuoteEnd = new HashSet<char> { '\'' };
        private static readonly HashSet<char> SearchDoubleQuoteEnd = new HashSet<char> { '"' };
        private static readonly HashSet<char> SearchQuoteOrVariableEnd = new HashSet<char> { '}', '\'', '"' };
        private static readonly HashSet<char> SearchQuoteOrTagEnd = new HashSet<char> { '%', '\'', '"' };
        private static readonly Regex LiquidAnyStartingTagRegex = R.B(R.Q(@"({0})([-])?"), Liquid.AnyStartingTag);
        private static readonly Regex TagNameRegex = R.B(R.Q(@"{0}\s*(\w+)"), Liquid.AnyStartingTag);
        private static readonly ConcurrentDictionary<string, Regex> EndTagRegexes = new ConcurrentDictionary<string, Regex>();

        /// <summary>
        /// Splits a string into an array of `tokens` that represent either a tag, object/variable, or literal string
        /// </summary>
        /// <param name="source">The Liquid Template string</param>
        /// <returns></returns>
        /// <exception cref="SyntaxException"></exception>
        internal static List<string> Tokenize(string source)
        {
            if (string.IsNullOrEmpty(source))
                return new List<string>();

            // Trim trailing whitespace.
            source = Regex.Replace(source, string.Format(@"-({0}|{1})(\n|\r\n|[ \t]+)?", Liquid.VariableEnd, Liquid.TagEnd), "$1", RegexOptions.None, Template.RegexTimeOut);

            var tokens = new List<string>();

            using (var markupEnumerator = new DotLiquid.Util.CharEnumerator(source))
            {
                var match = LiquidAnyStartingTagRegex.Match(source, markupEnumerator.Position);
                while (match.Success)
                {
                    // Check if there was a literal before the tag
                    if (match.Index > markupEnumerator.Position)
                    {
                        var tokenBeforeMatch = ReadChars(markupEnumerator, match.Index - markupEnumerator.Position);
                        if (match.Groups[2].Success)
                            tokenBeforeMatch = tokenBeforeMatch.TrimEnd(new char[] { '\t', ' ' });
                        if (tokenBeforeMatch != string.Empty)
                            tokens.Add(tokenBeforeMatch);
                    }

                    var isTag = match.Groups[1].Value == "{%";
                    // Ignore hyphen in tag name, add the tag/variable itself
                    var sb = new StringBuilder(markupEnumerator.Remaining);
                    sb.Append(match.Groups[1].Value);
                    ReadChars(markupEnumerator, match.Length);

                    // Add the parameters and tag closure
                    ReadToEnd(sb, markupEnumerator, isTag ? SearchQuoteOrTagEnd : SearchQuoteOrVariableEnd);
                    var token = sb.ToString();
                    tokens.Add(token);

                    if (isTag)
                    {
                        var tagMatch = TagNameRegex.Match(token);
                        if (tagMatch.Success)
                        {
                            var tagName = tagMatch.Groups[1].Value;
                            if (Template.IsRawTag(tagName))
                            {
                                var endTagRegex = EndTagRegexes.GetOrAdd(tagName, (key) => R.B(@"{0}-?\s*end{1}\s*-?{2}", Liquid.TagStart, key, Liquid.TagEnd));
                                var endTagMatch = endTagRegex.Match(source, markupEnumerator.Position);
                                if (!endTagMatch.Success)
                                    throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNotClosedException"), tagName);
                                if (endTagMatch.Index > markupEnumerator.Position) //add tag of everything between start and end tags
                                    tokens.Add(ReadChars(markupEnumerator, endTagMatch.Index - markupEnumerator.Position));
                            }
                        }
                    }
                    match = LiquidAnyStartingTagRegex.Match(source, markupEnumerator.Position);
                }

                if (markupEnumerator.Remaining > 0)
                    tokens.Add(ReadChars(markupEnumerator, markupEnumerator.Remaining));
            }

            return tokens;
        }

        /// <summary>
        /// Reads a fixed number of characters and advances the enumerator position
        /// </summary>
        /// <param name="markupEnumerator">The string enumerator</param>
        /// <param name="markupLength">The number of characters to read</param>
        /// <returns></returns>
        private static string ReadChars(DotLiquid.Util.CharEnumerator markupEnumerator, int markupLength)
        {
            var sb = new StringBuilder(markupLength);
            for (var i = 0; i < markupLength; i++)
                markupEnumerator.AppendNext(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Reads a tag or object variable until the end sequence, <tt>%}</tt> or <tt>}}</tt> respectively and advances the enumerator position
        /// </summary>
        /// <param name="sb">The string builder</param>
        /// <param name="markupEnumerator">The string enumerator</param>
        /// <param name="initialSearchChars">The character set to search for</param>
        /// <exception cref="SyntaxException"></exception>
        private static void ReadToEnd(StringBuilder sb, DotLiquid.Util.CharEnumerator markupEnumerator, HashSet<char> initialSearchChars)
        {
            var searchChars = initialSearchChars;
            var isInQuotes = false;

            while (markupEnumerator.AppendNext(sb))
            {
                char nextChar = markupEnumerator.Current;
                if (searchChars.Contains(nextChar))
                {
                    switch (nextChar)
                    {
                        case '\'':
                            isInQuotes = !isInQuotes;
                            searchChars = isInQuotes ? SearchSingleQuoteEnd : initialSearchChars;
                            break;
                        case '"':
                            isInQuotes = !isInQuotes;
                            searchChars = isInQuotes ? SearchDoubleQuoteEnd : initialSearchChars;
                            break;
                        case '}':
                        case '%':
                            if (markupEnumerator.Remaining > 0 && markupEnumerator.Next == '}')
                            {
                                markupEnumerator.AppendNext(sb);
                                return;
                            }
                            break;
                    }
                }
            };

            if (markupEnumerator.Remaining == 0) //Somehow we reached the end without finding the end character(s)
            {
                if (initialSearchChars == SearchQuoteOrTagEnd)
                    throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNotTerminatedException"), sb.ToString(), Liquid.TagEnd);
                else
                    throw new SyntaxException(Liquid.ResourceManager.GetString("BlockVariableNotTerminatedException"), sb.ToString(), Liquid.VariableEnd);
            }
        }
    }
}
