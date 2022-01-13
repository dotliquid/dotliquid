using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// Represents a unrendered block in liquid
    /// </summary>
    public class RawBlock : Block
    {
        /// <summary>
        /// Delimiter signaling the end of the block.
        /// </summary>
        /// <remarks>Raw blocks are not changeable from the typical "end"+block name</remarks>
        protected sealed override string BlockDelimiter
        {
            get { return base.BlockDelimiter; }
        }

        /// <summary>
        /// Initializes the tag
        /// </summary>
        /// <param name="tagName">Name of the parsed tag</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Tokens of the parsed tag</param>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            if (!markup.IsNullOrWhiteSpace())
                throw new SyntaxException(string.Format(Liquid.ResourceManager.GetString("SimpleTagSyntaxException"), tagName));
            base.Initialize(tagName, markup, tokens);
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
