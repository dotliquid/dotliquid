using System;
using System.Globalization;
using System.Threading;
using System.Linq;
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
            Assert.AreEqual(null, StandardFilters.Truncate(null));
            Assert.AreEqual("", StandardFilters.Truncate(""));
            Assert.AreEqual("1234...", StandardFilters.Truncate("1234567890", 7));
            Assert.AreEqual("1234567890", StandardFilters.Truncate("1234567890", 20));
            Assert.AreEqual("...", StandardFilters.Truncate("1234567890", 0));
            Assert.AreEqual("1234567890", StandardFilters.Truncate("1234567890"));
        }

        [Test]
        public void TestEscape()
        {
            Assert.AreEqual(null, StandardFilters.Escape(null));
            Assert.AreEqual("", StandardFilters.Escape(""));
            Assert.AreEqual("&lt;strong&gt;", StandardFilters.Escape("<strong>"));
            Assert.AreEqual("&lt;strong&gt;", StandardFilters.H("<strong>"));
        }

        [Test]
        public void TestTruncateWords()
        {
            Assert.AreEqual(null, StandardFilters.TruncateWords(null));
            Assert.AreEqual("", StandardFilters.TruncateWords(""));
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
        public void TestStrip()
        {
            Assert.AreEqual("test", StandardFilters.Strip("  test  "));
            Assert.AreEqual("test", StandardFilters.Strip("   test"));
            Assert.AreEqual("test", StandardFilters.Strip("test   "));
            Assert.AreEqual("test", StandardFilters.Strip("test"));
            Assert.AreEqual(null, StandardFilters.Strip(null));
        }

        [Test]
        public void TestLStrip()
        {
            Assert.AreEqual("test  ", StandardFilters.Lstrip("  test  "));
            Assert.AreEqual("test", StandardFilters.Lstrip("   test"));
            Assert.AreEqual("test   ", StandardFilters.Lstrip("test   "));
            Assert.AreEqual("test", StandardFilters.Lstrip("test"));
            Assert.AreEqual(null, StandardFilters.Lstrip(null));
        }

        [Test]
        public void TestRStrip()
        {
            Assert.AreEqual("  test", StandardFilters.Rstrip("  test  "));
            Assert.AreEqual("   test", StandardFilters.Rstrip("   test"));
            Assert.AreEqual("test", StandardFilters.Rstrip("test   "));
            Assert.AreEqual("test", StandardFilters.Rstrip("test"));
            Assert.AreEqual(null, StandardFilters.Rstrip(null));
        }

        [Test]
        public void TestJoin()
        {
            Assert.AreEqual(null, StandardFilters.Join(null));
            Assert.AreEqual("", StandardFilters.Join(""));
            Assert.AreEqual("1 2 3 4", StandardFilters.Join(new[] { 1, 2, 3, 4 }));
            Assert.AreEqual("1 - 2 - 3 - 4", StandardFilters.Join(new[] { 1, 2, 3, 4 }, " - "));
        }

        [Test]
        public void TestSort()
        {
            Assert.AreEqual(null, StandardFilters.Sort(null));
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.Sort(new string[] { }));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, StandardFilters.Sort(new[] { 4, 3, 2, 1 }));
            CollectionAssert.AreEqual(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } },
                StandardFilters.Sort(new[] { new { a = 4 }, new { a = 3 }, new { a = 1 }, new { a = 2 } }, "a"));
        }

        [Test]
        public void TestSort_OnHashList_WithProperty_DoesNotFlattenList()
        {
            var list = new System.Collections.Generic.List<Hash>();
            var hash1 = CreateHash("1", "Text1");
            var hash2 = CreateHash("2", "Text2");
            var hash3 = CreateHash("3", "Text3");
            list.Add(hash3);
            list.Add(hash1);
            list.Add(hash2);

            var result = StandardFilters.Sort(list, "sortby").Cast<Hash>().ToArray();
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(hash1["content"], result[0]["content"]);
            Assert.AreEqual(hash2["content"], result[1]["content"]);
            Assert.AreEqual(hash3["content"], result[2]["content"]);
        }

        private static Hash CreateHash(string sortby, string content)
        {
            var hash = new Hash();
            hash.Add("sortby", sortby);
            hash.Add("content", content);
            return hash;
        }

        [Test]
        public void TestMap()
        {
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.Map(new string[] { }, "a"));
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
            CollectionAssert.AreEqual(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } },
                StandardFilters.Map(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } }, "b"));

            Assert.AreEqual(null, StandardFilters.Map(null, "a"));
            CollectionAssert.AreEqual(new object[] { null }, StandardFilters.Map(new object[] { null }, "a"));

            var hash = Hash.FromAnonymousObject(new {
                ary = new[] {
                    new Helper.DataObject { PropAllowed = "a", PropDisallowed = "x" },
                    new Helper.DataObject { PropAllowed = "b", PropDisallowed = "y" },
                    new Helper.DataObject { PropAllowed = "c", PropDisallowed = "z" },
                }
            });

            Helper.AssertTemplateResult("abc", "{{ ary | map:'prop_allowed' | join:'' }}", hash);
            Helper.AssertTemplateResult("", "{{ ary | map:'prop_disallowed' | join:'' }}", hash);

            hash = Hash.FromAnonymousObject(new
            {
                ary = new[] {
                    new Helper.DataObjectDrop { Prop = "a" },
                    new Helper.DataObjectDrop { Prop = "b" },
                    new Helper.DataObjectDrop { Prop = "c" },
                }
            });

            Helper.AssertTemplateResult("abc", "{{ ary | map:'prop' | join:'' }}", hash);
            Helper.AssertTemplateResult("", "{{ ary | map:'no_prop' | join:'' }}", hash);
        }

        [TestCase("6.72", "$6.72")]
        [TestCase("6000", "$6,000.00")]
        [TestCase("6000000", "$6,000,000.00")]
        [TestCase("6000.4", "$6,000.40")]
        [TestCase("6000000.4", "$6,000,000.40")]
        [TestCase("6.8458", "$6.85")]
        public void TestAmericanCurrencyFromString(string input, string expected)
        {
#if CORE
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
#else
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
#endif
            Assert.AreEqual(expected, StandardFilters.Currency(input));
        }

        [TestCase("6.72", "6,72 €")]
        [TestCase("6000", "6.000,00 €")]
        [TestCase("6000000", "6.000.000,00 €")]
        [TestCase("6000.4", "6.000,40 €")]
        [TestCase("6000000.4", "6.000.000,40 €")]
        [TestCase("6.8458", "6,85 €")]
        public void TestEuroCurrencyFromString(string input, string expected)
        {
#if CORE
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
#else
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
#endif
            Assert.AreEqual(expected, StandardFilters.Currency(input, "de-DE"));
        }

        [Test]
        public void TestMalformedCurrency()
        {
            Assert.AreEqual("teststring", StandardFilters.Currency("teststring", "de-DE"));
        }

        [Test]
        public void TestCurrencyWithinTemplateRender()
        {
#if CORE
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
#else
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
#endif

            Template dollarTemplate = Template.Parse(@"{{ amount | currency }}");
            Template euroTemplate = Template.Parse(@"{{ amount | currency: ""de-DE"" }}");

            Assert.AreEqual("$7,000.00", dollarTemplate.Render(Hash.FromAnonymousObject(new { amount = "7000" })));
            Assert.AreEqual("7.000,00 €", euroTemplate.Render(Hash.FromAnonymousObject(new { amount = 7000 })));
        }

        [Test]
        public void TestCurrencyFromDoubleInput()
        {
            Assert.AreEqual("$6.85", StandardFilters.Currency(6.8458, "en-US"));
            Assert.AreEqual("$6.72", StandardFilters.Currency(6.72, "en-CA"));
            Assert.AreEqual("6.000.000,00 €", StandardFilters.Currency(6000000, "de-DE"));
            Assert.AreEqual("6.000.000,78 €", StandardFilters.Currency(6000000.78, "de-DE"));
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

            Assert.AreEqual("08/01/2006 10:00:00", StandardFilters.Date("08/01/2006 10:00:00", string.Empty));
            Assert.AreEqual("08/02/2006 10:00:00", StandardFilters.Date("08/02/2006 10:00:00", null));
            Assert.AreEqual(new DateTime(2006, 8, 3, 10, 0, 0).ToString(), StandardFilters.Date(new DateTime(2006, 8, 3, 10, 0, 0), string.Empty));
            Assert.AreEqual(new DateTime(2006, 8, 4, 10, 0, 0).ToString(), StandardFilters.Date(new DateTime(2006, 8, 4, 10, 0, 0), null));

            Assert.AreEqual(new DateTime(2006, 7, 5).ToString("MM/dd/yyyy"), StandardFilters.Date("2006-07-05 10:00:00", "MM/dd/yyyy"));

            Assert.AreEqual(new DateTime(2004, 7, 16).ToString("MM/dd/yyyy"), StandardFilters.Date("Fri Jul 16 2004 01:00:00", "MM/dd/yyyy"));

            Assert.AreEqual(null, StandardFilters.Date(null, "MMMM"));

            Assert.AreEqual("hi", StandardFilters.Date("hi", "MMMM"));

            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date("now", "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date("today", "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date("Now", "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date("Today", "MM/dd/yyyy"));

            Assert.AreEqual(DateTime.Now.ToString(), StandardFilters.Date("now", null));
            Assert.AreEqual(DateTime.Now.ToString(), StandardFilters.Date("today", null));
            Assert.AreEqual(DateTime.Now.ToString(), StandardFilters.Date("now", string.Empty));
            Assert.AreEqual(DateTime.Now.ToString(), StandardFilters.Date("today", string.Empty));

            Assert.AreEqual("345000", StandardFilters.Date(DateTime.Parse("2006-05-05 10:00:00.345"), "ffffff"));

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
            Assert.AreEqual(new DateTime(2006, 8, 3, 10, 0, 0).ToString(), StandardFilters.Date(new DateTime(2006, 8, 3, 10, 0, 0), string.Empty));
            Assert.AreEqual(new DateTime(2006, 8, 4, 10, 0, 0).ToString(), StandardFilters.Date(new DateTime(2006, 8, 4, 10, 0, 0), null));

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
            Assert.Null(StandardFilters.First(null));
            Assert.Null(StandardFilters.Last(null));
            Assert.AreEqual(1, StandardFilters.First(new[] { 1, 2, 3 }));
            Assert.AreEqual(3, StandardFilters.Last(new[] { 1, 2, 3 }));
            Assert.Null(StandardFilters.First(new object[] { }));
            Assert.Null(StandardFilters.Last(new object[] { }));
        }

        [Test]
        public void TestReplace()
        {
            Assert.Null(StandardFilters.Replace(null, "a", "b"));
            Assert.AreEqual("", StandardFilters.Replace("", "a", "b"));
            Assert.AreEqual("a a a a", StandardFilters.Replace("a a a a", null, "b"));
            Assert.AreEqual("a a a a", StandardFilters.Replace("a a a a", "", "b"));
            Assert.AreEqual("b b b b", StandardFilters.Replace("a a a a", "a", "b"));
        }

        [Test]
        public void TestReplaceFirst()
        {
            Assert.Null(StandardFilters.ReplaceFirst(null, "a", "b"));
            Assert.AreEqual("", StandardFilters.ReplaceFirst("", "a", "b"));
            Assert.AreEqual("a a a a", StandardFilters.ReplaceFirst("a a a a", null, "b"));
            Assert.AreEqual("a a a a", StandardFilters.ReplaceFirst("a a a a", "", "b"));
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
                Hash.FromAnonymousObject(new { source = "a\r\nb\r\nc" }));
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
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("2", "{{ 1 | plus:1 }}");
                Helper.AssertTemplateResult("5.5", "{{ 2  | plus:3.5 }}");
                Helper.AssertTemplateResult("5.5", "{{ 3.5 | plus:2 }}");
                Helper.AssertTemplateResult("11", "{{ '1' | plus:'1' }}");
            }
        }

        [Test]
        public void TestMinus()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("4", "{{ input | minus:operand }}", Hash.FromAnonymousObject(new { input = 5, operand = 1 }));
                Helper.AssertTemplateResult("-1.5", "{{ 2  | minus:3.5 }}");
                Helper.AssertTemplateResult("1.5", "{{ 3.5 | minus:2 }}");
            }
        }

        [Test]
        public void TestMinusWithFrenchDecimalSeparator()
        {
            using (CultureHelper.SetCulture("fr-FR"))
            {
                Helper.AssertTemplateResult(string.Format("1{0}2", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{{ 3,2 | minus:2 | round:1 }}");
            }
        }

        [Test]
        public void TestRound()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("1.235", "{{ 1.234678 | round:3 }}");
                Helper.AssertTemplateResult("1", "{{ 1 | round }}");

                Assert.Null(StandardFilters.Round("1.2345678", "two"));
            }
        }

        [Test]
        public void TestTimes()
        {
            using (CultureHelper.SetCulture("en-GB"))
            { 
                Helper.AssertTemplateResult("12", "{{ 3 | times:4 }}");
                Helper.AssertTemplateResult("125", "{{ 10 | times:12.5 }}");
                Helper.AssertTemplateResult("125", "{{ 10.0 | times:12.5 }}");
                Helper.AssertTemplateResult("125", "{{ 12.5 | times:10 }}");
                Helper.AssertTemplateResult("125", "{{ 12.5 | times:10.0 }}");
                Helper.AssertTemplateResult("foofoofoofoo", "{{ 'foo' | times:4 }}");
            }
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
            Assert.Null(StandardFilters.DividedBy(null, 3));
            Assert.Null(StandardFilters.DividedBy(4, null));
        }

        [Test]
        public void TestInt32DividedByInt64 ()
        {
            int a = 20;
            long b = 5;
            var c = a / b;
            Assert.AreEqual( c, (long)4 );


            Hash assigns = Hash.FromAnonymousObject(new { a = a, b = b});
            Helper.AssertTemplateResult("4", "{{ a | divided_by:b }}", assigns);
        }

        [Test]
        public void TestModulo()
        {
            Helper.AssertTemplateResult("1", "{{ 3 | modulo:2 }}");
            Assert.Null(StandardFilters.Modulo(null, 3));
            Assert.Null(StandardFilters.Modulo(4, null));
        }

        [Test]
        public void TestUrlencode()
        {
            Assert.AreEqual("http%3A%2F%2Fdotliquidmarkup.org%2F", StandardFilters.UrlEncode("http://dotliquidmarkup.org/"));
            Assert.AreEqual(null, StandardFilters.UrlEncode(null));
        }

        [Test]
        public void TestDefault()
        {
            Hash assigns = Hash.FromAnonymousObject(new { var1 = "foo", var2 = "bar" });
            Helper.AssertTemplateResult("foo", "{{ var1 | default: 'foobar' }}", assigns);
            Helper.AssertTemplateResult("bar", "{{ var2 | default: 'foobar' }}", assigns);
            Helper.AssertTemplateResult("foobar", "{{ unknownvariable | default: 'foobar' }}", assigns);
        }

        [Test]
        public void TestCapitalize()
        {
            Assert.AreEqual(null, StandardFilters.Capitalize(null));
            Assert.AreEqual("", StandardFilters.Capitalize(""));
            Assert.AreEqual(" ", StandardFilters.Capitalize(" "));
            Assert.AreEqual("That Is One Sentence.", StandardFilters.Capitalize("That is one sentence."));
        }
    }
}
