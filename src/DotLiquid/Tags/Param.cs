using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        private string paramName;

        private string paramValue;

        /// <summary>
        /// Initializes the Param tag
        /// </summary>
        /// <param name="tagName">Name of the parsed tag</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Tokens of the parsed tag</param>
        /// <exception cref="SyntaxException">If parameter format is invalid.</exception>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            var syntaxMatch = Syntax.Match(markup);
            if (!syntaxMatch.Success)
                throw new SyntaxException("Invalid markup format for tag Param: " + markup);

            this.paramName = syntaxMatch.Groups[1].Value;
            this.paramValue = syntaxMatch.Groups[2].Value;

            base.Initialize(tagName: tagName, markup: markup, tokens: tokens);
        }

        /// <summary>
        /// Apply override parameters to the Context for rendering.
        /// </summary>
        /// <exception cref="SyntaxException">For unknown parameters or invalid options for a known parameter.</exception>
        /// <exception cref="FilterNotFoundException">If a non-safelisted filter class is encountered.</exception>
        public override void Render(Context context, TextWriter result)
        {
            // Apply the parameter
            var value = context[this.paramValue].ToString();
            switch (this.paramName.ToLower())
            {
                case "culture": // value should be a language tag, such as en-US, en-GB or fr-FR
                    if (value.IsNullOrWhiteSpace())
                        value = String.Empty; // String.Empty will ensure the InvariantCulture is returned
                    try
                    {
#if CORE
                        context.CurrentCulture = new CultureInfo(value);
#else
                        context.CurrentCulture = CultureInfo.GetCultureInfo(value);
#endif
                    }
                    catch (CultureNotFoundException exception)
                    {
                        throw new SyntaxException("CultureNotFoundException", value);
                    }
                    break;
                case "dateformat": // CSharp syntax
                case "date_format": // Ruby syntax
                    context.UseRubyDateFormat = String.Equals("ruby", value, StringComparison.OrdinalIgnoreCase) ? true : false;
                    break;
                case "syntax": // DotLiquid20|DotLiquid21|DotLiquid22|...
                    if (Enum.TryParse<SyntaxCompatibility>(value, out var syntax))
                        context.SyntaxCompatibilityLevel = syntax;
                    else
                        throw new SyntaxException("SyntaxCompatibilityException", value, string.Join(",", System.Enum.GetNames(typeof(SyntaxCompatibility))));
                    break;
                case "using": // using additional filters
                    if (Template.TryGetSafelistedFilter(value, out var filterClassType))
                        context.AddFilters(new[] { filterClassType });
                    else
                        throw new FilterNotFoundException(
                            message: Liquid.ResourceManager.GetString("FilterClassNotFoundException"),
                            args: new[] { this.paramValue, string.Join(",", Template.GetSafelistedFilterAliases()) });
                    break;
                default:
                    throw new SyntaxException("ParamTagSyntaxException", this.paramName);
            }
        }
    }
}