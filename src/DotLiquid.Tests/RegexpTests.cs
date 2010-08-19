using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class RegexpTests
	{
		[Test]
		public void TestEmpty()
		{
			CollectionAssert.IsEmpty(R.Scan(string.Empty, Liquid.QuotedFragment));
		}

		[Test]
		public void TestQuote()
		{
			CollectionAssert.AreEqual(new[] { "\"arg 1\"" }, R.Scan("\"arg 1\"", Liquid.QuotedFragment));
		}

		[Test]
		public void TestWords()
		{
			CollectionAssert.AreEqual(new[] { "arg1", "arg2" }, R.Scan("arg1 arg2", Liquid.QuotedFragment));
		}

		[Test]
		public void TestTags()
		{
			CollectionAssert.AreEqual(new[] { "<tr>", "</tr>" }, R.Scan("<tr> </tr>", Liquid.QuotedFragment));
			CollectionAssert.AreEqual(new[] { "<tr></tr>" }, R.Scan("<tr></tr>", Liquid.QuotedFragment));
			CollectionAssert.AreEqual(new[] { "<style", "class=\"hello\">", "</style>" }, R.Scan("<style class=\"hello\">' </style>", Liquid.QuotedFragment));
		}

		[Test]
		public void TestQuotedWords()
		{
			CollectionAssert.AreEqual(new[] { "arg1", "arg2", "\"arg 3\"" }, R.Scan("arg1 arg2 \"arg 3\"", Liquid.QuotedFragment));
		}

		[Test]
		public void TestQuotedWords2()
		{
			CollectionAssert.AreEqual(new[] { "arg1", "arg2", "'arg 3'" }, R.Scan("arg1 arg2 'arg 3'", Liquid.QuotedFragment));
		}

		[Test]
		public void TestQuotedWordsInTheMiddle()
		{
			CollectionAssert.AreEqual(new[] { "arg1", "arg2", "\"arg 3\"", "arg4" }, R.Scan("arg1 arg2 \"arg 3\" arg4", Liquid.QuotedFragment));
		}

		[Test]
		public void TestVariableParser()
		{
			CollectionAssert.AreEqual(new[] { "var" }, R.Scan("var", Liquid.VariableParser));
			CollectionAssert.AreEqual(new[] { "var", "method" }, R.Scan("var.method", Liquid.VariableParser));
			CollectionAssert.AreEqual(new[] { "var", "[method]" }, R.Scan("var[method]", Liquid.VariableParser));
			CollectionAssert.AreEqual(new[] { "var", "[method]", "[0]" }, R.Scan("var[method][0]", Liquid.VariableParser));
			CollectionAssert.AreEqual(new[] { "var", "[\"method\"]", "[0]" }, R.Scan("var[\"method\"][0]", Liquid.VariableParser));
			CollectionAssert.AreEqual(new[] { "var", "[method]", "[0]", "method" }, R.Scan("var[method][0].method", Liquid.VariableParser));
		}
	}
}