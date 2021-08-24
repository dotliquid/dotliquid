using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Adds filters in a safelisted class to the current context.
    ///
    /// {%- addfilters 'ShopifyFilters' -%}
    /// </summary>
    public class AddFilters : Tag
    {
        private static readonly Regex Syntax = R.B(R.Q(@"\s*([""'].+[""'])\s*"));

        private static readonly IDictionary<string, Type> Safelisted = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Add the provided class to the safelist of classes that can be added by a Liquid Designer
        /// </summary>
        /// <param name="filterClassType">A class containing filter operations.</param>
        /// <param name="alias">An alias for the class, if not provided the class' short name is used</param>
        public static void Safelist(Type filterClassType, string alias = null)
        {
            Safelisted[alias ?? filterClassType.Name] = filterClassType;
        }

        private string alias;

        /// <inheritdoc/>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            var syntaxMatch = Syntax.Match(markup);
            if (!syntaxMatch.Success)
                throw new SyntaxException(Liquid.ResourceManager.GetString("AddFiltersTagSyntaxException"));

            this.alias = syntaxMatch.Groups[1].Value;

            base.Initialize(tagName: tagName, markup: markup, tokens: tokens);
        }

        /// <inheritdoc/>
        public override void Render(Context context, TextWriter result)
        {
            var aliasValue = context[alias].ToString();
            if (!Safelisted.ContainsKey(aliasValue))
            {
                throw new FilterNotFoundException(
                    message: Liquid.ResourceManager.GetString("FilterClassNotFoundException"),
                    args: new[] { alias, string.Join(",", Safelisted.Keys) });
            }

            context.AddFilters(new[] { Safelisted[aliasValue] });
        }
    }
}
