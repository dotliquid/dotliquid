using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Cycle is usually used within a loop to alternate between values, like colors or DOM classes.
    ///
    ///   {% for item in items %}
    ///    &lt;div class="{% cycle 'red', 'green', 'blue' %}"&gt; {{ item }} &lt;/div&gt;
    ///   {% end %}
    ///
    ///    &lt;div class="red"&gt; Item one &lt;/div&gt;
    ///    &lt;div class="green"&gt; Item two &lt;/div&gt;
    ///    &lt;div class="blue"&gt; Item three &lt;/div&gt;
    ///    &lt;div class="red"&gt; Item four &lt;/div&gt;
    ///    &lt;div class="green"&gt; Item five&lt;/div&gt;
    /// </summary>
    public class Cycle : Tag
    {
        private static readonly Regex SimpleSyntax = R.B(R.Q(@"^{0}+"), Liquid.QuotedFragment);
        private static readonly Regex NamedSyntax = R.B(R.Q(@"^({0})\s*\:\s*(.*)"), Liquid.QuotedFragment);
        private static readonly Regex QuotedFragmentRegex = R.B(R.Q(@"\s*({0})\s*"), Liquid.QuotedFragment);

        private string[] _variables;
        private string _name;

        /// <summary>
        /// Initializes the cycle tag
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="markup"></param>
        /// <param name="tokens"></param>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match match = NamedSyntax.Match(markup);
            if (match.Success)
            {
                _variables = VariablesFromString(match.Groups[2].Value);
                _name = match.Groups[1].Value;
            }
            else
            {
                match = SimpleSyntax.Match(markup);
                if (match.Success)
                {
                    _variables = VariablesFromString(markup);
                    _name = "'" + string.Join(string.Empty, _variables) + "'";
                }
                else
                {
                    throw new SyntaxException(Liquid.ResourceManager.GetString("CycleTagSyntaxException"));
                }
            }

            base.Initialize(tagName, markup, tokens);
        }

        private static string[] VariablesFromString(string markup)
        {
            return markup.Split(',').Select(var =>
            {
                Match match = QuotedFragmentRegex.Match(var);
                return (match.Success && !string.IsNullOrEmpty(match.Groups[1].Value))
                    ? match.Groups[1].Value
                    : null;
            }).ToArray();
        }

        /// <summary>
        /// Renders the cycle tag
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public override Task RenderAsync(Context context, TextWriter result)
        {
            context.Registers["cycle"] = context.Registers["cycle"] ?? new Hash(0);

            context.Stack(async () =>
            {
                string key = context[_name].ToString();
                int iteration = (int) (((Hash) context.Registers["cycle"])[key] ?? 0);
                await result.WriteAsync(context[_variables[iteration]].ToString()).ConfigureAwait(false);
                ++iteration;
                if (iteration >= _variables.Length)
                    iteration = 0;
                ((Hash) context.Registers["cycle"])[key] = iteration;
            });

            return Task.CompletedTask;
        }
    }
}
