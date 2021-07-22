using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Adds a Filter class containing one or more non-standard filters to the current context.
    /// Duplicate/Additional calls to register the same filter class are benign and will be ignored. 
    ///
    /// {%- register_filters 'DotLiquid.ShopifyFilters' -%}
    /// </summary>
    public class RegisterFilters : Tag
    {
        private static readonly Regex Syntax = R.B(R.Q(@"\s*(.+)\s*"));

        private string className;
        private Type classType;

        /// <inheritdoc/>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
                className = syntaxMatch.Groups[1].Value.Trim(new[] { '\'', '"' });

            // The class name must be non-empty
            if (string.IsNullOrWhiteSpace(className))
                throw new SyntaxException(Liquid.ResourceManager.GetString("SimpleTagSyntaxException"), new[] { tagName });

            // Attempt to find the class type
            classType = Type.GetType(className);
            if (classType is null)
                throw new FilterNotFoundException(Liquid.ResourceManager.GetString("FilterClassNotFoundException"), new[] { markup });

            base.Initialize(tagName, markup, tokens);
        }

        /// <inheritdoc/>
        public override void Render(Context context, TextWriter result)
        {
            // Add the filter class to the context
            context.AddFilters(new[] { classType });
        }
    }
}
