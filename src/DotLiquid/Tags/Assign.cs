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
		protected static readonly Regex Syntax = R.B(R.Q(@"({0}+)\s*=\s*(.*)\s*"), Liquid.VariableSignature);

		protected string To;
		protected Variable From;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
			{
				To = syntaxMatch.Groups[1].Value;
				From = new Variable(syntaxMatch.Groups[2].Value);
			}
			else
			{
				throw new SyntaxException(Liquid.ResourceManager.GetString("AssignTagSyntaxException"));
			}

			base.Initialize(tagName, markup, tokens);
		}

		public override void Render(Context context, TextWriter result)
		{
			context.Scopes.Last()[To] = From.Render(context);
		}
	}
}