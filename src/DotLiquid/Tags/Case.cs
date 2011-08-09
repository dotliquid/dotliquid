using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid.Tags
{
	public class Case : DotLiquid.Block
	{
		private static readonly Regex Syntax = new Regex(string.Format(@"({0})", Liquid.QuotedFragment));
		private static readonly Regex WhenSyntax = new Regex(string.Format(@"({0})(?:(?:\s+or\s+|\s*\,\s*)({0}.*))?", Liquid.QuotedFragment));

		private List<Condition> _blocks;
		private string _left;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			_blocks = new List<Condition>();

			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
				_left = syntaxMatch.Groups[1].Value;
			else
				throw new SyntaxException(Liquid.ResourceManager.GetString("CaseTagSyntaxException"));

			base.Initialize(tagName, markup, tokens);
		}

		public override void UnknownTag(string tag, string markup, List<string> tokens)
		{
			NodeList = new List<object>();
			switch (tag)
			{
				case "when":
					RecordWhenCondition(markup);
					break;
				case "else":
					RecordElseCondition(markup);
					break;
				default:
					base.UnknownTag(tag, markup, tokens);
					break;
			}
		}

		public override void Render(Context context, TextWriter result)
		{
			context.Stack(() =>
			{
				bool executeElseBlock = true;
				_blocks.ForEach(block =>
				{
					if (block.IsElse)
					{
						if (executeElseBlock)
						{
							RenderAll(block.Attachment, context, result);
							return;
						}
					}
					else if (block.Evaluate(context))
					{
						executeElseBlock = false;
						RenderAll(block.Attachment, context, result);
					}
				});
			});
		}

		private void RecordWhenCondition(string markup)
		{
			while (markup != null)
			{
				// Create a new nodelist and assign it to the new block
				Match whenSyntaxMatch = WhenSyntax.Match(markup);
				if (!whenSyntaxMatch.Success)
					throw new SyntaxException(Liquid.ResourceManager.GetString("CaseTagWhenSyntaxException"));

				markup = whenSyntaxMatch.Groups[2].Value;
				if (string.IsNullOrEmpty(markup))
					markup = null;

				Condition block = new Condition(_left, "==", whenSyntaxMatch.Groups[1].Value);
				block.Attach(NodeList);
				_blocks.Add(block);
			}
		}

		private void RecordElseCondition(string markup)
		{
			if (markup.Trim() != string.Empty)
				throw new SyntaxException(Liquid.ResourceManager.GetString("CaseTagElseSyntaxException"));

			ElseCondition block = new ElseCondition();
			block.Attach(NodeList);
			_blocks.Add(block);
		}
	}
}