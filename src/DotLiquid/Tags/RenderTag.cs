using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    public class RenderTag : DotLiquid.Block
    {
        private const string For = "for";
        private static readonly Regex Syntax = R.B(@"({0}+)(\s+(with|{1})\s+({2}+)(?:\s+as\s+({3}+))?)?", Liquid.QuotedString, For, Liquid.QuotedFragment, Liquid.VariableSegment);
        private static readonly string[] DisabledTags = { "include" };

        private string _templateNameExpr, _variableNameExpr, _aliasName;
        private Dictionary<string, string> _attributes;
        private bool IsForLoop { get; set; }

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            var syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
            {
                _templateNameExpr = syntaxMatch.Groups[1].Value;
                var withOrFor = syntaxMatch.Groups[3].Value;
                _variableNameExpr = syntaxMatch.Groups[4].Value;
                _aliasName = syntaxMatch.Groups[5].Value;
                if (_variableNameExpr == string.Empty)
                    _variableNameExpr = null;
                if (_aliasName == string.Empty)
                    _aliasName = null;
                if (withOrFor == string.Empty)
                    withOrFor = null;
                IsForLoop = withOrFor == For;
                _attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
                R.Scan(markup, Liquid.TagAttributes, (key, value) => _attributes[key] = value);
            }
            else
                throw new SyntaxException(Liquid.ResourceManager.GetString("RenderTagSyntaxException"));

            base.Initialize(tagName, markup, tokens);
        }

        protected override void Parse(List<string> tokens)
        {
        }

        public override void Render(Context context, TextWriter result)
        {
            if (!(context[_templateNameExpr] is string templateName))
                throw new Exceptions.ArgumentException(Liquid.ResourceManager.GetString("TemplateNameArgumentException"), TagName);
            var partial = PartialCache.Load(templateName, context);
            var contextVariableName = _aliasName ?? templateName;
            var variable = _variableNameExpr != null ? context[_variableNameExpr] : null;

            context.WithDisabledTags(DisabledTags, () =>
            {
                if (IsForLoop && variable is IEnumerable c && !(variable is string))
                {
                    var items = c.Cast<object>().ToList();
                    var length = items.Count;
                    for (var index = 0; index < length; index++)
                        RenderPartial(items[index], new ForloopDrop(length, index));
                }
                else
                {
                    RenderPartial(variable, null);
                }
            });

            void RenderPartial(object var, ForloopDrop forloop)
            {
                var innerContext = context.NewIsolatedContext();
                if (forloop != null)
                    innerContext["forloop"] = forloop;
                foreach (var keyValue in _attributes)
                    innerContext[keyValue.Key] = context[keyValue.Value];
                if (var != null)
                    innerContext[contextVariableName] = var;
                partial.Render(result, RenderParameters.FromContext(innerContext, result.FormatProvider));
            }
        }
    }
}
