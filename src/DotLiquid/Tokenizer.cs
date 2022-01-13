using DotLiquid.Exceptions;
using DotLiquid.Util;
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
        private static readonly HashSet<char> SearchVariableEnd = new HashSet<char> { '[', '.' };
        private static readonly char BracketEnd = ']';
        private static readonly HashSet<char> SearchQuoteOrVariableEnd = new HashSet<char> { '}', '\'', '"' };
        private static readonly HashSet<char> SearchQuoteOrTagEnd = new HashSet<char> { '%', '\'', '"' };
        private static readonly char[] WhitespaceCharsV20 = new char[] { '\t', ' ' };
        private static readonly char[] WhitespaceCharsV22 = new char[] { '\t', '\n', '\v', '\f', '\r', ' ' };
        private static readonly Regex LiquidAnyStartingTagRegex = R.B(R.Q(@"({0})([-])?"), Liquid.AnyStartingTag);
        private static readonly Regex TagNameRegex = R.B(R.Q(@"{0}\s*(\w+)"), Liquid.AnyStartingTag);
        private static readonly Regex VariableSegmentRegex = R.C(Liquid.VariableSegment);
        private static readonly ConcurrentDictionary<string, Regex> EndTagRegexes = new ConcurrentDictionary<string, Regex>();

        /// <summary>
        /// Splits a string into an array of `tokens` that represent either a tag, object/variable, or literal string
        /// </summary>
        /// <param name="source">The Liquid Template string</param>
        /// <param name="syntaxCompatibilityLevel">The Liquid syntax flag used for backward compatibility</param>
        /// <exception cref="SyntaxException"></exception>
        internal static List<string> Tokenize(string source, SyntaxCompatibility syntaxCompatibilityLevel)
        {
            if (string.IsNullOrEmpty(source))
                return new List<string>();

            // Trim leading whitespace - backward compatible list of chars
            var whitespaceChars = syntaxCompatibilityLevel < SyntaxCompatibility.DotLiquid22 ? WhitespaceCharsV20 : WhitespaceCharsV22;

            // Trim trailing whitespace - new lines or spaces/tabs but not both
            if (syntaxCompatibilityLevel < SyntaxCompatibility.DotLiquid22)
            {
                source = DotLiquid.Tags.Literal.FromShortHand(source);
                source = DotLiquid.Tags.Comment.FromShortHand(source);
                source = Regex.Replace(source, string.Format(@"-({0}|{1})(\n|\r\n|[ \t]+)?", Liquid.VariableEnd, Liquid.TagEnd), "$1", RegexOptions.None, Template.RegexTimeOut);
            }

            var tokens = new List<string>();

            using (var markupEnumerator = new CharEnumerator(source))
            {
                var match = LiquidAnyStartingTagRegex.Match(source, markupEnumerator.Position);
                while (match.Success)
                {
                    // Check if there was a literal before the tag
                    if (match.Index > markupEnumerator.Position)
                    {
                        var tokenBeforeMatch = ReadChars(markupEnumerator, match.Index - markupEnumerator.Position);
                        if (match.Groups[2].Success)
                            tokenBeforeMatch = tokenBeforeMatch.TrimEnd(whitespaceChars);
                        if (tokenBeforeMatch != string.Empty)
                            tokens.Add(tokenBeforeMatch);
                    }

                    var isTag = match.Groups[1].Value == "{%";
                    // Ignore hyphen in tag name, add the tag/variable itself
                    var nextToken = new StringBuilder(markupEnumerator.Remaining);
                    nextToken.Append(match.Groups[1].Value);
                    ReadChars(markupEnumerator, match.Length);

                    // Add the parameters and tag closure
                    if (isTag)
                    {
                        if (!ReadToEndOfTag(nextToken, markupEnumerator, SearchQuoteOrTagEnd, syntaxCompatibilityLevel))
                            throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNotTerminatedException"), nextToken.ToString(), Liquid.TagEnd);
                    }
                    else
                    {
                        if (!ReadToEndOfTag(nextToken, markupEnumerator, SearchQuoteOrVariableEnd, syntaxCompatibilityLevel))
                            throw new SyntaxException(Liquid.ResourceManager.GetString("BlockVariableNotTerminatedException"), nextToken.ToString(), Liquid.VariableEnd);
                    }

                    var token = nextToken.ToString();
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
        /// Enumerates over a variable sequence in dotted or bracket notation
        /// </summary>
        /// <param name="source">The Liquid Variable string</param>
        /// <exception cref="SyntaxException"></exception>
        internal static IEnumerator<string> GetVariableEnumerator(string source)
        {
            if (string.IsNullOrEmpty(source))
                yield break;

            using (var markupEnumerator = new CharEnumerator(source))
            {
                while (markupEnumerator.HasNext())
                {
                    var isComplete = false;
                    var nextVariable = new StringBuilder();

                    switch (markupEnumerator.Next)
                    {
                        case '[': // Example Syntax: [var] or ["literal"]
                            markupEnumerator.AppendNext(nextVariable);
                            if (!markupEnumerator.HasNext())
                                break;

                            switch (markupEnumerator.Next)
                            {
                                case '"':
                                case '\'':
                                    markupEnumerator.AppendNext(nextVariable);
                                    isComplete = ReadToChar(nextVariable, markupEnumerator, markupEnumerator.Next) && ReadToChar(nextVariable, markupEnumerator, BracketEnd);
                                    break;
                                default:
                                    isComplete = ReadToChar(nextVariable, markupEnumerator, BracketEnd);
                                    break;
                            }

                            break;
                        default:
                            isComplete = ReadWordChar(nextVariable, markupEnumerator) && ReadToEndOfVariable(nextVariable, markupEnumerator);
                            break;
                    }

                    if (!isComplete) // Somehow we reached the end without finding the end character(s)
                        throw new SyntaxException(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), source, Liquid.VariableEnd);

                    if (markupEnumerator.HasNext() && markupEnumerator.Next == '.' && markupEnumerator.Remaining > 1)  // Don't include dot in tokens as it is a separator
                        markupEnumerator.MoveNext();

                    if (nextVariable.Length > 0)
                        yield return nextVariable.ToString();
                };
            }
        }

        /// <summary>
        /// Reads a fixed number of characters and advances the enumerator position
        /// </summary>
        /// <param name="markupEnumerator">The string enumerator</param>
        /// <param name="markupLength">The number of characters to read</param>
        private static string ReadChars(CharEnumerator markupEnumerator, int markupLength)
        {
            var sb = new StringBuilder(markupLength);
            for (var i = 0; i < markupLength; i++)
                markupEnumerator.AppendNext(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Reads a tag or object variable until the end sequence, <tt>%}</tt> or <tt>}}</tt> respectively and advances the enumerator position
        /// </summary>
        /// <param name="sb">The StringBuilder to write to</param>
        /// <param name="markupEnumerator">The string enumerator</param>
        /// <param name="searchChars">The character set to search for</param>
        /// <returns>True if reaches end sequence, otherwise false</returns>
        private static bool ReadToEndOfTag(StringBuilder sb, CharEnumerator markupEnumerator, HashSet<char> searchChars, SyntaxCompatibility syntaxCompatibilityLevel)
        {
            while (markupEnumerator.AppendNext(sb))
            {
                char nextChar = markupEnumerator.Current;
                if (searchChars.Contains(nextChar))
                {
                    switch (nextChar)
                    {
                        case '\'':
                        case '"':
                            ReadToChar(sb, markupEnumerator, nextChar);
                            break;
                        case '}':
                        case '%':
                            if (markupEnumerator.Remaining > 0 && markupEnumerator.Next == '}')
                            {
                                var previousCharIsWhitespaceControl = syntaxCompatibilityLevel >= SyntaxCompatibility.DotLiquid22 && markupEnumerator.Previous == '-';
                                markupEnumerator.AppendNext(sb);
                                if (previousCharIsWhitespaceControl)
                                {
                                    // Remove hyphen from token
                                    sb.Remove(sb.Length - 3, 1);

                                    // Trim trailing whitespace by skipping ahead beyond the tag end
                                    while (markupEnumerator.Remaining > 0)
                                    {
                                        if (((uint)markupEnumerator.Next - '\t') <= 5 || markupEnumerator.Next == ' ')
                                            markupEnumerator.MoveNext();
                                        else
                                            break;
                                    }
                                }
                                return true;
                            }
                            break;
                    }
                }
            };

            // Somehow we reached the end without finding the end character(s)
            return false;
        }

        /// <summary>
        /// Reads a token until, and inclusive of, the end character and advances the enumerator position
        /// </summary>
        /// <param name="sb">The StringBuilder to write to</param>
        /// <param name="markupEnumerator">The string enumerator</param>
        /// <param name="endChar">The character that indicates end of token</param>
        /// <returns><see langword="true"/> if reaches <paramref name="endChar"/>, otherwise <see langword="false"/></returns>
        private static bool ReadToChar(StringBuilder sb, CharEnumerator markupEnumerator, char endChar)
        {
            while (markupEnumerator.AppendNext(sb))
            {
                if (markupEnumerator.Current == endChar)
                    return true;
            };

            return false;
        }

        /// <summary>
        /// Reads a token until the end of a variable segment and advances the enumerator position
        /// </summary>
        /// <param name="sb">The StringBuilder to write to</param>
        /// <param name="markupEnumerator">The string enumerator</param>
        /// <returns>True if valid variable, otherwise false</returns>
        private static bool ReadToEndOfVariable(StringBuilder sb, CharEnumerator markupEnumerator)
        {
            while (markupEnumerator.HasNext())
            {
                var nextChar = markupEnumerator.Next;
                if (SearchVariableEnd.Contains(nextChar))
                    return true;

                if (!ReadWordChar(sb, markupEnumerator))
                    return false;
            };
            return true;
        }

        /// <summary>
        /// Reads a single word-character and advances the enumerator position
        /// </summary>
        /// <param name="sb">The StringBuilder to write to</param>
        /// <param name="markupEnumerator">The string enumerator</param>
        /// <returns>True if upcoming character is a word character, otherwise false</returns>
        private static bool ReadWordChar(StringBuilder sb, CharEnumerator markupEnumerator)
        {
            var nextChar = markupEnumerator.Next;
            if (nextChar < 128) // For better performance, avoid regex for standard ascii
            {
                if (!((uint)nextChar - '0' < 10 || (uint)nextChar - 'A' < 26 || (uint)nextChar - 'a' < 26 || nextChar == '_' || nextChar == '-'))
                    return false;
            }
            else if (!VariableSegmentRegex.IsMatch(nextChar.ToString()))
            {
                return false;
            }

            markupEnumerator.AppendNext(sb);
            return true;
        }
    }
}
