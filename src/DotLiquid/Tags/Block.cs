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
			Dictionary<Block, List<object>> blockNodes
                = ((Dictionary<Block, List<object>>)context.Scopes[0]["blocknodes"]);
            Dictionary<Block, Block> blockParents
                = ((Dictionary<Block, Block>)context.Scopes[0]["blockparents"]);
			context.Stack(() =>
			{
				context["block"] = new BlockDrop(this, result);
			    context["blocknodes"] = blockNodes; // Copy the block node replacements into this scope (for nested blocks)
			    context["blockparents"] = blockParents;
				RenderAll(GetNodeList(blockNodes), context, result);
			});
		}

        // Gets the render-time node list from the node list cache
        public List<object> GetNodeList(Dictionary<Block, List<object>> blockNodes)
        {
            List<object> nodeList;
            if (blockNodes == null || !blockNodes.TryGetValue(this, out nodeList)) nodeList = NodeList;
            return nodeList;
        }

        public void AddParent(Dictionary<Block, Block> parents, List<object> nodeList)
        {
            Block parent;
            if(parents.TryGetValue(this, out parent))
            {
                parent.AddParent(parents, nodeList);
            }
            else
            {
                parent = new Block();
                parent.Initialize(TagName, BlockName, null);
                parent.NodeList = new List<object>(nodeList);
                parents[this] = parent;
            }
        }

		public void CallSuper(Context context, TextWriter result)
		{
            Dictionary<Block, Block> blockParents
                = ((Dictionary<Block, Block>)context.Scopes[0]["blockparents"]);
		    Block parent;
		    if(blockParents != null && blockParents.TryGetValue(this, out parent) && parent != null)
		    {
		        parent.Render(context, result);
		    }
		}
	}
}