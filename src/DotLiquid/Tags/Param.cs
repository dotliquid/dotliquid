using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// This tag can be used by Designers to override selected Developer parameters.
    /// </summary>
    public class Param : DotLiquid.Tag
    {
        private static readonly Regex Syntax = R.B(R.Q(@"([\w\-]+)\s*=\s*(.+)\s*"));

        private string _name;
        private string _value;

        /// <summary>
        /// Initializes the Param tag
        /// </summary>
        /// <param name="tagName">Name of the parsed tag</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Tokens of the parsed tag</param>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);
            if (!syntaxMatch.Success)
                throw new SyntaxException("Invalid markup format for tag Param: " + markup);

            _name = syntaxMatch.Groups[1].Value;
            _value = syntaxMatch.Groups[2].Value;

            base.Initialize(tagName, markup, tokens);
        }

        /// <summary>
        /// Apply override parameters to the Context for rendering.
        /// </summary>
        /// <exception cref="SyntaxException">For unknown parameters or invalidaid options for a given parameter.</exception>
        public override void Render(Context context, TextWriter result)
        {
            // Apply the parameter
            var value = context[_value].ToString();
            switch (_name.ToLower())
            {
                case "culture": // value should be a language tag, such as en-US, en-GB or fr-FR
                    if (value.IsNullOrWhiteSpace())
                        value = String.Empty; // String.Empty ensure the InvariantCulture is returned
#if CORE
                    context.CurrentCulture = new CultureInfo(value);
#else
                    context.CurrentCulture = CultureInfo.GetCultureInfo(value);
#endif
                    break;
                case "date_format": //ruby|csharp
                    context.UseRubyDateFormat = String.Equals("ruby", value, StringComparison.OrdinalIgnoreCase) ? true : false;
                    break;
                case "syntax": //DotLiquid20|DotLiquid21|...
                    if (Enum.TryParse<SyntaxCompatibility>(value, out var syntax))
                        context.SyntaxCompatibilityLevel = syntax;
                    else
                        throw new SyntaxException("The specified SyntaxCompatibility in invalid, supported options are: "
                            + string.Join(",", System.Enum.GetNames(typeof(SyntaxCompatibility))));
                    break;
                default:
                    throw new SyntaxException(Liquid.ResourceManager.GetString("ParamTagSyntaxException"), _name);
            }
        }
    }
}