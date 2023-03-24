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

        private delegate void ParamDelegate(Context context, string value);

        private static readonly Dictionary<string, ParamDelegate> Params = new Dictionary<string, ParamDelegate>()
        {
            { "culture",     (context, value) => SetCulture(context, value) },
            { "dateformat",  (context, value) => SetDateFormat(context, value) },
            { "syntax",      (context, value) => SetSyntax( context, value) },
            { "using",       (context, value) => AddUsing( context, value) }
        };

        private ParamDelegate param;

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

            // Ensure the markup refers to a valid param (discard underscore to support both Ruby and C# naming)
            var paramName = syntaxMatch.Groups[1].Value.Replace("_", "").ToLower();
            if (!Params.ContainsKey(paramName))
                throw new SyntaxException(
                    message: "ParamTagSyntaxException",
                    args: new string[] { syntaxMatch.Groups[1].Value, Params.Keys.ToString() });

            // Save the Param and the Value (which could be a variable or literal).
            this.param = Params[paramName];
            this.paramValue = syntaxMatch.Groups[2].Value.TrimEnd();

            base.Initialize(tagName: tagName, markup: markup, tokens: tokens);
        }

        /// <summary>
        /// Apply override parameters to the Context for rendering.
        /// </summary>
        /// <exception cref="SyntaxException">For unknown parameters or invalid options for a known parameter.</exception>
        /// <exception cref="FilterNotFoundException">If a non-safelisted filter class is encountered.</exception>
        public override void Render(Context context, TextWriter _)
        {
            // Apply the parameter
            param(context, context[this.paramValue].ToString());
        }

        private static void SetDateFormat(Context context, string value)
        {
            // Ruby or .NET date formats
            if (!"ruby".Equals(value, StringComparison.OrdinalIgnoreCase) && !"dotnet".Equals(value, StringComparison.OrdinalIgnoreCase))
                throw new SyntaxException("ParamOptionSyntaxException", "date_format", value, "dotnet, ruby");

            context.UseRubyDateFormat = String.Equals("dotnet", value, StringComparison.OrdinalIgnoreCase) ? false : true;
        }

        private static void SetCulture(Context context, string value)
        {
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
        }

        private static void SetSyntax(Context context, string value)
        {
            // DotLiquid20|DotLiquid21|DotLiquid22|...
            if (Enum.TryParse<SyntaxCompatibility>(value, out var syntax))
                context.SyntaxCompatibilityLevel = syntax;
            else
                throw new SyntaxException("ParamOptionSyntaxException", "syntax", value, string.Join(",", System.Enum.GetNames(typeof(SyntaxCompatibility))));
        }

        private static void AddUsing(Context context, string value)
        {
            if (Template.TryGetSafelistedFilter(value, out var filterClassType))
                context.AddFilters(new[] { filterClassType });
            else
                throw new SyntaxException("ParamOptionSyntaxException", "using", value, string.Join(",", string.Join(",", Template.GetSafelistedFilterAliases())));
        }
    }
}