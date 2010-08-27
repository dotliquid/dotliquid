using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags.Html
{
	public class TableRow : DotLiquid.Block
	{
		private static readonly Regex Syntax = R.B(R.Q(@"(\w+)\s+in\s+({0}+)"), Liquid.VariableSignature);

		private string _variableName, _collectionName;
		private Dictionary<string, string> _attributes;

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
				throw new SyntaxException("Syntax Error in 'tablerow' loop - Valid syntax: tablerow [item] in [collection] cols=3");

			base.Initialize(tagName, markup, tokens);
		}

		public override void Render(Context context, StringBuilder result)
		{
			object coll = context[_collectionName];

			if (!(coll is IEnumerable))
				return;
			IEnumerable<object> collection = ((IEnumerable) coll).Cast<object>();

			if (_attributes.ContainsKey("limit") || _attributes.ContainsKey("offset"))
			{
				int limit = _attributes.ContainsKey("limit") ? Convert.ToInt32("limit") : -1;
				int offset = _attributes.ContainsKey("offset") ? Convert.ToInt32("offset") : 0;
				collection = collection.Skip(offset).Take(limit);
			}

			collection = collection.ToList();
			int length = collection.Count();

			int cols = Convert.ToInt32(context[_attributes["cols"]]);

			int row = 1;
			int col = 0;

			result.AppendLine("<tr class=\"row1\">");
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

				StringBuilder temp = new StringBuilder();
				RenderAll(NodeList, context, temp);
				result.AppendFormat("<td class=\"col{0}\">{1}</td>", col, temp.ToString());

				if (col == cols && index != length - 1)
				{
					col = 0;
					++row;
					result.AppendLine("</tr>");
					result.AppendFormat("<tr class=\"row{0}\">", row);
				}
			}));
			result.AppendLine("</tr>");
		}
	}
}