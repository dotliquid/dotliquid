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

        private string _templateName, _variableName, _aliasName;
        private Dictionary<string, string> _attributes;
        private bool IsForLoop { get; set; }

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            var syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
            {
                _templateName = syntaxMatch.Groups[1].Value;
                string withOrFor = syntaxMatch.Groups[3].Value;
                _variableName = syntaxMatch.Groups[4].Value;
                _aliasName = syntaxMatch.Groups[5].Value;
                if (_variableName == string.Empty)
                    _variableName = null;
                if (_aliasName == string.Empty)
                    _aliasName = null;
                if (withOrFor == string.Empty)
                    withOrFor = null;
                IsForLoop = (withOrFor == For);
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
            string templateName = context[_templateName] as string;
            Template partial = PartialCache.Load(templateName, context);
            string contextVariableName = _aliasName ?? templateName;
            var variable = _variableName != null ? context[_variableName] : null;

            context.WithDisabledTags(DisabledTags, () =>
            {
                if (IsForLoop && variable is IEnumerable enumerable && !(variable is string))
                {
                    var items = enumerable.Cast<object>().ToList();
                    var length = items.Count;
                    for (var index = 0; index < length; index++)
                    {
                        RenderPartial(items[index], new ForloopDrop(length, index));
                    }
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
