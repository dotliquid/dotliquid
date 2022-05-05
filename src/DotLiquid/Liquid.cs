using System;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// Utility containing regexes for Liquid syntax and registering default tags and blocks
    /// </summary>
    public static class Liquid
    {
        internal static readonly ResourceManager ResourceManager = new ResourceManager(typeof(Properties.Resources));

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
        public static readonly string QuotedString = R.Q(@"""[^""]*""|'[^']*'");
        public static readonly string QuotedFragment = string.Format(R.Q(@"{0}|(?:[^\s,\|'""]|{0})+"), QuotedString);
        public static readonly string QuotedAssignFragment = string.Format(R.Q(@"{0}|(?:[^\s\|'""]|{0})+"), QuotedString);
        public static readonly string TagAttributes = string.Format(R.Q(@"(\w+)\s*\:\s*({0})"), QuotedFragment);
        public static readonly string AnyStartingTag = R.Q(@"\{\{|\{\%");
        public static readonly string VariableParser = string.Format(R.Q(@"\[[^\]]+\]|{0}+\??"), VariableSegment);
        public static readonly string LiteralShorthand = R.Q(@"^(?:\{\{\{\s?)(.*?)(?:\s*\}\}\})$");
        public static readonly string CommentShorthand = R.Q(@"^(?:\{\s?\#\s?)(.*?)(?:\s*\#\s?\})$");
        public static bool UseRubyDateFormat = false;

        internal static readonly string DirectorySeparators = @"[\\/]";
        internal static readonly string LimitRelativePath = @"^(?![\\\/\.])(?:[^<>:;,?""*|\x00-\x1F\/\\]+|[\/\\](?!\.))+(?<!\/)$"; /* Blocks hidden files in linux and directory traversal .. */
        private static readonly Lazy<Regex> LazyDirectorySeparatorsRegex = new Lazy<Regex>(() => R.C(DirectorySeparators), LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<Regex> LazyLimitRelativePathRegex = new Lazy<Regex>(() => R.C(LimitRelativePath), LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<Regex> LazyVariableSegmentRegex = new Lazy<Regex>(() => R.B(R.Q(@"\A\s*(?<Variable>{0}+)\s*\Z"), Liquid.VariableSegment), LazyThreadSafetyMode.ExecutionAndPublication);

        internal static Regex DirectorySeparatorsRegex => LazyDirectorySeparatorsRegex.Value;
        internal static Regex LimitRelativePathRegex => LazyLimitRelativePathRegex.Value;
        internal static Regex VariableSegmentRegex => LazyVariableSegmentRegex.Value;


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
            Template.RegisterTag<Tags.Increment>("increment");
            Template.RegisterTag<Tags.Decrement>("decrement");
            Template.RegisterTag<Tags.Param>("param");

            Template.RegisterTag<Tags.Html.TableRow>("tablerow");

            Template.RegisterFilter(typeof(StandardFilters));

            // Safe list optional filters so that they can be enabled by Designers.
            Template.SafelistFilter(typeof(ExtendedFilters));
            Template.SafelistFilter(typeof(ShopifyFilters));
        }
    }
}
