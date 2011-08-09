using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class VariableTests
	{
		[Test]
		public void TestVariable()
		{
			Variable var = new Variable("hello");
			Assert.AreEqual("hello", var.Name);
		}

		[Test]
		public void TestFilters()
		{
			Variable var = new Variable("hello | textileze");
			Assert.AreEqual("hello", var.Name);
			Assert.AreEqual(1, var.Filters.Count);
			Assert.AreEqual("textileze", var.Filters[0].Name);
			Assert.AreEqual(0, var.Filters[0].Arguments.Length);

			var = new Variable("hello | textileze | paragraph");
			Assert.AreEqual("hello", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("textileze", new string[] { }), new Variable.Filter("paragraph", new string[] { }) }, var.Filters);

			var = new Variable(" hello | strftime: '%Y'");
			Assert.AreEqual("hello", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("strftime", new[] { "'%Y'" }) }, var.Filters);

			var = new Variable(" 'typo' | link_to: 'Typo', true ");
			Assert.AreEqual("'typo'", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("link_to", new[] { "'Typo'", "true" }) }, var.Filters);

			var = new Variable(" 'typo' | link_to: 'Typo', false ");
			Assert.AreEqual("'typo'", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("link_to", new[] { "'Typo'", "false" }) }, var.Filters);

			var = new Variable(" 'foo' | repeat: 3 ");
			Assert.AreEqual("'foo'", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("repeat", new[] { "3" }) }, var.Filters);

			var = new Variable(" 'foo' | repeat: 3, 3 ");
			Assert.AreEqual("'foo'", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("repeat", new[] { "3", "3" }) }, var.Filters);

			var = new Variable(" 'foo' | repeat: 3, 3, 3 ");
			Assert.AreEqual("'foo'", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("repeat", new[] { "3", "3", "3" }) }, var.Filters);

			var = new Variable(" hello | strftime: '%Y, okay?'");
			Assert.AreEqual("hello", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("strftime", new[] { "'%Y, okay?'" }) }, var.Filters);

			var = new Variable(" hello | things: \"%Y, okay?\", 'the other one'");
			Assert.AreEqual("hello", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("things", new[] { "\"%Y, okay?\"", "'the other one'" }) }, var.Filters);
		}

		[Test]
		public void TestFilterWithDateParameter()
		{
			Variable var = new Variable(" '2006-06-06' | date: \"%m/%d/%Y\"");
			Assert.AreEqual("'2006-06-06'", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("date", new[] { "\"%m/%d/%Y\"" }) }, var.Filters);
		}

		[Test]
		public void TestFiltersWithoutWhitespace()
		{
			Variable var = new Variable("hello | textileze | paragraph");
			Assert.AreEqual("hello", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("textileze", new string[] { }), new Variable.Filter("paragraph", new string[] { }) }, var.Filters);

			var = new Variable("hello|textileze|paragraph");
			Assert.AreEqual("hello", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("textileze", new string[] { }), new Variable.Filter("paragraph", new string[] { }) }, var.Filters);
		}

		[Test]
		public void TestSymbol()
		{
			Variable var = new Variable("http://disney.com/logo.gif | image: 'med' ");
			Assert.AreEqual("http://disney.com/logo.gif", var.Name);
			AssertFiltersAreEqual(new[] { new Variable.Filter("image", new[] { "'med'" }) }, var.Filters);
		}

		[Test]
		public void TestStringSingleQuoted()
		{
			Variable var = new Variable(" 'hello' ");
			Assert.AreEqual("'hello'", var.Name);
		}

		[Test]
		public void TestStringDoubleQuoted()
		{
			Variable var = new Variable(" \"hello\" ");
			Assert.AreEqual("\"hello\"", var.Name);
		}

		[Test]
		public void TestInteger()
		{
			Variable var = new Variable(" 1000 ");
			Assert.AreEqual("1000", var.Name);
		}

		[Test]
		public void TestFloat()
		{
			Variable var = new Variable(" 1000.01 ");
			Assert.AreEqual("1000.01", var.Name);
		}

		[Test]
		public void TestStringWithSpecialChars()
		{
			Variable var = new Variable(" 'hello! $!@.;\"ddasd\" ' ");
			Assert.AreEqual("'hello! $!@.;\"ddasd\" '", var.Name);
		}

		[Test]
		public void TestStringDot()
		{
			Variable var = new Variable(" test.test ");
			Assert.AreEqual("test.test", var.Name);
		}

		private static void AssertFiltersAreEqual(Variable.Filter[] expected, System.Collections.Generic.List<Variable.Filter> actual)
		{
			Assert.AreEqual(expected.Length, actual.Count);
			for (int i = 0; i < expected.Length; ++i)
			{
				Assert.AreEqual(expected[i].Name, actual[i].Name);
				Assert.AreEqual(expected[i].Arguments.Length, actual[i].Arguments.Length);
				for (int j = 0; j < expected[i].Arguments.Length; ++j)
					Assert.AreEqual(expected[i].Arguments[j], actual[i].Arguments[j]);
			}
		}
	}
}