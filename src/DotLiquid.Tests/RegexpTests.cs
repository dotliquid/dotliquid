using System.Text.RegularExpressions;
using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class RegexpTests
	{
        private static Regex _quotedFragmentRegex = new Regex(Liquid.QuotedFragment, RegexOptions.Compiled);
        private static Regex _variableParserRegex = new Regex(Liquid.VariableParser, RegexOptions.Compiled);

		[Test]
		public void TestEmpty()
		{
			CollectionAssert.IsEmpty(R.Scan(string.Empty, _quotedFragmentRegex));
		}

		[Test]
		public void TestQuote()
		{
			CollectionAssert.AreEqual(new[] { "\"arg 1\"" }, R.Scan("\"arg 1\"", _quotedFragmentRegex));
		}

		[Test]
		public void TestWords()
		{
			CollectionAssert.AreEqual(new[] { "arg1", "arg2" }, R.Scan("arg1 arg2", _quotedFragmentRegex));
		}

		[Test]
		public void TestTags()
		{
			CollectionAssert.AreEqual(new[] { "<tr>", "</tr>" }, R.Scan("<tr> </tr>", _quotedFragmentRegex));
			CollectionAssert.AreEqual(new[] { "<tr></tr>" }, R.Scan("<tr></tr>", _quotedFragmentRegex));
			CollectionAssert.AreEqual(new[] { "<style", "class=\"hello\">", "</style>" }, R.Scan("<style class=\"hello\">' </style>", _quotedFragmentRegex));
		}

		[Test]
		public void TestQuotedWords()
		{
			CollectionAssert.AreEqual(new[] { "arg1", "arg2", "\"arg 3\"" }, R.Scan("arg1 arg2 \"arg 3\"", _quotedFragmentRegex));
		}

		[Test]
		public void TestQuotedWords2()
		{
			CollectionAssert.AreEqual(new[] { "arg1", "arg2", "'arg 3'" }, R.Scan("arg1 arg2 'arg 3'", _quotedFragmentRegex));
		}

		[Test]
		public void TestQuotedWordsInTheMiddle()
		{
			CollectionAssert.AreEqual(new[] { "arg1", "arg2", "\"arg 3\"", "arg4" }, R.Scan("arg1 arg2 \"arg 3\" arg4", _quotedFragmentRegex));
		}

		[Test]
		public void TestVariableParser()
		{
			CollectionAssert.AreEqual(new[] { "var" }, R.Scan("var", _variableParserRegex));
			CollectionAssert.AreEqual(new[] { "var", "method" }, R.Scan("var.method", _variableParserRegex));
			CollectionAssert.AreEqual(new[] { "var", "[method]" }, R.Scan("var[method]", _variableParserRegex));
			CollectionAssert.AreEqual(new[] { "var", "[method]", "[0]" }, R.Scan("var[method][0]", _variableParserRegex));
			CollectionAssert.AreEqual(new[] { "var", "[\"method\"]", "[0]" }, R.Scan("var[\"method\"][0]", _variableParserRegex));
			CollectionAssert.AreEqual(new[] { "var", "[method]", "[0]", "method" }, R.Scan("var[method][0].method", _variableParserRegex));
		}
	}
}