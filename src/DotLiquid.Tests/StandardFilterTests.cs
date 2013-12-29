using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class StandardFilterTests
	{
		[Test]
		public void TestSize()
		{
			Assert.AreEqual(3, StandardFilters.Size(new[] { 1, 2, 3 }));
			Assert.AreEqual(0, StandardFilters.Size(new object[] { }));
			Assert.AreEqual(0, StandardFilters.Size(null));
		}

		[Test]
		public void TestDowncase()
		{
			Assert.AreEqual("testing", StandardFilters.Downcase("Testing"));
			Assert.AreEqual(null, StandardFilters.Downcase(null));
		}

		[Test]
		public void TestUpcase()
		{
			Assert.AreEqual("TESTING", StandardFilters.Upcase("Testing"));
			Assert.AreEqual(null, StandardFilters.Upcase(null));
		}

		[Test]
		public void TestTruncate()
		{
			Assert.AreEqual("1234...", StandardFilters.Truncate("1234567890", 7));
			Assert.AreEqual("1234567890", StandardFilters.Truncate("1234567890", 20));
			Assert.AreEqual("...", StandardFilters.Truncate("1234567890", 0));
			Assert.AreEqual("1234567890", StandardFilters.Truncate("1234567890"));
		}

		[Test]
		public void TestEscape()
		{
			Assert.AreEqual("&lt;strong&gt;", StandardFilters.Escape("<strong>"));
			Assert.AreEqual("&lt;strong&gt;", StandardFilters.H("<strong>"));
		}

		[Test]
		public void TestTruncateWords()
		{
			Assert.AreEqual("one two three", StandardFilters.TruncateWords("one two three", 4));
			Assert.AreEqual("one two...", StandardFilters.TruncateWords("one two three", 2));
			Assert.AreEqual("one two three", StandardFilters.TruncateWords("one two three"));
			Assert.AreEqual("Two small (13&#8221; x 5.5&#8221; x 10&#8221; high) baskets fit inside one large basket (13&#8221;...", StandardFilters.TruncateWords("Two small (13&#8221; x 5.5&#8221; x 10&#8221; high) baskets fit inside one large basket (13&#8221; x 16&#8221; x 10.5&#8221; high) with cover.", 15));
		}

		[Test]
		public void TestSplit()
		{
			Assert.AreEqual(new[] { "This", "is", "a", "sentence" }, StandardFilters.Split("This is a sentence", " "));
			Assert.AreEqual(new string[] { null }, StandardFilters.Split(null, null));
		}

		[Test]
		public void TestStripHtml()
		{
			Assert.AreEqual("test", StandardFilters.StripHtml("<div>test</div>"));
			Assert.AreEqual("test", StandardFilters.StripHtml("<div id='test'>test</div>"));
			Assert.AreEqual(null, StandardFilters.StripHtml(null));
		}

		[Test]
		public void TestJoin()
		{
			Assert.AreEqual("1 2 3 4", StandardFilters.Join(new[] { 1, 2, 3, 4 }));
			Assert.AreEqual("1 - 2 - 3 - 4", StandardFilters.Join(new[] { 1, 2, 3, 4 }, " - "));
		}

		[Test]
		public void TestSort()
		{
			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, StandardFilters.Sort(new[] { 4, 3, 2, 1 }));
			CollectionAssert.AreEqual(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } },
				StandardFilters.Sort(new[] { new { a = 4 }, new { a = 3 }, new { a = 1 }, new { a = 2 } }, "a"));
		}

		[Test]
		public void TestMap()
		{
			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 },
				StandardFilters.Map(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } }, "a"));
			Helper.AssertTemplateResult("abc", "{{ ary | map:'foo' | map:'bar' }}",
				Hash.FromAnonymousObject(
					new
					{
						ary =
							new[]
							{
								Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "a" }) }), Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "b" }) }),
								Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "c" }) })
							}
					}));
		}

		[Test]
		public void TestDate()
		{
			Liquid.UseRubyDateFormat = false;
			DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

			Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date(DateTime.Parse("2006-05-05 10:00:00"), "MMMM"));
			Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date(DateTime.Parse("2006-06-05 10:00:00"), "MMMM"));
			Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date(DateTime.Parse("2006-07-05 10:00:00"), "MMMM"));

			Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date("2006-05-05 10:00:00", "MMMM"));
			Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date("2006-06-05 10:00:00", "MMMM"));
			Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date("2006-07-05 10:00:00", "MMMM"));

			Assert.AreEqual("05/07/2006 10:00:00", StandardFilters.Date("05/07/2006 10:00:00", string.Empty));
			Assert.AreEqual("05/07/2006 10:00:00", StandardFilters.Date("05/07/2006 10:00:00", null));

			Assert.AreEqual(new DateTime(2006, 7, 5).ToString("MM/dd/yyyy"), StandardFilters.Date("2006-07-05 10:00:00", "MM/dd/yyyy"));

			Assert.AreEqual(new DateTime(2004, 7, 16).ToString("MM/dd/yyyy"), StandardFilters.Date("Fri Jul 16 2004 01:00:00", "MM/dd/yyyy"));

			Assert.AreEqual(null, StandardFilters.Date(null, "MMMM"));

			Assert.AreEqual("hi", StandardFilters.Date("hi", "MMMM"));

			Template template = Template.Parse(@"{{ hi | date:""MMMM"" }}");
			Assert.AreEqual("hi", template.Render(Hash.FromAnonymousObject(new { hi = "hi" })));
		}

		[Test]
		public void TestStrFTime()
		{
			Liquid.UseRubyDateFormat = true;
			DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

			Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date(DateTime.Parse("2006-05-05 10:00:00"), "%B"));
			Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date(DateTime.Parse("2006-06-05 10:00:00"), "%B"));
			Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date(DateTime.Parse("2006-07-05 10:00:00"), "%B"));

			Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date("2006-05-05 10:00:00", "%B"));
			Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date("2006-06-05 10:00:00", "%B"));
			Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date("2006-07-05 10:00:00", "%B"));

			Assert.AreEqual("05/07/2006 10:00:00", StandardFilters.Date("05/07/2006 10:00:00", string.Empty));
			Assert.AreEqual("05/07/2006 10:00:00", StandardFilters.Date("05/07/2006 10:00:00", null));

			Assert.AreEqual("07/05/2006", StandardFilters.Date("2006-07-05 10:00:00", "%m/%d/%Y"));

			Assert.AreEqual("07/16/2004", StandardFilters.Date("Fri Jul 16 2004 01:00:00", "%m/%d/%Y"));

			Assert.AreEqual(null, StandardFilters.Date(null, "%M"));

			Assert.AreEqual("hi", StandardFilters.Date("hi", "%M"));

			Template template = Template.Parse(@"{{ hi | date:""%M"" }}");
			Assert.AreEqual("hi", template.Render(Hash.FromAnonymousObject(new { hi = "hi" })));
		}

		[Test]
		public void TestFirstLast()
		{
			Assert.AreEqual(1, StandardFilters.First(new[] { 1, 2, 3 }));
			Assert.AreEqual(3, StandardFilters.Last(new[] { 1, 2, 3 }));
			Assert.AreEqual(null, StandardFilters.First(new object[] { }));
			Assert.AreEqual(null, StandardFilters.Last(new object[] { }));
		}

		[Test]
		public void TestReplace()
		{
			Assert.AreEqual("b b b b", StandardFilters.Replace("a a a a", "a", "b"));
			Assert.AreEqual("b a a a", StandardFilters.ReplaceFirst("a a a a", "a", "b"));
			Helper.AssertTemplateResult("b a a a", "{{ 'a a a a' | replace_first: 'a', 'b' }}");
		}

		[Test]
		public void TestRemove()
		{
			Assert.AreEqual("   ", StandardFilters.Remove("a a a a", "a"));
			Assert.AreEqual("a a a", StandardFilters.RemoveFirst("a a a a", "a "));
			Helper.AssertTemplateResult("a a a", "{{ 'a a a a' | remove_first: 'a ' }}");
		}

		[Test]
		public void TestPipesInStringArguments()
		{
			Helper.AssertTemplateResult("foobar", "{{ 'foo|bar' | remove: '|' }}");
		}

		[Test]
		public void TestStripWindowsNewlines()
		{
			Helper.AssertTemplateResult("abc", "{{ source | strip_newlines }}", Hash.FromAnonymousObject(new { source = "a\r\nb\r\nc" }));
            Helper.AssertTemplateResult("ab", "{{ source | strip_newlines }}", Hash.FromAnonymousObject(new { source = "a\r\n\r\n\r\nb" }));
		}

        [Test]
        public void TestStripUnixNewlines()
        {
            Helper.AssertTemplateResult("abc", "{{ source | strip_newlines }}", Hash.FromAnonymousObject(new { source = "a\nb\nc" }));
            Helper.AssertTemplateResult("ab", "{{ source | strip_newlines }}", Hash.FromAnonymousObject(new { source = "a\n\n\nb" }));
        }

		[Test]
		public void TestWindowsNewlinesToBr()
		{
            Helper.AssertTemplateResult("a<br />\r\nb<br />\r\nc",
				"{{ source | newline_to_br }}",
				Hash.FromAnonymousObject(new { source = "a" + Environment.NewLine + "b" + Environment.NewLine + "c" }));
		}

        [Test]
        public void TestUnixNewlinesToBr()
        {
            Helper.AssertTemplateResult("a<br />\nb<br />\nc",
                "{{ source | newline_to_br }}",
                Hash.FromAnonymousObject(new { source = "a\nb\nc" }));
        }

		[Test]
		public void TestPlus()
		{
			Helper.AssertTemplateResult("2", "{{ 1 | plus:1 }}");
			Helper.AssertTemplateResult("11", "{{ '1' | plus:'1' }}");
		}

		[Test]
		public void TestMinus()
		{
			Helper.AssertTemplateResult("4", "{{ input | minus:operand }}", Hash.FromAnonymousObject(new { input = 5, operand = 1 }));
		}

		[Test, SetCulture("fr-FR")]
		public void TestMinusWithFrenchDecimalSeparator()
		{
			Helper.AssertTemplateResult(string.Format("1{0}2", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{{ 3,2 | minus:2 }}");
		}

		[Test]
		public void TestTimes()
		{
			Helper.AssertTemplateResult("12", "{{ 3 | times:4 }}");
			Helper.AssertTemplateResult("foofoofoofoo", "{{ 'foo' | times:4 }}");
		}

		[Test]
		public void TestAppend()
		{
			Hash assigns = Hash.FromAnonymousObject(new { a = "bc", b = "d" });
			Helper.AssertTemplateResult("bcd", "{{ a | append: 'd'}}", assigns);
			Helper.AssertTemplateResult("bcd", "{{ a | append: b}}", assigns);
		}

		[Test]
		public void TestPrepend()
		{
			Hash assigns = Hash.FromAnonymousObject(new { a = "bc", b = "a" });
			Helper.AssertTemplateResult("abc", "{{ a | prepend: 'a'}}", assigns);
			Helper.AssertTemplateResult("abc", "{{ a | prepend: b}}", assigns);
		}

		[Test]
		public void TestDividedBy()
		{
			Helper.AssertTemplateResult("4", "{{ 12 | divided_by:3 }}");
			Helper.AssertTemplateResult("4", "{{ 14 | divided_by:3 }}");
			Helper.AssertTemplateResult("5", "{{ 15 | divided_by:3 }}");
		}

		[Test]
		public void TestModulo()
		{
			Helper.AssertTemplateResult("1", "{{ 3 | modulo:2 }}");
		}
	}
}