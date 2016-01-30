using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid.Tags
{
	/// <summary>
	/// Capture stores the result of a block into a variable without rendering it inplace.
	/// 
	/// {% capture heading %}
	/// Monkeys!
	/// {% endcapture %}
	/// ...
	/// <h1>{{ heading }}</h1>
	/// 
	/// Capture is useful for saving content for use later in your template, such as
	/// in a sidebar or footer.
	/// </summary>
	public class Capture : DotLiquid.Block
	{
		private static readonly Regex Syntax = new Regex(@"(\w+)", RegexOptions.Compiled);

		private string _to;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
				_to = syntaxMatch.Groups[1].Value;
			else
				throw new SyntaxException(Liquid.ResourceManager.GetString("CapureTagSyntaxException"));

			base.Initialize(tagName, markup, tokens);
		}

		public override ReturnCode Render(Context context, TextWriter result)
		{
			using (TextWriter temp = new StringWriter())
			{
				var retCode = base.Render(context, temp);
				if (retCode != ReturnCode.Return)
					return retCode;
				context.GlobalScope[_to] = temp.ToString();
			}

			return ReturnCode.Return;
		}
	}
}