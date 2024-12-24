using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private string _variable;

        /// <summary>
        /// Initializes the increment tag and ensures the syntax is correct
        /// </summary>
        /// <param name="tagName">The tag name (should be <pre>increment</pre>)</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Tokens of the parsed tag</param>
        /// <exception cref="SyntaxException">If the increment tag is malformed</exception>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Liquid.VariableSegmentRegex.Match(markup);
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
            var environment = context.Environments.First();
            var currentValue = environment.ContainsKey(_variable) ? environment[_variable] : 0;
            if (environment is IDictionary<string, object> dict)
                Increment32(dict, result, currentValue);
            else
            {
                var stackedEnvironment = new Hash();
                context.Environments.Insert(0, stackedEnvironment);
                Increment32(stackedEnvironment, result, currentValue);
            }
            base.Render(context, result);
        }

        private void Increment32(IDictionary<string, object> environment, TextWriter result, object current)
        {
            try
            {
                checked
                { //needed to force OverflowException at runtime
                    var counter = Convert.ToInt32(current);
                    environment[_variable] = counter + 1;
                    result.Write(counter);
                }
            }
            catch (OverflowException)
            {
                Increment64(environment, result, current);
            }
        }

        private void Increment64(IDictionary<string, object> environment, TextWriter result, object current)
        {
            var counter = Convert.ToInt64(current);
            environment[_variable] = counter + 1;
            result.Write(counter);
        }
    }
}
