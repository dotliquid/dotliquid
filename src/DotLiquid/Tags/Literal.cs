using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Literal
    /// Literal outputs text as is, usefull if your template contains Liquid syntax.
    ///
    /// {% literal %}{% if user = 'tobi' %}hi{% endif %}{% endliteral %}
    ///
    /// or (shorthand version)
    ///
    /// {{{ {% if user = 'tobi' %}hi{% endif %} }}}
    /// </summary>
    public class Literal : DotLiquid.Block
    {
        private static readonly Regex LiteralRegex = R.C(Liquid.LiteralShorthand);

        /// <summary>
        /// Creates a literal from shorthand
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string FromShortHand(string @string)
        {
            if (@string == null)
                return @string;

            Match match = LiteralRegex.Match(@string);
            return match.Success ? string.Format(@"{{% literal %}}{0}{{% endliteral %}}", match.Groups[1].Value) : @string;
        }

        /// <summary>
        /// Parses the tag
        /// </summary>
        /// <param name="tokens"></param>
        protected override void Parse(List<string> tokens)
        {
            NodeList = NodeList ?? new List<object>();
            NodeList.Clear();

            string token;
            while ((token = tokens.Shift()) != null)
            {
                Match fullTokenMatch = FullToken.Match(token);
                if (fullTokenMatch.Success && BlockDelimiter == fullTokenMatch.Groups[1].Value)
                {
                    EndTag();
                    return;
                }
                else
                    NodeList.Add(token);
            }

            AssertMissingDelimitation();
        }
    }
}
