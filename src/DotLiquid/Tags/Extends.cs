using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
	/// <summary>
	/// The Extends tag is used in conjunction with the Block tag to provide template inheritance.
	/// For further syntax and usage please refer to
	/// <see cref="http://docs.djangoproject.com/en/dev/topics/templates/#template-inheritance"/>
	/// </summary>
	/// <example>
	///	To see how Extends and Block can be used together, start by considering this example:
	///
	/// <html>
	/// <head>
	///   <title>{% block title %}My Website{% endblock %}</title>
	/// </head>
	///
	/// <body>
	///   <div id="sidebar">
	///     {% block sidebar %}
	///     <ul>
	///       <li><a href="/">Home</a></li>
	///       <li><a href="/blog/">Blog</a></li>
	///     </ul>
	///     {% endblock %}
	///   </div>
	///
	///   <div id="content">
	///     {% block content %}{% endblock %}
	///   </div>
	/// </body>
	/// </html>
	///
	/// We'll assume this is saved in a file called base.html. In ASP.NET MVC terminology, this file would
	/// be the master page or layout, and each of the "blocks" would be a section. Child templates
	/// (in ASP.NET MVC terminology, views) fill or override these blocks with content. If a child template
	/// does not define a particular block, then the content from the parent template is used as a fallback.
	///
	/// A child template might look like this:
	///
	/// {% extends "base.html" %}
	/// {% block title %}My AMAZING Website{% endblock %}
	///
	/// {% block content %}
	/// {% for entry in blog_entries %}
	///   <h2>{{ entry.title }}</h2>
	///   <p>{{ entry.body }}</p>
	/// {% endfor %}
	/// {% endblock %}
	///
	/// The current IFileSystem will be used to locate "base.html".
	/// </example>
	public class Extends : DotLiquid.Block
	{
		private static readonly Regex Syntax = new Regex(string.Format(@"^({0})", Liquid.QuotedFragment), RegexOptions.Compiled);

		private string _templateName;

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
		}

        internal override void AssertTagRulesViolation(List<IRenderable> rootNodeList)
		{
			if (!(rootNodeList[0] is Extends))
			{
				throw new SyntaxException(Liquid.ResourceManager.GetString("ExtendsTagMustBeFirstTagException"));
			}

			NodeList.ForEach(n =>
			{
                if (!(n is StringRenderable || n is Block || n is Comment || n is Extends))
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

		public override ReturnCode Render(Context context, TextWriter result)
		{
			// Get the template or template content and then either copy it (since it will be modified) or parse it
			IFileSystem fileSystem = context.Registers["file_system"] as IFileSystem ?? Template.FileSystem;
            object file = fileSystem.ReadTemplateFile(context, _templateName);
            Template template = file as Template;
            template = template ?? Template.Parse(file == null ? null : file.ToString());

            List<Block> parentBlocks = FindBlocks(template.Root, null);
            List<Block> orphanedBlocks = ((List<Block>)context.LocalScope["extends"]) ?? new List<Block>();
            BlockRenderState blockState = BlockRenderState.Find(context) ?? new BlockRenderState();

            return context.Stack(() =>
            {
                context["blockstate"] = blockState;         // Set or copy the block state down to this scope
                context["extends"] = new List<Block>();     // Holds Blocks that were not found in the parent
                foreach (Block block in NodeList.OfType<Block>().Concat(orphanedBlocks))
                {
                    Block pb = parentBlocks.Find(b => b.BlockName == block.BlockName);
                    
                    if (pb != null)
                    {
                        Block parent;
                        if (blockState.Parents.TryGetValue(block, out parent))
                            blockState.Parents[pb] = parent;
                        pb.AddParent(blockState.Parents, pb.GetNodeList(blockState));
                        blockState.NodeLists[pb] = block.GetNodeList(blockState);
                    }
                    else if(IsExtending(template))
                    {
                        ((List<Block>)context.LocalScope["extends"]).Add(block);
                    }
                }
                template.Render(result, RenderParameters.FromContext(context));
                return ReturnCode.Return;
            });
		}

        public bool IsExtending(Template template)
        {
            return template.Root.NodeList.Any(node => node is Extends);
        }

		private List<Block> FindBlocks(object node, List<Block> blocks)
		{
			if(blocks == null) blocks = new List<Block>();

			if (node.RespondTo("NodeList"))
			{
				var nodeList = (List<IRenderable>) node.Send("NodeList");

				if (nodeList != null)
				{
					nodeList.ForEach(n =>
					{
						Block block = n as Block;

						if (block != null)
						{
							if (blocks.All(bl => bl.BlockName != block.BlockName)) blocks.Add(block);
						}
						
						FindBlocks(n, blocks);
					});
				}
			}

			return blocks;
		}
	}
}