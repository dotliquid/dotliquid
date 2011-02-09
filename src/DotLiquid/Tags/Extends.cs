using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    public class Extends : DotLiquid.Block
    {
        private static readonly Regex Syntax = new Regex(string.Format(@"^({0})", Liquid.QuotedFragment));

        private string _templateName;
        protected List<Block> Blocks { get; private set; }

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);

            if (syntaxMatch.Success)
            {
                _templateName = syntaxMatch.Groups[1].Value;
            }
            else
                throw new SyntaxException(Liquid.ResourceManager.GetString("ExtendsTagSyntaxException"));

            base.Initialize(tagName, markup, tokens);

            Blocks = new List<Block>();

            if (NodeList != null)
            {
                NodeList.ForEach(n =>
                {
                    Block block = n as Block;

                    if (block != null)
                    {
                        Blocks.Add(block);
                    }
                });
            }
        }

        internal override void AssertTagRulesViolation(List<object> rootNodeList)
        {
            if (!(rootNodeList[0] is Extends))
            {
				throw new SyntaxException(Liquid.ResourceManager.GetString("ExtendsTagMustBeFirstTagException"));
            }

            NodeList.ForEach(n =>
            {
                if (!((n is string && ((string)n).IsNullOrWhiteSpace()) || n is Block || n is Comment || n is Extends))
                    throw new SyntaxException(Liquid.ResourceManager.GetString("ExtendsTagUnallowedTagsException"));
            });

            if (NodeList.Count(o => o is Extends) > 0)
            {
				throw new SyntaxException(Liquid.ResourceManager.GetString("ExtendsTagCanBeUsedOneException"));
            }
        }

        protected override void AssertMissingDelimitation()
        {

        }

		public override void Render(Context context, StreamWriter result)
        {
            IFileSystem fileSystem = context.Registers["file_system"] as IFileSystem ?? Template.FileSystem;
            string source = fileSystem.ReadTemplateFile(context, _templateName);
            Template template = Template.Parse(source);

            List<Block> parentBlocks = FindBlocks(template.Root);

            Blocks.ForEach(block =>
            {
                Block pb = parentBlocks.Find(b => b.BlockName == block.BlockName);

                if (pb != null)
                {
                    pb.Parent = block.Parent;
                    pb.AddParent(pb.NodeList);
                    pb.NodeList.Clear();
                    pb.NodeList.AddRange(block.NodeList);
                }
                else
                    if (IsExtending(template))
                        template.Root.NodeList.Add(block);
            });

            template.Render(result, context);
        }

        public bool IsExtending(Template template)
        {
            return template.Root.NodeList.Any(node => node is Extends);
        }

#if NET35
        private List<Block> FindBlocks(object node)
        {
            return FindBlocks(node, null);
        }

        private List<Block> FindBlocks(object node, List<Block> blocks)
#else
        private List<Block> FindBlocks(object node, List<Block> blocks = null)
#endif
        {
            if (node.RespondTo("NodeList"))
            {
                List<object> nodeList = (List<object>)node.Send("NodeList");

                if (nodeList != null)
                {
                    List<Block> b = new List<Block>();

                    nodeList.ForEach(n =>
                    {
                        Block block = n as Block;

                        if (block != null)
                        {
                            Block found = b.Find(bl => bl.BlockName == block.BlockName);

                            if (found != null)
                                found = block;
                            else
                                b.Add(block);
                        }
                        else
                            b.AddRange(FindBlocks(n, b));
                    });

                    return b;
                }
            }

            return blocks;
        }
    }
}