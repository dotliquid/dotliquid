using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                throw new SyntaxException("Syntax Error in tag 'extends' - Valid syntax: extends [template]");

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
                throw new SyntaxException("Liquid Error: 'extends' must be the first tag in an extending template");
            }

            NodeList.ForEach(n =>
            {
                if (!((n is string && string.IsNullOrWhiteSpace((string)n)) || n is Block || n is Comment || n is Extends))
                    throw new SyntaxException("Liquid Error: only 'comment' and 'block' tags are allowed in an extending template " + n.ToString());
            });

            if (NodeList.Count(o => o is Extends) > 0)
            {
                throw new SyntaxException("Liquid Error: 'extends' tag can be used only once");
            }
        }

        protected override void AssertMissingDelimitation()
        {

        }

        public override void Render(Context context, StringBuilder result)
        {
            IFileSystem fileSystem = context.Registers["file_system"] as IFileSystem ?? Template.FileSystem;
            string source = fileSystem.ReadTemplateFile(context[_templateName] as string);
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

            result.Append(template.Render(context));
        }

        public bool IsExtending(Template template)
        {
            return template.Root.NodeList.Any(node => node is Extends);
        }

        private List<Block> FindBlocks(object node, List<Block> blocks = null)
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