using System.Collections;
using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class CustomFilterTests
	{
		#region Classes used in tests

		private static class MoneyFilter
		{
			public static string Money(object input)
			{
				return string.Format(" {0:d}$ ", input);
			}

			public static string MoneyWithUnderscore(object input)
			{
				return string.Format(" {0:d}$ ", input);
			}
		}

		private static class CanadianMoneyFilter
		{
			public static string Money(object input)
			{
				return string.Format(" {0:d}$ CAD ", input);
			}
		}

		private static class FiltersWithArguments
		{
#if NET35
            public static string Adjust(int input)
            {
                return Adjust(input, 10);
            }

            public static string Adjust(int input, int offset)
#else
			public static string Adjust(int input, int offset = 10)
#endif
			{
				return string.Format("[{0:d}]", input + offset);
			}

#if NET35
            public static string AddSub(int input, int plus)
            {
                return AddSub(input, plus, 20);
            }

            public static string AddSub(int input, int plus, int minus)
#else
			public static string AddSub(int input, int plus, int minus = 20)
#endif
			{
				return string.Format("[{0:d}]", input + plus - minus);
			}
		}

		private static class ContextFilters
		{
			public static string BankStatement(Context context, object input)
			{
				return string.Format(" " + context["name"] + " has {0:d}$ ", input);
			}
		}

		#endregion

		private Context _context;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_context = new Context();
		}

		/*[Test]
		public void TestNonExistentFilter()
		{
			_context["var"] = 1000;
			Assert.Throws<FilterNotFoundException>(() => new Variable("var | syzzy").Render(_context));
		}*/

		[Test]
		public void TestLocalFilter()
		{
			_context["var"] = 1000;
			_context.AddFilters(typeof(MoneyFilter));
			//Assert.AreEqual(" 1000$ ", new Variable("var | money").Render(_context));
            Assert.AreEqual(" 1000$ ", new MarkupExpression("var | money").Evaluate(_context));
		}

		[Test]
		public void TestUnderscoreInFilterName()
		{
			_context["var"] = 1000;
			_context.AddFilters(typeof(MoneyFilter));
			//Assert.AreEqual(" 1000$ ", new Variable("var | money_with_underscore").Render(_context));
            Assert.AreEqual(" 1000$ ", new MarkupExpression("var | money_with_underscore").Evaluate(_context));
		}

		[Test]
		public void TestFilterWithNumericArgument()
		{
			_context["var"] = 1000;
			_context.AddFilters(typeof(FiltersWithArguments));
			//Assert.AreEqual("[1005]", new Variable("var | adjust: 5").Render(_context));
            Assert.AreEqual("[1005]", new MarkupExpression("var | adjust: 5").Evaluate(_context));
		}

		[Test]
		public void TestFilterWithNegativeArgument()
		{
			_context["var"] = 1000;
			_context.AddFilters(typeof(FiltersWithArguments));
			//Assert.AreEqual("[995]", new Variable("var | adjust: -5").Render(_context));
            Assert.AreEqual("[995]", new MarkupExpression("var | adjust: -5").Evaluate(_context));
		}

		[Test]
		public void TestFilterWithDefaultArgument()
		{
			_context["var"] = 1000;
			_context.AddFilters(typeof(FiltersWithArguments));
			//Assert.AreEqual("[1010]", new Variable("var | adjust").Render(_context));
            Assert.AreEqual("[1010]", new MarkupExpression("var | adjust").Evaluate(_context));
		}

		[Test]
		public void TestFilterWithTwoArguments()
		{
			_context["var"] = 1000;
			_context.AddFilters(typeof(FiltersWithArguments));
			//Assert.AreEqual("[1150]", new Variable("var | add_sub: 200, 50").Render(_context));
            Assert.AreEqual("[1150]", new MarkupExpression("var | add_sub: 200, 50").Evaluate(_context));
		}

		/*/// <summary>
		/// ATM the trailing value is silently ignored. Should raise an exception?
		/// </summary>
		[Test]
		public void TestFilterWithTwoArgumentsNoComma()
		{
			_context["var"] = 1000;
			_context.AddFilters(typeof(FiltersWithArguments));
			Assert.AreEqual("[1150]", string.Join(string.Empty, new Variable("var | add_sub: 200 50").Render(_context));
		}*/

		[Test]
		public void TestSecondFilterOverwritesFirst()
		{
			_context["var"] = 1000;
			_context.AddFilters(typeof(MoneyFilter));
			_context.AddFilters(typeof(CanadianMoneyFilter));
			//Assert.AreEqual(" 1000$ CAD ", new Variable("var | money").Render(_context));
            Assert.AreEqual(" 1000$ CAD ", new MarkupExpression("var | money").Evaluate(_context));
		}

		[Test]
		public void TestSize()
		{
			_context["var"] = "abcd";
			_context.AddFilters(typeof(MoneyFilter));
			//Assert.AreEqual(4, new Variable("var | size").Render(_context));
            Assert.AreEqual(4, new MarkupExpression("var | size").Evaluate(_context));
		}

		[Test]
		public void TestJoin()
		{
			_context["var"] = new[] { 1, 2, 3, 4 };
			//Assert.AreEqual("1 2 3 4", new Variable("var | join").Render(_context));
            Assert.AreEqual("1 2 3 4", new MarkupExpression("var | join").Evaluate(_context));
		}

		[Test]
		public void TestSort()
		{
			_context["value"] = 3;
			_context["numbers"] = new[] { 2, 1, 4, 3 };
			_context["words"] = new[] { "expected", "as", "alphabetic" };
			_context["arrays"] = new[] { new[] { "flattened" }, new[] { "are" } };

//			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, new Variable("numbers | sort").Render(_context) as IEnumerable);
//			CollectionAssert.AreEqual(new[] { "alphabetic", "as", "expected" }, new Variable("words | sort").Render(_context) as IEnumerable);
//			CollectionAssert.AreEqual(new[] { 3 }, new Variable("value | sort").Render(_context) as IEnumerable);
//			CollectionAssert.AreEqual(new[] { "are", "flattened" }, new Variable("arrays | sort").Render(_context) as IEnumerable);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, new MarkupExpression("numbers | sort").Evaluate(_context) as IEnumerable);
            CollectionAssert.AreEqual(new[] { "alphabetic", "as", "expected" }, new MarkupExpression("words | sort").Evaluate(_context) as IEnumerable);
            CollectionAssert.AreEqual(new[] { 3 }, new MarkupExpression("value | sort").Evaluate(_context) as IEnumerable);
            CollectionAssert.AreEqual(new[] { "are", "flattened" }, new MarkupExpression("arrays | sort").Evaluate(_context) as IEnumerable);

		}

		[Test]
		public void TestSplit()
		{
			_context["var"] = "a~b";
			//Assert.AreEqual(new[] { "a", "b" }, new Variable("var | split:'~'").Render(_context));
            Assert.AreEqual(new[] { "a", "b" }, new MarkupExpression("var | split:'~'").Evaluate(_context));
		}

		[Test]
		public void TestStripHtml()
		{
			_context["var"] = "<b>bla blub</a>";
			//Assert.AreEqual("bla blub", new Variable("var | strip_html").Render(_context));
            Assert.AreEqual("bla blub", new MarkupExpression("var | strip_html").Evaluate(_context));
		}

		[Test]
		public void Capitalize()
		{
			_context["var"] = "blub";
			//Assert.AreEqual("Blub", new Variable("var | capitalize").Render(_context));
            Assert.AreEqual("Blub", new MarkupExpression("var | capitalize").Evaluate(_context));
		}

		[Test]
		public void TestLocalGlobal()
		{
			Template.RegisterFilter(typeof(MoneyFilter));

			Assert.AreEqual(" 1000$ ", Template.Parse("{{1000 | money}}").Render());
			Assert.AreEqual(" 1000$ CAD ", Template.Parse("{{1000 | money}}").Render(new RenderParameters { Filters = new[] { typeof(CanadianMoneyFilter) } }));
			Assert.AreEqual(" 1000$ CAD ", Template.Parse("{{1000 | money}}").Render(new RenderParameters { Filters = new[] { typeof(CanadianMoneyFilter) } }));
		}

		[Test]
		public void TestContextFilter()
		{
			_context["var"] = 1000;
			_context["name"] = "King Kong";
			_context.AddFilters(typeof(ContextFilters));
			//Assert.AreEqual(" King Kong has 1000$ ", new Variable("var | bank_statement").Render(_context));
            Assert.AreEqual(" King Kong has 1000$ ", new MarkupExpression("var | bank_statement").Evaluate(_context));
		}
	}
}