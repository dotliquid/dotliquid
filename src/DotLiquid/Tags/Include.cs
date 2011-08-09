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
		private static readonly Regex Syntax = new Regex(string.Format(@"({0}+)(\s+(?:with|for)\s+({0}+))?", Liquid.QuotedFragment));

		private string _templateName, _variableName;
		private Dictionary<string, string> _attributes;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
			{
				_templateName = syntaxMatch.Groups[1].Value;
				_variableName = syntaxMatch.Groups[3].Value;
				if (_variableName == string.Empty)
					_variableName = null;
				_attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
				R.Scan(markup, Liquid.TagAttributes, (key, value) => _attributes[key] = value);
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
			string source = fileSystem.ReadTemplateFile(context, _templateName);
			Template partial = Template.Parse(source);

			string shortenedTemplateName = _templateName.Substring(1, _templateName.Length - 2);
			object variable = context[_variableName ?? shortenedTemplateName];

			context.Stack(() =>
			{
				foreach (var keyValue in _attributes)
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