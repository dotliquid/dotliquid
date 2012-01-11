using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
	/// <summary>
	/// If is the conditional block
	///
	/// {% if user.admin %}
	///   Admin user!
	/// {% else %}
	///   Not admin user
	/// {% endif %}
	///
	///  There are {% if count &lt; 5 %} less {% else %} more {% endif %} items than you need.
	/// </summary>
	public class If : DotLiquid.Block
	{
		private string SyntaxHelp = Liquid.ResourceManager.GetString("IfTagSyntaxException");
		private static readonly Regex Syntax = R.B(R.Q(@"({0})\s*([=!<>a-z_]+)?\s*({0})?"), Liquid.QuotedFragment);
		private static readonly string ExpressionsAndOperators = string.Format(R.Q(@"(?:\b(?:\s?and\s?|\s?or\s?)\b|(?:\s*(?!\b(?:\s?and\s?|\s?or\s?)\b)(?:{0}|\S+)\s*)+)"), Liquid.QuotedFragment);

		protected List<Condition> Blocks { get; private set; }

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Blocks = new List<Condition>();
			PushBlock("if", markup);
			base.Initialize(tagName, markup, tokens);
		}

		public override void UnknownTag(string tag, string markup, List<string> tokens)
		{
			// Ruby version did not include "elseif", but I've added that to make it more C#-friendly.
			if (tag == "elsif" || tag == "elseif" || tag == "else")
				PushBlock(tag, markup);
			else
				base.UnknownTag(tag, markup, tokens);
		}

		public override void Render(Context context, TextWriter result)
		{
			context.Stack(() =>
			{
				foreach (Condition block in Blocks)
				{
					if (block.Evaluate(context))
					{
						RenderAll(block.Attachment, context, result);
						return;
					}
				}
				;
			});
		}

		private void PushBlock(string tag, string markup)
		{
			Condition block;
			if (tag == "else")
			{
				block = new ElseCondition();
			}
			else
			{
				List<string> expressions = R.Scan(markup, ExpressionsAndOperators);
				expressions.Reverse();
				string syntax = expressions.Shift();
				if (string.IsNullOrEmpty(syntax))
					throw new SyntaxException(SyntaxHelp);
				Match syntaxMatch = Syntax.Match(syntax);
				if (!syntaxMatch.Success)
					throw new SyntaxException(SyntaxHelp);

				Condition condition = new Condition(syntaxMatch.Groups[1].Value,
					syntaxMatch.Groups[2].Value, syntaxMatch.Groups[3].Value);

				while (expressions.Any())
				{
					string @operator = expressions.Shift().Trim();

					Match expressionMatch = Syntax.Match(expressions.Shift());
					if (!expressionMatch.Success)
						throw new SyntaxException(SyntaxHelp);

					Condition newCondition = new Condition(expressionMatch.Groups[1].Value,
						expressionMatch.Groups[2].Value, expressionMatch.Groups[3].Value);
					switch (@operator)
					{
						case "and":
							newCondition.And(condition);
							break;
						case "or":
							newCondition.Or(condition);
							break;
					}
					condition = newCondition;
				}
				block = condition;
			}

			Blocks.Add(block);
			NodeList = block.Attach(new List<object>());
		}
	}
}