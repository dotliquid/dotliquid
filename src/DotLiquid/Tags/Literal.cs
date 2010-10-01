using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Literal
    /// 
    /// {% capture heading %}
    /// Monkeys!
    /// {% endcapture %}
    /// ...
    /// <h1>{{ heading }}</h1>
    /// 
    /// Capture is useful for saving content for use later in your template, such as
    /// in a sidebar or footer.
    /// </summary>
    public class Literal : DotLiquid.Block
    {
        public static string FromShortHand(string @string)
        {
            Match match = Regex.Match(@string, Liquid.LiteralShorthand);

            return match.Success ? string.Format(@"{{% literal %}}{0}{{% endliteral %}}", match.Groups[1].Value) : @string;
        }

        protected override void Parse(List<string> tokens)
        {
            NodeList = NodeList ?? new List<object>();
            NodeList.Clear();

            string token;
            while ((token = tokens.Shift()) != null)
            {
                Match fullTokenMatch = FullToken.Match(token);
                if (fullTokenMatch.Success)
                {
                    if (BlockDelimiter == fullTokenMatch.Groups[1].Value)
                    {
                        EndTag();
                        return;
                    }
                    else
                        NodeList.Add(token);
                }
                else
                    NodeList.Add(token);
            }

            AssertMissingDelimitation();
        }
    }
}