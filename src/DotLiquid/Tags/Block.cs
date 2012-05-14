using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid.Tags
{
	public class BlockDrop : Drop
	{
		private readonly Block _block;
	    private readonly TextWriter _result;

		public BlockDrop(Block block, TextWriter result)
		{
		    _block = block;
		    _result = result;
		}

	    public void Super()
		{
			_block.CallSuper(Context, _result);
		}
	}

	/// <summary>
	/// The Block tag is used in conjunction with the Extends tag to provide template inheritance.
	/// For an example please refer to the Extends tag.
	/// </summary>
	public class Block : DotLiquid.Block
	{
		private static readonly Regex Syntax = new Regex(@"(\w+)");

		internal Block Parent { get; set; }
		internal string BlockName { get; set; }

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
				BlockName = syntaxMatch.Groups[1].Value;
			else
				throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagSyntaxException"));

			if (tokens != null)
			{
				base.Initialize(tagName, markup, tokens);
			}
		}

		internal override void AssertTagRulesViolation(List<object> rootNodeList)
		{
			rootNodeList.ForEach(n =>
			{
				Block b1 = n as Block;

				if (b1 != null)
				{
					List<object> found = rootNodeList.FindAll(o =>
					{
						Block b2 = o as Block;
						return b2 != null && b1.BlockName == b2.BlockName;
					});

					if (found != null && found.Count > 1)
					{
						throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagAlreadyDefinedException"), b1.BlockName);
					}
				}
			});
		}

		public override void Render(Context context, TextWriter result)
		{
			context.Stack(() =>
			{
				context["block"] = new BlockDrop(this, result);
				RenderAll(NodeList, context, result);
			});
		}

		public void AddParent(List<object> nodeList)
		{
			if (Parent != null)
			{
				Parent.AddParent(nodeList);
			}
			else
			{
				Parent = new Block();
				Parent.Initialize(TagName, BlockName, null);
				Parent.NodeList = new List<object>(nodeList);
			}
		}

		public void CallSuper(Context context, TextWriter result)
		{
			if (Parent != null)
			{
				Parent.Render(context, result);
			}
		}
	}
}