using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid
{
	public class Block : Tag
	{
		private static readonly Regex IsTag = new Regex(string.Format(@"^{0}", Liquid.TagStart));
		private static readonly Regex IsVariable = new Regex(string.Format(@"^{0}", Liquid.VariableStart));
		private static readonly Regex ContentOfVariable = new Regex(string.Format(@"^{0}(.*){1}$", Liquid.VariableStart, Liquid.VariableEnd));

		internal static readonly Regex FullToken = new Regex(string.Format(@"^{0}\s*(\w+)\s*(.*)?{1}$", Liquid.TagStart, Liquid.TagEnd));

		protected override void Parse(List<string> tokens)
		{
			NodeList = NodeList ?? new List<object>();
			NodeList.Clear();

			string token;
			while ((token = tokens.Shift()) != null)
			{
				Match isTagMatch = IsTag.Match(token);
				if (isTagMatch.Success)
				{
					Match fullTokenMatch = FullToken.Match(token);
					if (fullTokenMatch.Success)
					{
						// If we found the proper block delimitor just end parsing here and let the outer block
						// proceed
						if (BlockDelimiter == fullTokenMatch.Groups[1].Value)
						{
							EndTag();
							return;
						}

						// Fetch the tag from registered blocks
						Type tagType;
						if ((tagType = Template.GetTagType(fullTokenMatch.Groups[1].Value)) != null)
						{
							Tag tag = (Tag) Activator.CreateInstance(tagType);
							tag.Initialize(fullTokenMatch.Groups[1].Value, fullTokenMatch.Groups[2].Value, tokens);
							NodeList.Add(tag);

							// If the tag has some rules (eg: it must occur once) then check for them
							tag.AssertTagRulesViolation(NodeList);
						}
						else
						{
							// This tag is not registered with the system
							// pass it to the current block for special handling or error reporting
							UnknownTag(fullTokenMatch.Groups[1].Value, fullTokenMatch.Groups[2].Value, tokens);
						}
					}
					else
					{
						throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNotTerminatedException"), token, Liquid.TagEnd);
					}
				}
				else if (IsVariable.Match(token).Success)
				{
					NodeList.Add(CreateVariable(token));
				}
				else if (token == string.Empty)
				{
					// Pass
				}
				else
				{
					NodeList.Add(token);
				}
			}

			// Make sure that its ok to end parsing in the current block.
			// Effectively this method will throw an exception unless the current block is
			// of type Document
			AssertMissingDelimitation();
		}

		public virtual void EndTag()
		{
		}

		public virtual void UnknownTag(string tag, string markup, List<string> tokens)
		{
			switch (tag)
			{
				case "else":
					throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNoElseException"), BlockName);
				case "end":
					throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNoEndException"), BlockName, BlockDelimiter);
				default:
					throw new SyntaxException(Liquid.ResourceManager.GetString("BlockUnknownTagException"), tag);
			}
		}

		protected virtual string BlockDelimiter
		{
			get { return string.Format("end{0}", BlockName); }
		}

		private string BlockName
		{
			get { return TagName; }
		}

		public Variable CreateVariable(string token)
		{
			Match match = ContentOfVariable.Match(token);
			if (match.Success)
				return new Variable(match.Groups[1].Value);
			throw new SyntaxException(Liquid.ResourceManager.GetString("BlockVariableNotTerminatedException"), token, Liquid.VariableEnd);
		}

		public override void Render(Context context, TextWriter result)
		{
			RenderAll(NodeList, context, result);
		}

		protected virtual void AssertMissingDelimitation()
		{
			throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagNotClosedException"), BlockName);
		}

		protected void RenderAll(List<object> list, Context context, TextWriter result)
		{
			list.ForEach(token =>
			{
				try
				{
					if (token is IRenderable)
						((IRenderable) token).Render(context, result);
					else
						result.Write(token.ToString());
				}
				catch (Exception ex)
				{
					if (ex.InnerException is LiquidException)
						ex = ex.InnerException;
					result.Write(context.HandleError(ex));
				}
			});
		}
	}
}