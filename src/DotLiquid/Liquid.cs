using System.Reflection;
using System.Resources;
using DotLiquid.NamingConventions;
using DotLiquid.Tags;
using DotLiquid.Tags.Html;
using DotLiquid.Util;

namespace DotLiquid
{
	public static class Liquid
	{
		internal static readonly ResourceManager ResourceManager = new ResourceManager(typeof(DotLiquid.Properties.Resources));

		public static readonly string FilterSeparator = R.Q(@"\|");
		public static readonly string ArgumentSeparator = R.Q(@",");
		public static readonly string FilterArgumentSeparator = R.Q(@":");
		public static readonly string VariableAttributeSeparator = R.Q(@".");
		public static readonly string TagStart = R.Q(@"\{\%");
		public static readonly string TagEnd = R.Q(@"\%\}");
		public static readonly string VariableSignature = R.Q(@"\(?[\w\-\.\[\]]\)?");
		public static readonly string VariableSegment = R.Q(@"[\w\-]");
		public static readonly string VariableStart = R.Q(@"\{\{");
		public static readonly string VariableEnd = R.Q(@"\}\}");
		public static readonly string VariableIncompleteEnd = R.Q(@"\}\}?");
		public static readonly string QuotedString = R.Q(@"""[^""]*""|'[^']*'");
		public static readonly string QuotedFragment = string.Format(R.Q(@"{0}|(?:[^\s,\|'""]|{0})+"), QuotedString);
		public static readonly string QuotedAssignFragment = string.Format(R.Q(@"{0}|(?:[^\s\|'""]|{0})+"), QuotedString);
		public static readonly string StrictQuotedFragment = R.Q(@"""[^""]+""|'[^']+'|[^\s\|\:\,]+");
		public static readonly string FirstFilterArgument = string.Format(R.Q(@"{0}(?:{1})"), FilterArgumentSeparator, StrictQuotedFragment);
		public static readonly string OtherFilterArgument = string.Format(R.Q(@"{0}(?:{1})"), ArgumentSeparator, StrictQuotedFragment);
		public static readonly string SpacelessFilter = string.Format(R.Q(@"^(?:'[^']+'|""[^""]+""|[^'""])*{0}(?:{1})(?:{2}(?:{3})*)?"), FilterSeparator, StrictQuotedFragment, FirstFilterArgument, OtherFilterArgument);
		public static readonly string Expression = string.Format(R.Q(@"(?:{0}(?:{1})*)"), QuotedFragment, SpacelessFilter);
		public static readonly string TagAttributes = string.Format(R.Q(@"(\w+)\s*\:\s*({0})"), QuotedFragment);
		public static readonly string AnyStartingTag = R.Q(@"\{\{|\{\%");
		public static readonly string PartialTemplateParser = string.Format(R.Q(@"{0}.*?{1}|{2}.*?{3}"), TagStart, TagEnd, VariableStart, VariableIncompleteEnd);
		public static readonly string TemplateParser = string.Format(R.Q(@"({0}|{1})"), PartialTemplateParser, AnyStartingTag);
		public static readonly string VariableParser = string.Format(R.Q(@"\[[^\]]+\]|{0}+\??"), VariableSegment);
		public static readonly string LiteralShorthand = R.Q(@"^(?:\{\{\{\s?)(.*?)(?:\s*\}\}\})$");
		public static readonly string CommentShorthand = R.Q(@"^(?:\{\s?\#\s?)(.*?)(?:\s*\#\s?\})$");
		public static bool UseRubyDateFormat = false;

		static Liquid()
		{
			Template.RegisterTag<Tags.Assign>("assign");
			Template.RegisterTag<Tags.Block>("block");
			Template.RegisterTag<Tags.Capture>("capture");
			Template.RegisterTag<Tags.Case>("case");
			Template.RegisterTag<Tags.Comment>("comment");
			Template.RegisterTag<Tags.Cycle>("cycle");
			Template.RegisterTag<Tags.Extends>("extends");
			Template.RegisterTag<Tags.For>("for");
			Template.RegisterTag<Tags.Break>("break");
			Template.RegisterTag<Tags.Continue>("continue");
			Template.RegisterTag<Tags.If>("if");
			Template.RegisterTag<Tags.IfChanged>("ifchanged");
			Template.RegisterTag<Tags.Include>("include");
			Template.RegisterTag<Tags.Literal>("literal");
			Template.RegisterTag<Tags.Unless>("unless");
			Template.RegisterTag<Tags.Raw>("raw");

			Template.RegisterTag<Tags.Html.TableRow>("tablerow");

			Template.RegisterFilter(typeof(StandardFilters));
		}
	}
}