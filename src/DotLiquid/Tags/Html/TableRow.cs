using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

		private bool _hasLimit;
		private bool _hasOffset;
		private string _limitAttribute;
		private string _offsetAttribute;
		private string _colsAttribute;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
			{
				_variableName = syntaxMatch.Groups[1].Value;
				_collectionName = syntaxMatch.Groups[2].Value;
				var attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
				R.Scan(markup, TagAttributesRegex, (key, value) => attributes[key] = value);

				_hasOffset = attributes.TryGetValue("offset", out _offsetAttribute);
				_hasLimit = attributes.TryGetValue("limit", out _limitAttribute);
				attributes.TryGetValue("cols", out _colsAttribute);
			}
			else
				throw new SyntaxException(Liquid.ResourceManager.GetString("TableRowTagSyntaxException"));

			base.Initialize(tagName, markup, tokens);
		}

		public override ReturnCode Render(Context context, TextWriter result)
		{
			object coll = context[_collectionName];

			var enumerable = coll as IEnumerable;
			if (enumerable == null)
				return ReturnCode.Return;
			var collection = enumerable.Cast<object>();

			if (_hasOffset)
			{
				collection = collection.Skip(Convert.ToInt32(context[_offsetAttribute]));
			}

			if (_hasLimit)
			{
				collection = collection.Take(Convert.ToInt32(context[_limitAttribute]));
			}

			collection = collection.ToList();
			int length = collection.Count();

			int cols = Convert.ToInt32(context[_colsAttribute]);

			int row = 1;
			int col = 0;

			result.WriteLine("<tr class=\"row1\">");
			var returnCode = context.Stack(() =>
			{
				int index = 0;
				foreach (object item in collection)
				{
					context[_variableName] = item;

					var rowLoopHash = new Hash();

					rowLoopHash["length"] = length;
					rowLoopHash["index"] = index + 1;
					rowLoopHash["index0"] = index;
					rowLoopHash["col"] = col + 1;
					rowLoopHash["col0"] = col;
					rowLoopHash["rindex"] = length - index;
					rowLoopHash["rindex0"] = length - index - 1;
					rowLoopHash["first"] = (index == 0);
					rowLoopHash["last"] = (index == length - 1);
					rowLoopHash["col_first"] = (col == 0);
					rowLoopHash["col_last"] = (col == cols - 1);

					context["tablerowloop"] = rowLoopHash;

					++col;

					using (TextWriter temp = new StringWriter())
					{
						var retCode = RenderAll(NodeList, context, temp);
						if (retCode != ReturnCode.Return)
							return retCode;

						result.Write("<td class=\"col{0}\">{1}</td>", col, temp.ToString());
					}

					if (col == cols && index != length - 1)
					{
						col = 0;
						++row;
						result.WriteLine("</tr>");
						result.Write("<tr class=\"row{0}\">", row);
					}

					++index;
				}

				return ReturnCode.Return;
			});

			result.WriteLine("</tr>");

			return returnCode;
		}
	}
}