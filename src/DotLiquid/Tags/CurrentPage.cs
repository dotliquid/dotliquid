using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
	/// <summary>
	/// CurrentPage sets the page used by the Pagination tag in your template.
	///
	/// {% current_page = 10 %}
	/// </summary>
	public class CurrentPage : Tag
	{
		private static readonly Regex Syntax = R.B(R.Q(@"\s*=\s*(.*)\s*"), Liquid.VariableSignature);

		private int _page;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
			{
                if (syntaxMatch.Groups.Count > 0)
                    if (!Int32.TryParse(syntaxMatch.Groups[1].Value, out _page))
                        _page = 1;
			}
			else
			{
                throw new SyntaxException(Liquid.ResourceManager.GetString("CurrentPageTagSyntaxException"));
			}

			base.Initialize(tagName, markup, tokens);
		}

		public override void Render(Context context, TextWriter result)
		{
		    context.Stack(() =>
		    {
                context.Registers["current_page"] = _page;
		    });
		}
	}
}