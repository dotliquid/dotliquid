using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid.Tags
{
    public class BlockDrop : Drop
    {
        private Block _block;

        public BlockDrop(Block block)
        {
            _block = block;
        }

        public string Super()
        {
            return _block.CallSuper(Context);
        }
    }

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
                throw new SyntaxException("Syntax Error in 'block' - Valid syntax: block [name]");

            if (tokens != null)
            {
                base.Initialize(tagName, markup, tokens);
            }
        }

        public override void Render(Context context, StringBuilder result)
        {
            context.Stack(() =>
            {
                context["block"] = new BlockDrop(this);
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

        public string CallSuper(Context context)
        {
            return Parent != null ? Parent.Render(context) : "";
        }
    }
}