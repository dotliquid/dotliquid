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
	public class Include : DotLiquid.Block
	{
		protected static readonly Regex Syntax = new Regex(string.Format(@"({0}+)(\s+(?:with|for)\s+({0}+))?", Liquid.QuotedFragment));

		protected string TemplateName, VariableName;
		protected Dictionary<string, string> Attributes;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
			{
				TemplateName = syntaxMatch.Groups[1].Value;
				VariableName = syntaxMatch.Groups[3].Value;
				if (VariableName == string.Empty)
					VariableName = null;
				Attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
				R.Scan(markup, Liquid.TagAttributes, (key, value) => Attributes[key] = value);
			}
			else
				throw new SyntaxException(Liquid.ResourceManager.GetString("IncludeTagSyntaxException"));

			base.Initialize(tagName, markup, tokens);
		}

		protected override void Parse(List<string> tokens)
		{
		}

		public override void Render(Context context, TextWriter result)
		{
			IFileSystem fileSystem = context.Registers["file_system"] as IFileSystem ?? Template.FileSystem;
			string source = fileSystem.ReadTemplateFile(context, TemplateName);
			Template partial = Template.Parse(source);

			string shortenedTemplateName = TemplateName.Substring(1, TemplateName.Length - 2);
			object variable = context[VariableName ?? shortenedTemplateName];

			context.Stack(() =>
			{
				foreach (var keyValue in Attributes)
					context[keyValue.Key] = context[keyValue.Value];

				if (variable is IEnumerable)
				{
					((IEnumerable) variable).Cast<object>().ToList().ForEach(v =>
					{
						context[shortenedTemplateName] = v;
						partial.Render(result, RenderParameters.FromContext(context));
					});
					return;
				}

				context[shortenedTemplateName] = variable;
				partial.Render(result, RenderParameters.FromContext(context));
			});
		}
	}
}