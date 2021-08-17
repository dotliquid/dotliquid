using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Adds filters in a whitelisted class to the current context.
    ///
    /// {%- addfilters 'ShopifyFilters' -%}
    /// </summary>
    public class AddFilters : Tag
    {
        private static readonly Regex Syntax = R.B(R.Q(@"\s*([""'].+[""'])\s*"));

        private static readonly IDictionary<string, Type> Whitelisted = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Add the provided class to the whitelist of classes that can be added by a Liquid Designer
        /// </summary>
        /// <param name="filterClassType">A class containing filter operations.</param>
        /// <param name="alias">An alias for the class, if not provided the class' short name is used</param>
        public static void Whitelist(Type filterClassType, string alias = null)
        {
            Whitelisted[alias ?? filterClassType.Name] = filterClassType;
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
            if (!Whitelisted.ContainsKey(aliasValue))
            {
                throw new FilterNotFoundException(
                    message: Liquid.ResourceManager.GetString("FilterClassNotFoundException"),
                    args: new[] { alias, string.Join(",", Whitelisted.Keys) });
            }

            context.AddFilters(new[] { Whitelisted[aliasValue] });
        }
    }
}
