using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Increments a computed value
    /// </summary>
    public class Increment : Tag
    {
        private static readonly Regex Syntax = R.B(R.Q(@"(?<Variable>{0}+)\s*"), Liquid.VariableSignature);

        private string _variable;

        /// <summary>
        /// Initialises the increment tag and ensure the syntax is correct
        /// </summary>
        /// <param name="tagName">The tag name (should be <pre>increment</pre></param>
        /// <param name="markup">The raw parameters</param>
        /// <param name="tokens">The tokens</param>
        /// <exception cref="SyntaxException">If the increment tag is malformed</exception>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
            {
                _variable = syntaxMatch.Groups["Variable"].Value;
            }
            else
            {
                throw new SyntaxException(Liquid.ResourceManager.GetString("IncrementSyntaxException"));
            }

            base.Initialize(tagName, markup, tokens);
        }

        /// <summary>
        /// Renders the incremented value
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="result">The output buffer containing the currently rendered template</param>
        public override void Render(Context context, TextWriter result)
        {
            if(context.Counters.TryGetValue(_variable, out var counter))
            {
                counter++;
            }
            else
            {
                counter = 0;
            }
            context.Counters[_variable] = counter;

            result.Write(counter);
            base.Render(context, result);
        }
    }
}
