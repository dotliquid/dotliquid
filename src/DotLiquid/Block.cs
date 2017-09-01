using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// Represents a block in liquid:
    /// {% random 5 %} you have drawn number ^^^, lucky you! {% endrandom %}
    /// </summary>
    public class Block : Tag
    {
        private static readonly Regex IsTag = R.B(@"^{0}", Liquid.TagStart);
        private static readonly Regex IsVariable = R.B(@"^{0}", Liquid.VariableStart);
        private static readonly Regex ContentOfVariable = R.B(@"^{0}(.*){1}$", Liquid.VariableStart, Liquid.VariableEnd);

        internal static readonly Regex FullToken = R.B(@"^{0}\s*(\w+)\s*(.*)?{1}$", Liquid.TagStart, Liquid.TagEnd);

        /// <summary>
        /// Parses a list of tokens
        /// </summary>
        /// <param name="tokens"></param>
        protected override void Parse(List<string> tokens)
        {
            NodeList = NodeList ?? new List<object>();
            NodeList.Clear();

            string token;
            while ((token = tokens.Shift()) != null)
            {
                Match isTagMatch = IsTag.Match(token);
                if (isTagMatch.Success)
                {
                    Match fullTokenMatch = FullToken.Match(token);
                    if (fullTokenMatch.Success)
                    {
                        // If we found the proper block delimitor just end parsing here and let the outer block
                        // proceed
                        if (BlockDelimiter == fullTokenMatch.Groups[1].Value)
                        {
                            EndTag();
                            return;
                        }

                        // Fetch the tag from registered blocks
                        Tag tag;
                        if ((tag = Template.CreateTag(fullTokenMatch.Groups[1].Value)) != null)
                        {
                            tag.Initialize(fullTokenMatch.Groups[1].Value, fullTokenMatch.Groups[2].Value, tokens);
                            NodeList.Add(tag);

                            // If the tag has some rules (eg: it must occur once) then check for them
                            tag.AssertTagRulesViolation(NodeList);
                        }
                        else
                        {
                            // This tag is not registered with the system
                            // pass it to the current block for special handling or error reporting
                            UnknownTag(fullTokenMatch.Groups[1].Value, fullTokenMatch.Groups[2].Value, tokens);
                        }
                    }
                    else
                    {
                        throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNotTerminatedException"), token, Liquid.TagEnd);
                    }
                }
                else if (IsVariable.Match(token).Success)
                {
                    NodeList.Add(CreateVariable(token));
                }
                else if (token == string.Empty)
                {
                    // Pass
                }
                else
                {
                    NodeList.Add(token);
                }
            }

            // Make sure that its ok to end parsing in the current block.
            // Effectively this method will throw an exception unless the current block is
            // of type Document
            AssertMissingDelimitation();
        }

        /// <summary>
        /// Called at the end of the parsing of the tag
        /// </summary>
        public virtual void EndTag()
        {
        }

        /// <summary>
        /// Handles an unknown tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="markup"></param>
        /// <param name="tokens"></param>
        public virtual void UnknownTag(string tag, string markup, List<string> tokens)
        {
            switch (tag)
            {
                case "else":
                    throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNoElseException"), BlockName);
                case "end":
                    throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNoEndException"), BlockName, BlockDelimiter);
                default:
                    throw new SyntaxException(Liquid.ResourceManager.GetString("BlockUnknownTagException"), tag);
            }
        }

        /// <summary>
        /// Delimiter signaling the end of the block.
        /// </summary>
        /// <remarks>Usually "end"+block name</remarks>
        protected virtual string BlockDelimiter
        {
            get { return string.Format("end{0}", BlockName); }
        }

        private string BlockName
        {
            get { return TagName; }
        }

        /// <summary>
        /// Creates a variable from a token:
        /// 
        /// {{ variable }}
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Variable CreateVariable(string token)
        {
            Match match = ContentOfVariable.Match(token);
            if (match.Success)
                return new Variable(match.Groups[1].Value);
            throw new SyntaxException(Liquid.ResourceManager.GetString("BlockVariableNotTerminatedException"), token, Liquid.VariableEnd);
        }

        /// <summary>
        /// Renders the block
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public override void Render(Context context, TextWriter result)
        {
            RenderAll(NodeList, context, result);
        }

        /// <summary>
        /// Throw an exception if the block isn't closed
        /// </summary>
        protected virtual void AssertMissingDelimitation()
        {
            throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNotClosedException"), BlockName);
        }

        /// <summary>
        /// Renders all the objects in the list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        protected void RenderAll(List<object> list, Context context, TextWriter result)
        {
            foreach (var token in list)
            {
                context.CheckTimeout();

                try
                {
                    if (token is IRenderable renderableToken)
                    {
                        renderableToken.Render(context, result);
                    }
                    else
                    {
                        result.Write(token.ToString());
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is LiquidException)
                    {
                        ex = ex.InnerException;
                    }
                    result.Write(context.HandleError(ex));
                }
            }
        }
    }
}
