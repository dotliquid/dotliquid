using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Assign sets a variable in your template.
    ///
    /// {% assign foo = 'monkey' %}
    ///
    /// You can then use the variable later in the page.
    ///
    /// {{ foo }}
    /// </summary>
    public class Assign : Tag
    {
        private string _to;
        private Variable _from;

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            var partsEnumerator = new DotLiquid.Util.CharEnumerator(markup);
            _to = Tokenizer.ReadToChar(partsEnumerator, Tokenizer.CharEquals).Trim();
            if (!string.IsNullOrEmpty(_to) && partsEnumerator.HasNext())
            {
                partsEnumerator.MoveNext();
                _from = new Variable(Tokenizer.ReadChars(partsEnumerator, partsEnumerator.Remaining));
            }
            else
            {
                throw new SyntaxException(Liquid.ResourceManager.GetString("AssignTagSyntaxException"));
            }

            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {
            context.Scopes.Last()[_to] = _from.Render(context);
        }
    }
}
