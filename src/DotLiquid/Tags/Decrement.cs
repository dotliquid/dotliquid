using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Decrements a computed value
    /// </summary>
    public class Decrement : Tag
    {
        private string _variable;

        /// <summary>
        /// Initializes the decrement tag and ensures the syntax is correct
        /// </summary>
        /// <param name="tagName">The tag name (should be <pre>decrement</pre>)</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Tokens of the parsed tag</param>
        /// <exception cref="SyntaxException">If the decrement tag is malformed</exception>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Liquid.VariableSegmentRegex.Match(markup);
            if (syntaxMatch.Success)
            {
                _variable = syntaxMatch.Groups["Variable"].Value;
            }
            else
            {
                throw new SyntaxException(Liquid.ResourceManager.GetString("DecrementSyntaxException"));
            }

            base.Initialize(tagName, markup, tokens);
        }

        /// <summary>
        /// Renders the decremented value
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="result">The output buffer containing the currently rendered template</param>
        public override void Render(Context context, TextWriter result)
        {
            Decrement32(context, result, context.Environments[0].TryGetValue(_variable, out var counterObj) ? counterObj : 0);
            base.Render(context, result);
        }

        private void Decrement32(Context context, TextWriter result, object current)
        {
            try
            {
                checked
                { //needed to force OverflowException at runtime
                    var counter = Convert.ToInt32(current) - 1;
                    context.Environments[0][_variable] = counter;
                    result.Write(counter);
                }
            }
            catch (OverflowException)
            {
                Decrement64(context, result, current);
            }
        }

        private void Decrement64(Context context, TextWriter result, object current)
        {
            var counter = Convert.ToInt64(current) - 1;
            context.Environments[0][_variable] = counter;
            result.Write(counter);
        }
    }
}
