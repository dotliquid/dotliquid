using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags.Html
{
    /// <summary>
    /// TablerRow tag
    /// </summary>
    /// <example>
    /// &lt;table&gt;
    ///   {% tablerow product in collection.products %}
    ///     {{ product.title }}
    ///   {% endtablerow %}
    /// &lt;/table&gt;
    /// </example>
    public class TableRow : DotLiquid.Block
    {
        private static readonly Regex Syntax = R.B(R.Q(@"(\w+)\s+in\s+({0}+)"), Liquid.VariableSignature);

        private string _variableName, _collectionName;
        private Dictionary<string, string> _attributes;

        /// <summary>
        /// Initializes the tablerow tag
        /// </summary>
        /// <param name="tagName">Name of the parsed tag</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Toeksn of the parsed tag</param>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
            {
                _variableName = syntaxMatch.Groups[1].Value;
                _collectionName = syntaxMatch.Groups[2].Value;
                _attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
                R.Scan(markup, Liquid.TagAttributes, (key, value) => _attributes[key] = value);
            }
            else
                throw new SyntaxException(Liquid.ResourceManager.GetString("TableRowTagSyntaxException"));

            base.Initialize(tagName, markup, tokens);
        }

        /// <summary>
        /// Renders the tablerow tag
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public override void Render(Context context, TextWriter result)
        {
            object coll = context[_collectionName];

            if (!(coll is IEnumerable))
                return;
            IEnumerable<object> collection = ((IEnumerable) coll).Cast<object>();

            if (_attributes.ContainsKey("offset"))
            {
                int offset = Convert.ToInt32(_attributes["offset"]);
                collection = collection.Skip(offset);
            }

            if (_attributes.ContainsKey("limit"))
            {
                int limit = Convert.ToInt32(_attributes["limit"]);
                collection = collection.Take(limit);
            }

            collection = collection.ToList();
            int length = collection.Count();

            int cols = Convert.ToInt32(context[_attributes["cols"]]);

            int row = 1;
            int col = 0;

            result.WriteLine("<tr class=\"row1\">");
            context.Stack(() => collection.EachWithIndex((item, index) =>
            {
                context[_variableName] = item;
                context["tablerowloop"] = Hash.FromAnonymousObject(new
                {
                    length = length,
                    index = index + 1,
                    index0 = index,
                    col = col + 1,
                    col0 = col,
                    rindex = length - index,
                    rindex0 = length - index - 1,
                    first = (index == 0),
                    last = (index == length - 1),
                    col_first = (col == 0),
                    col_last = (col == cols - 1)
                });

                ++col;

                using (TextWriter temp = new StringWriter(result.FormatProvider))
                {
                    RenderAll(NodeList, context, temp);
                    result.Write("<td class=\"col{0}\">{1}</td>", col, temp.ToString());
                }

                if (col == cols && index != length - 1)
                {
                    col = 0;
                    ++row;
                    result.WriteLine("</tr>");
                    result.Write("<tr class=\"row{0}\">", row);
                }
            }));
            result.WriteLine("</tr>");
        }
    }
}
