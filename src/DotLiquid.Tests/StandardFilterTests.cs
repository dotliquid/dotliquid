using System;
using System.Globalization;
using System.Threading;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StandardFilterTests
    {
        private Context _contextV20;
        private Context _contextV21;

        [OneTimeSetUp]
        public void SetUp()
        {
            _contextV20 = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid20
            };
            _contextV21 = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid21
            };
        }

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
            Assert.AreEqual(expected: null, actual: StandardFilters.Truncate(null));
            Assert.AreEqual(expected: "", actual: StandardFilters.Truncate(""));
            Assert.AreEqual(expected: "1234...", actual: StandardFilters.Truncate("1234567890", 7));
            Assert.AreEqual(expected: "1234567890", actual: StandardFilters.Truncate("1234567890", 20));
            Assert.AreEqual(expected: "...", actual: StandardFilters.Truncate("1234567890", 0));
            Assert.AreEqual(expected: "1234567890", actual: StandardFilters.Truncate("1234567890"));
            Helper.AssertTemplateResult(expected: "H...", template: "{{ 'Hello' | truncate:4 }}");

            Helper.AssertTemplateResult(expected: "Ground control to...", template: "{{ \"Ground control to Major Tom.\" | truncate: 20}}");
            Helper.AssertTemplateResult(expected: "Ground control, and so on", template: "{{ \"Ground control to Major Tom.\" | truncate: 25, \", and so on\"}}");
            Helper.AssertTemplateResult(expected: "Ground control to Ma", template: "{{ \"Ground control to Major Tom.\" | truncate: 20, \"\"}}");
            Helper.AssertTemplateResult(expected: "...", template: "{{ \"Ground control to Major Tom.\" | truncate: 0}}");
            Helper.AssertTemplateResult(expected: "Liquid error: Value was either too large or too small for an Int32.", template: $"{{{{ \"Ground control to Major Tom.\" | truncate: {((long)int.MaxValue) + 1}}}}}");
            Helper.AssertTemplateResult(expected: "...", template: "{{ \"Ground control to Major Tom.\" | truncate: -1}}");
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

            Helper.AssertTemplateResult(expected: "Ground control to...", template: "{{ \"Ground control to Major Tom.\" | truncate_words: 3}}");
            Helper.AssertTemplateResult(expected: "Ground control to--", template: "{{ \"Ground control to Major Tom.\" | truncate_words: 3, \"--\"}}");
            Helper.AssertTemplateResult(expected: "Ground control to", template: "{{ \"Ground control to Major Tom.\" | truncate_words: 3, \"\"}}");
            Helper.AssertTemplateResult(expected: "...", template: "{{ \"Ground control to Major Tom.\" | truncate_words: 0}}");
            Helper.AssertTemplateResult(expected: "...", template: "{{ \"Ground control to Major Tom.\" | truncate_words: -1}}");
            Helper.AssertTemplateResult(expected: "Liquid error: Value was either too large or too small for an Int32.", template: $"{{{{ \"Ground control to Major Tom.\" | truncate_words: {((long)int.MaxValue) + 1}}}}}");
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
        public void TestSlice()
        {
            Assert.AreEqual(null, StandardFilters.Slice(null, 1));
            Assert.AreEqual(null, StandardFilters.Slice("", 10));
            Assert.AreEqual("abc", StandardFilters.Slice("abcdefg", 0, 3));
            Assert.AreEqual("bcd", StandardFilters.Slice("abcdefg", 1, 3));
            Assert.AreEqual("efg", StandardFilters.Slice("abcdefg", -3, 3));
            Assert.AreEqual("efg", StandardFilters.Slice("abcdefg", -3, 30));
            Assert.AreEqual("efg", StandardFilters.Slice("abcdefg", 4, 30));
            Assert.AreEqual("a", StandardFilters.Slice("abc", -4, 2));
            Assert.AreEqual("", StandardFilters.Slice("abcdefg", -10, 1));
        }
        
        [Test]
        public void TestJoin()
        {
            Assert.AreEqual(null, StandardFilters.Join(null));
            Assert.AreEqual("", StandardFilters.Join(""));
            Assert.AreEqual("1 2 3 4", StandardFilters.Join(new[] { 1, 2, 3, 4 }));
            Assert.AreEqual("1 - 2 - 3 - 4", StandardFilters.Join(new[] { 1, 2, 3, 4 }, " - "));

            // Sample from specification at https://shopify.github.io/liquid/filters/join/
            Helper.AssertTemplateResult(
                expected: "\r\nJohn and Paul and George and Ringo",
                template: @"{% assign beatles = ""John, Paul, George, Ringo"" | split: "", "" %}
{{ beatles | join: "" and "" }}");
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

        [Test]
        public void TestSort_OnDictionaryWithPropertyOnlyInSomeElement_ReturnsSortedDictionary()
        {
            var list = new System.Collections.Generic.List<Hash>();
            var hash1 = CreateHash("1", "Text1");
            var hash2 = CreateHash("2", "Text2");
            var hashWithNoSortByProperty = new Hash();
            hashWithNoSortByProperty.Add("content", "Text 3");
            list.Add(hash2);
            list.Add(hashWithNoSortByProperty);
            list.Add(hash1);

            var result = StandardFilters.Sort(list, "sortby").Cast<Hash>().ToArray();
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(hashWithNoSortByProperty["content"], result[0]["content"]);
            Assert.AreEqual(hash1["content"], result[1]["content"]);
            Assert.AreEqual(hash2["content"], result[2]["content"]);
        }

        private static Hash CreateHash(string sortby, string content) =>
            new Hash
            {
                { "sortby", sortby },
                { "content", content }
            };

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

            var hash = Hash.FromAnonymousObject(new
            {
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

        /// <summary>
        /// Tests map filter per Shopify specification sample
        /// </summary>
        /// <remarks><see href="https://shopify.github.io/liquid/filters/map/"/></remarks>
        [Test]
        public void TestMapSpecificationSample()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                site = new {
                    pages = new[] {
                        new { category = "business" },
                        new { category = "celebrities" },
                        new { category = "lifestyle" },
                        new { category = "sports" },
                        new { category = "technology" }
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "\r\n- business\r\n\r\n- celebrities\r\n\r\n- lifestyle\r\n\r\n- sports\r\n\r\n- technology\r\n",
                template: @"{% assign all_categories = site.pages | map: ""category"" %}{% for item in all_categories %}
- {{ item }}
{% endfor %}",
                localVariables: hash);
        }

        /// <summary>
        /// Tests map filter per Shopify specification sample
        /// </summary>
        /// <remarks>In this variant of the test we add another property to the items in the collection to ensure the map filter does its job of removing other properties</remarks>
        [Test]
        public void TestMapSpecificationSampleVariant()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                site = new
                {
                    pages = new[] {
                        new { category = "business", author = "Joe" },
                        new { category = "celebrities", author = "Jon" },
                        new { category = "lifestyle", author = "John" },
                        new { category = "sports", author = "Joan" },
                        new { category = "technology", author = "Jean" }
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "\r\n- business\r\n\r\n- celebrities\r\n\r\n- lifestyle\r\n\r\n- sports\r\n\r\n- technology\r\n",
                template: @"{% assign all_categories = site.pages | map: ""category"" %}{% for item in all_categories %}
- {{ item }}
{% endfor %}", localVariables: hash);
        }

        [Test]
        public void TestMapShipmentPackage()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                content = new
                {
                    carrierSettings = new[] {
                        new
                        {
                            numberOfPiecesPerPackage = 10,
                            test = "test"
                        },
                        new
                        {
                            numberOfPiecesPerPackage = 12,
                            test = "test1"
                        }
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "{\r\n\r\n\"tests\" : [\r\n            {\r\n                \"numberOfPiecesPerPackage\" : \"10\"\r\n      },\r\n      \r\n            {\r\n                \"numberOfPiecesPerPackage\" : \"12\"\r\n      },\r\n      ]\r\n}",
                template: @"{
{% assign test1 = content.carrierSettings | map: ""numberOfPiecesPerPackage"" %}
""tests"" : [{% for test in test1 %}
            {
                ""numberOfPiecesPerPackage"" : ""{{ test }}""
      },
      {% endfor %}]
}",
                localVariables: hash);

            Helper.AssertTemplateResult(
                expected: "{\r\n\r\n\"tests\" : 1012\r\n}",
                template: @"{
{% assign test1 = content.carrierSettings | map: ""numberOfPiecesPerPackage"" %}
""tests"" : {{test1}}
}",
                localVariables: hash);
        }

        private class Package : IIndexable, ILiquidizable
        {
            private readonly int numberOfPiecesPerPackage;

            private readonly string test;

            public Package(int numberOfPiecesPerPackage, string test)
            {
                this.numberOfPiecesPerPackage = numberOfPiecesPerPackage;
                this.test = test;
            }

            public object this[object key] => key as string == "numberOfPiecesPerPackage"
                ? this.numberOfPiecesPerPackage as object
                : key as string == "test"
                    ? test
                    : null;

            public bool ContainsKey(object key)
            {
                return new List<string> { nameof(numberOfPiecesPerPackage), nameof(test) }
                    .Contains(key);
            }

            public object ToLiquid()
            {
                return this;
            }
        };

        [Test]
        public void TestMapIndexable()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                content = new
                {
                    carrierSettings = new[]
                    {
                        new Package(numberOfPiecesPerPackage:10, test:"test"),
                        new Package(numberOfPiecesPerPackage:12, test:"test1"),
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "{\r\n\r\n\"tests\" : [\r\n            {\r\n                \"numberOfPiecesPerPackage\" : \"10\"\r\n      },\r\n      \r\n            {\r\n                \"numberOfPiecesPerPackage\" : \"12\"\r\n      },\r\n      ]\r\n}",
                template: @"{
{% assign test1 = content.carrierSettings | map: ""numberOfPiecesPerPackage"" %}
""tests"" : [{% for test in test1 %}
            {
                ""numberOfPiecesPerPackage"" : ""{{ test }}""
      },
      {% endfor %}]
}",
                localVariables: hash);

            Helper.AssertTemplateResult(
                expected: "{\r\n\r\n\"tests\" : 1012\r\n}",
                template: @"{
{% assign test1 = content.carrierSettings | map: ""numberOfPiecesPerPackage"" %}
""tests"" : {{test1}}
}",
                localVariables: hash);
        }

        [Test]
        public void TestMapJoin()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                content = new
                {
                    carrierSettings = new[] {
                        new
                        {
                            numberOfPiecesPerPackage = 10,
                            test = "test"
                        },
                        new
                        {
                            numberOfPiecesPerPackage = 12,
                            test = "test1"
                        }
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "\r\n{ \"test\": \"10, 12\"}",
                template: @"{% assign test = content.carrierSettings | map: ""numberOfPiecesPerPackage"" | join: "", ""%}
{ ""test"": ""{{test}}""}",
                localVariables: hash);
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
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                TestDate(_contextV20);
            });
        }

        public void TestDate(Context context)
        {
            Liquid.UseRubyDateFormat = false;
            DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

            Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00"), format: "MMMM"));
            Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date(context: context, input: DateTime.Parse("2006-06-05 10:00:00"), format: "MMMM"));
            Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date(context: context, input: DateTime.Parse("2006-07-05 10:00:00"), format: "MMMM"));

            Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date(context: context, input: "2006-05-05 10:00:00", format: "MMMM"));
            Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date(context: context, input: "2006-06-05 10:00:00", format: "MMMM"));
            Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "MMMM"));

            Assert.AreEqual("08/01/2006 10:00:00", StandardFilters.Date(context: context, input: "08/01/2006 10:00:00", format: string.Empty));
            Assert.AreEqual("08/02/2006 10:00:00", StandardFilters.Date(context: context, input: "08/02/2006 10:00:00", format: null));
            Assert.AreEqual(new DateTime(2006, 8, 3, 10, 0, 0).ToString(), StandardFilters.Date(context: context, input: new DateTime(2006, 8, 3, 10, 0, 0), format: string.Empty));
            Assert.AreEqual(new DateTime(2006, 8, 4, 10, 0, 0).ToString(), StandardFilters.Date(context: context, input: new DateTime(2006, 8, 4, 10, 0, 0), format: null));

            Assert.AreEqual(new DateTime(2006, 7, 5).ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "MM/dd/yyyy"));

            Assert.AreEqual(new DateTime(2004, 7, 16).ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "Fri Jul 16 2004 01:00:00", format: "MM/dd/yyyy"));

            Assert.AreEqual(null, StandardFilters.Date(context: context, input: null, format: "MMMM"));

            Assert.AreEqual("hi", StandardFilters.Date(context: context, input: "hi", format: "MMMM"));

            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "now", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "today", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "Now", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "Today", format: "MM/dd/yyyy"));

            Assert.AreEqual("345000", StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00.345"), format: "ffffff"));

            Template template = Template.Parse(@"{{ hi | date:""MMMM"" }}");
            Assert.AreEqual("hi", template.Render(Hash.FromAnonymousObject(new { hi = "hi" })));
        }

        [Test]
        public void TestDateV20()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var context = _contextV20;
                // Legacy parser doesn't except Unix Epoch https://github.com/dotliquid/dotliquid/issues/322
                Assert.AreEqual("0", StandardFilters.Date(context: context, input: 0, format: null));
                Assert.AreEqual("2147483648", StandardFilters.Date(context: context, input: 2147483648, format: null)); // Beyond Int32 boundary

                // Legacy parser loses specified offset https://github.com/dotliquid/dotliquid/issues/149
                var testDate = new DateTime(2006, 8, 4, 10, 0, 0);
                Assert.AreEqual(new DateTimeOffset(testDate).ToString("zzz"), StandardFilters.Date(context: context, input: new DateTimeOffset(testDate, TimeSpan.FromHours(-14)), format: "zzz"));

                // Legacy parser doesn't handle local offset & explicit offset in calculating epoch
                Liquid.UseRubyDateFormat = true;
                var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
                var unixEpochOffset = new DateTimeOffset(unixEpoch).Offset.TotalSeconds;
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = unixEpoch.ToUniversalTime() }));
                Helper.AssertTemplateResult(expected: unixEpochOffset.ToString(), template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = unixEpoch }));
                Helper.AssertTemplateResult(expected: unixEpochOffset.ToString(), template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = DateTime.SpecifyKind(unixEpoch.ToLocalTime(), DateTimeKind.Unspecified) }));
                Helper.AssertTemplateResult(expected: unixEpochOffset.ToString(), template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = new DateTimeOffset(unixEpoch) }));
                Helper.AssertTemplateResult(expected: unixEpochOffset.ToString(), template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = new DateTimeOffset(unixEpoch).ToOffset(TimeSpan.FromHours(-14)) }));

                // Legacy parser defaults to the .NET default format
                Assert.AreEqual(DateTime.Now.ToString(), StandardFilters.Date(context: context, input: "now", format: null));
                Assert.AreEqual(DateTime.Now.ToString(), StandardFilters.Date(context: context, input: "today", format: null));
                Assert.AreEqual(DateTime.Now.ToString(), StandardFilters.Date(context: context, input: "now", format: string.Empty));
                Assert.AreEqual(DateTime.Now.ToString(), StandardFilters.Date(context: context, input: "today", format: string.Empty));
            });
        }

        [Test]
        public void TestDateV21()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var context = _contextV21;
                var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
                Assert.AreEqual(unixEpoch.ToString("g"), StandardFilters.Date(context: context, input: 0, format: "g"));
                Assert.AreEqual(unixEpoch.AddSeconds(Int32.MaxValue).AddSeconds(1).ToString("g"), StandardFilters.Date(context: context, input: 2147483648, format: "g")); // Beyond Int32 boundary
                Assert.AreEqual(unixEpoch.AddSeconds(UInt32.MaxValue).AddSeconds(1).ToString("g"), StandardFilters.Date(context: context, input: 4294967296, format: "g")); // Beyond UInt32 boundary
                Helper.AssertTemplateResult(expected: unixEpoch.ToString("g"), template: "{{ 0 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: unixEpoch.AddSeconds(Int32.MaxValue).AddSeconds(1).ToString("g"), template: "{{ 2147483648 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: unixEpoch.AddSeconds(UInt32.MaxValue).AddSeconds(1).ToString("g"), template: "{{ 4294967296 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);

                var testDate = new DateTime(2006, 8, 4, 10, 0, 0, DateTimeKind.Unspecified);
                Assert.AreEqual("-14:00", StandardFilters.Date(context: context, input: new DateTimeOffset(testDate, TimeSpan.FromHours(-14)), format: "zzz"));
                Helper.AssertTemplateResult(expected: "+00:00", template: "{{ '" + testDate.ToString("u") + "' | date: 'zzz' }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "-14:00", template: "{{ '" + testDate.ToString("u").Replace("Z", "-14:00") + "' | date: 'zzz' }}", syntax: context.SyntaxCompatibilityLevel);

                Liquid.UseRubyDateFormat = true;
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = 0 }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "2147483648", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = 2147483648 }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "4294967296", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = 4294967296 }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = unixEpoch.ToUniversalTime() }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = unixEpoch }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = DateTime.SpecifyKind(unixEpoch, DateTimeKind.Unspecified) }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = new DateTimeOffset(unixEpoch) }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = new DateTimeOffset(unixEpoch).ToOffset(TimeSpan.FromHours(-14)) }), syntax: context.SyntaxCompatibilityLevel);

                Assert.AreEqual("now", StandardFilters.Date(context: context, input: "now", format: null));
                Assert.AreEqual("today", StandardFilters.Date(context: context, input: "today", format: null));
                Assert.AreEqual("now", StandardFilters.Date(context: context, input: "now", format: string.Empty));
                Assert.AreEqual("today", StandardFilters.Date(context: context, input: "today", format: string.Empty));

                TestDate(context);
            });
        }

        [Test]
        public void TestStrFTime()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var context = _contextV20;
                Liquid.UseRubyDateFormat = true;
                DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

                Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00"), format: "%B"));
                Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date(context: context, input: DateTime.Parse("2006-06-05 10:00:00"), format: "%B"));
                Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date(context: context, input: DateTime.Parse("2006-07-05 10:00:00"), format: "%B"));

                Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date(context: context, input: "2006-05-05 10:00:00", format: "%B"));
                Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date(context: context, input: "2006-06-05 10:00:00", format: "%B"));
                Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "%B"));

                Assert.AreEqual("05/07/2006 10:00:00", StandardFilters.Date(context: context, input: "05/07/2006 10:00:00", format: string.Empty));
                Assert.AreEqual("05/07/2006 10:00:00", StandardFilters.Date(context: context, input: "05/07/2006 10:00:00", format: null));
                Assert.AreEqual(new DateTime(2006, 8, 3, 10, 0, 0).ToString(), StandardFilters.Date(context: context, input: new DateTime(2006, 8, 3, 10, 0, 0), format: string.Empty));
                Assert.AreEqual(new DateTime(2006, 8, 4, 10, 0, 0).ToString(), StandardFilters.Date(context: context, input: new DateTime(2006, 8, 4, 10, 0, 0), format: null));

                Assert.AreEqual("07/05/2006", StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "%m/%d/%Y"));

                Assert.AreEqual("07/16/2004", StandardFilters.Date(context: context, input: "Fri Jul 16 2004 01:00:00", format: "%m/%d/%Y"));

                Assert.AreEqual(null, StandardFilters.Date(context: context, input: null, format: "%M"));

                Assert.AreEqual("hi", StandardFilters.Date(context: context, input: "hi", format: "%M"));

                Template template = Template.Parse(@"{{ hi | date:""%M"" }}");
                Assert.AreEqual("hi", template.Render(Hash.FromAnonymousObject(new { hi = "hi" })));

                Helper.AssertTemplateResult(
                    expected: "14, 16",
                    template: "{{ \"March 14, 2016\" | date: \"%d, %y\" }}",
                    syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(
                    expected: "Mar 14, 16",
                    template: "{{ \"March 14, 2016\" | date: \"%b %d, %y\" }}",
                    syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(
                    expected: $"This page was last updated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}.",
                    template: "This page was last updated at {{ 'now' | date: '%Y-%m-%d %H:%M' }}.",
                    syntax: context.SyntaxCompatibilityLevel);
            });
        }

        [Test]
        public void TestFirstLastUsingRuby()
        {
            var namingConvention = new NamingConventions.RubyNamingConvention();
            TestFirstLast(namingConvention, (name) => namingConvention.GetMemberName(name));
        }

        [Test]
        public void TestFirstLastUsingCSharp()
        {
            var namingConvention = new NamingConventions.CSharpNamingConvention();
            TestFirstLast(namingConvention, (name) => char.ToUpperInvariant(name[0]) + name.Substring(1));
        }

        private void TestFirstLast(NamingConventions.INamingConvention namingConvention, Func<string, string> filterNameFunc )
        {
            var splitFilter = filterNameFunc("split");
            var firstFilter = filterNameFunc("first");
            var lastFilter = filterNameFunc("last");

            Assert.Null(StandardFilters.First(null));
            Assert.Null(StandardFilters.Last(null));
            Assert.AreEqual(1, StandardFilters.First(new[] { 1, 2, 3 }));
            Assert.AreEqual(3, StandardFilters.Last(new[] { 1, 2, 3 }));
            Assert.Null(StandardFilters.First(new object[] { }));
            Assert.Null(StandardFilters.Last(new object[] { }));

            Helper.AssertTemplateResult(
                expected: ".",
                template: "{{ 'Ground control to Major Tom.' | " + lastFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "Tom.",
                template: "{{ 'Ground control to Major Tom.' | "+ splitFilter + ": ' ' | " + lastFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "tiger",
                template: "{% assign my_array = 'zebra, octopus, giraffe, tiger' | " + splitFilter + ": ', ' %}{{ my_array." + lastFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "There goes a tiger!",
                template: "{% assign my_array = 'zebra, octopus, giraffe, tiger' | " + splitFilter + ": ', ' %}{% if my_array." + lastFilter + " == 'tiger' %}There goes a tiger!{% endif %}",
                namingConvention: namingConvention);

            Helper.AssertTemplateResult(
                expected: "G",
                template: "{{ 'Ground control to Major Tom.' | " + firstFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "Ground",
                template: "{{ 'Ground control to Major Tom.' | " + splitFilter + ": ' ' | " + firstFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "zebra",
                template: "{% assign my_array = 'zebra, octopus, giraffe, tiger' | " + splitFilter + ": ', ' %}{{ my_array." + firstFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "There goes a zebra!",
                template: "{% assign my_array = 'zebra, octopus, giraffe, tiger' | " + splitFilter + ": ', ' %}{% if my_array." + firstFilter + " == 'zebra' %}There goes a zebra!{% endif %}",
                namingConvention: namingConvention);
        }

        [Test]
        public void TestReplace()
        {
            TestReplace(_contextV20);
        }

        public void TestReplace(Context context)
        {
            Assert.Null(StandardFilters.Replace(context: context, input: null, @string: "a", replacement: "b"));
            Assert.AreEqual(expected: "", actual: StandardFilters.Replace(context: context, input: "", @string: "a", replacement: "b"));
            Assert.AreEqual(expected: "a a a a", actual: StandardFilters.Replace(context: context, input: "a a a a", @string: null, replacement: "b"));
            Assert.AreEqual(expected: "a a a a", actual: StandardFilters.Replace(context: context, input: "a a a a", @string: "", replacement: "b"));
            Assert.AreEqual(expected: "b b b b", actual: StandardFilters.Replace(context: context, input: "a a a a", @string: "a", replacement: "b"));

            Assert.AreEqual(expected: "Tesvalue\\\"", actual: StandardFilters.Replace(context: context, input: "Tesvalue\"", @string: "\"", replacement: "\\\""));
            Helper.AssertTemplateResult(expected: "Tesvalue\\\"", template: "{{ 'Tesvalue\"' | replace: '\"', '\\\"' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(
                expected: "Tesvalue\\\"",
                template: "{{ context | replace: '\"', '\\\"' }}",
                localVariables: Hash.FromAnonymousObject(new { context = "Tesvalue\"" }),
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestReplaceRegexV20()
        {
            var context = _contextV20;
            Assert.AreEqual(expected: "b b b b", actual: StandardFilters.Replace(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"));
        }

        [Test]
        public void TestReplaceRegexV21()
        {
            var context = _contextV21;
            Assert.AreEqual(expected: "a A A a", actual: StandardFilters.Replace(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"));
            TestReplace(context);
        }

        [Test]
        public void TestReplaceChain()
        {
            var assign = @"{%assign az='azerty'%}";
            Helper.AssertTemplateResult("qzerty", assign + "{{az |replace: 'a','q'}}");
            Helper.AssertTemplateResult("q zerty", assign + "{{az |replace: 'a','q '}}");
            Helper.AssertTemplateResult("qw erty", assign + "{{az |replace: 'a','q'   |replace: 'z','w '}}");
            Helper.AssertTemplateResult("q werty", assign + "{{az |replace: 'a','q '  |replace: 'z','w'}}");
            Helper.AssertTemplateResult("q rwerty", assign + "{{az |replace: 'a','q r' |replace: 'z','w'}}");
            Helper.AssertTemplateResult(" qwerty", assign + "{{az |replace: 'a',' q'  |replace: 'z','w'}}");
        }

        [Test]
        public void TestReplaceFirst()
        {
            TestReplaceFirst(_contextV20);
        }

        public void TestReplaceFirst(Context context)
        {
            Assert.Null(StandardFilters.ReplaceFirst(context: context, input: null, @string: "a", replacement: "b"));
            Assert.AreEqual("", StandardFilters.ReplaceFirst(context: context, input: "", @string: "a", replacement: "b"));
            Assert.AreEqual("a a a a", StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: null, replacement: "b"));
            Assert.AreEqual("a a a a", StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: "", replacement: "b"));
            Assert.AreEqual("b a a a", StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: "a", replacement: "b"));
            Helper.AssertTemplateResult(expected: "b a a a", template: "{{ 'a a a a' | replace_first: 'a', 'b' }}", syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestReplaceFirstRegexV20()
        {
            var context = _contextV20;
            Assert.AreEqual(expected: "b A A a", actual: StandardFilters.ReplaceFirst(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"));
        }

        [Test]
        public void TestReplaceFirstRegexV21()
        {
            var context = _contextV21;
            Assert.AreEqual(expected: "a A A a", actual: StandardFilters.ReplaceFirst(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"));
            TestReplaceFirst(context);
        }

        [Test]
        public void TestRemove()
        {
            TestRemove(_contextV20);
        }

        public void TestRemove(Context context)
        {

            Assert.AreEqual("   ", StandardFilters.Remove("a a a a", "a"));
            Assert.AreEqual("a a a", StandardFilters.RemoveFirst(context: context, input: "a a a a", @string: "a "));
            Helper.AssertTemplateResult(expected: "a a a", template: "{{ 'a a a a' | remove_first: 'a ' }}", syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestRemoveFirstRegexV20()
        {
            var context = _contextV20;
            Assert.AreEqual(expected: "r. Jones", actual: StandardFilters.RemoveFirst(context: context, input: "Mr. Jones", @string: "."));
        }

        [Test]
        public void TestRemoveFirstRegexV21()
        {
            var context = _contextV21;
            Assert.AreEqual(expected: "Mr Jones", actual: StandardFilters.RemoveFirst(context: context, input: "Mr. Jones", @string: "."));
            TestRemove(context);
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
            TestPlus(_contextV20);
        }

        private void TestPlus(Context context)
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult(expected: "2", template: "{{ 1 | plus:1 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "5.5", template: "{{ 2  | plus:3.5 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "5.5", template: "{{ 3.5 | plus:2 }}", syntax: context.SyntaxCompatibilityLevel);

                // Test that decimals are not introducing rounding-precision issues
                Helper.AssertTemplateResult(expected: "148397.77", template: "{{ 148387.77 | plus:10 }}", syntax: context.SyntaxCompatibilityLevel); 

                Helper.AssertTemplateResult(
                    expected: "2147483648",
                    template: "{{ i | plus: i2 }}",
                    localVariables: Hash.FromAnonymousObject(new { i = (int)Int32.MaxValue, i2 = (Int64)1 }),
                    syntax: context.SyntaxCompatibilityLevel);
            }
        }

        [Test]
        public void TestPlusStringV20()
        {
            var context = _contextV20;
            Helper.AssertTemplateResult(expected: "11", template: "{{ '1' | plus: 1 }}", syntax: context.SyntaxCompatibilityLevel);
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = context.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 1 | plus: '1' }}").Render(renderParams));
        }

        [Test]
        public void TestPlusStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "2", template: "{{ '1' | plus: 1 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "2", template: "{{ 1 | plus: '1' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "2", template: "{{ '1' | plus: '1' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "5.5", template: "{{ 2 | plus: '3.5' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "5.5", template: "{{ '3.5' | plus: 2 }}", syntax: context.SyntaxCompatibilityLevel);
            TestPlus(context);
        }

        [Test]
        public void TestMinus()
        {
            TestMinus(_contextV20);
        }

        private void TestMinus(Context context)
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult(expected: "4", template: "{{ input | minus:operand }}", localVariables: Hash.FromAnonymousObject(new { input = 5, operand = 1 }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "-1.5", template: "{{ 2  | minus:3.5 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "1.5", template: "{{ 3.5 | minus:2 }}", syntax: context.SyntaxCompatibilityLevel);
            }
        }

        [Test]
        public void TestMinusStringV20()
        {
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = _contextV20.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ '2' | minus: 1 }}").Render(renderParams));
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 2 | minus: '1' }}").Render(renderParams));
        }

        [Test]
        public void TestMinusStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "1", template: "{{ '2' | minus: 1 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "1", template: "{{ 2 | minus: '1' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "-1.5", template: "{{ 2 | minus: '3.5' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "-1.5", template: "{{ '2.5' | minus: 4 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "-1", template: "{{ '2.5' | minus: '3.5' }}", syntax: context.SyntaxCompatibilityLevel);
            TestMinus(context);
        }

        [Test]
        public void TestPlusCombinedWithMinus()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                // This detects rounding issues not visible with single operation.
                Helper.AssertTemplateResult("0.1", "{{ 0.1 | plus: 10 | minus: 10 }}");
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
        public void TestCeil()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("2", "{{ 1.2 | ceil }}");
                Helper.AssertTemplateResult("2", "{{ 2.0 | ceil }}");
                Helper.AssertTemplateResult("184", "{{ 183.357 | ceil }}");
                Helper.AssertTemplateResult("4", "{{ \"3.5\" | ceil }}");

                Assert.Null(StandardFilters.Ceil(""));
                Assert.Null(StandardFilters.Ceil("two"));
            }
        }

        [Test]
        public void TestFloor()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("1", "{{ 1.2 | floor }}");
                Helper.AssertTemplateResult("2", "{{ 2.0 | floor }}");
                Helper.AssertTemplateResult("183", "{{ 183.357 | floor }}");
                Helper.AssertTemplateResult("3", "{{ \"3.5\" | floor }}");

                Assert.Null(StandardFilters.Floor(""));
                Assert.Null(StandardFilters.Floor("two"));
            }
        }

        [Test]
        public void TestTimes()
        {
            TestTimes(_contextV20);
        }

        private void TestTimes(Context context)
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult(expected: "12", template: "{{ 3 | times:4 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "125", template: "{{ 10 | times:12.5 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "125", template: "{{ 10.0 | times:12.5 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "125", template: "{{ 12.5 | times:10 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "125", template: "{{ 12.5 | times:10.0 }}", syntax: context.SyntaxCompatibilityLevel);

                // Test against overflows when we try to be precise but the result exceeds the range of the input type.
                Helper.AssertTemplateResult(
                    expected: ((double)((decimal.MaxValue / 100) + (decimal).1) * (double)((decimal.MaxValue / 100) + (decimal).1)).ToString(),
                    template: $"{{{{ {(decimal.MaxValue / 100) + (decimal).1} | times:{(decimal.MaxValue / 100) + (decimal).1} }}}}",
                    syntax: context.SyntaxCompatibilityLevel);

                // Test against overflows going beyond the double precision float type's range
                Helper.AssertTemplateResult(
                    expected: double.NegativeInfinity.ToString(),
                    template: $"{{{{ 12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.0 | times:-12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.0 }}}}",
                    syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(
                    expected: double.PositiveInfinity.ToString(),
                    template: $"{{{{ 12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.0 | times:12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.0 }}}}",
                    syntax: context.SyntaxCompatibilityLevel);

                // Ensures no underflow exception is thrown when the result doesn't fit the precision of double.
                Helper.AssertTemplateResult(expected: "0",
                    template: $"{{{{ 0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001 | times:0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001 }}}}",
                    syntax: context.SyntaxCompatibilityLevel);
            }

            Assert.AreEqual(8.43, StandardFilters.Times(context: context, input: 0.843m, operand: 10));
            Assert.AreEqual(412, StandardFilters.Times(context: context, input: 4.12m, operand: 100));
            Assert.AreEqual(7556.3, StandardFilters.Times(context: context, input: 7.5563m, operand: 1000));
        }

        [Test]
        public void TestTimesStringV20()
        {
            var context = _contextV20;
            Helper.AssertTemplateResult(expected: "foofoofoofoo", template: "{{ 'foo' | times:4 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "3333", template: "{{ '3' | times:4 }}", syntax: context.SyntaxCompatibilityLevel);
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = context.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 3 | times: '4' }}").Render(renderParams));
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ '3' | times: '4' }}").Render(renderParams));
        }

        [Test]
        public void TestTimesStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "12", template: "{{ '3' | times: 4 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "12", template: "{{ 3 | times: '4' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "12", template: "{{ '3' | times: '4' }}", syntax: context.SyntaxCompatibilityLevel);
            TestTimes(context);
        }

        [Test]
        public void TestAppend()
        {
            Hash assigns = Hash.FromAnonymousObject(new { a = "bc", b = "d" });
            Helper.AssertTemplateResult(expected: "bcd", template: "{{ a | append: 'd'}}", localVariables: assigns);
            Helper.AssertTemplateResult(expected: "bcd", template: "{{ a | append: b}}", localVariables: assigns);
            Helper.AssertTemplateResult(expected: "/my/fancy/url.html", template: "{{ '/my/fancy/url' | append: '.html' }}");
            Helper.AssertTemplateResult(expected: "website.com/index.html", template: "{% assign filename = '/index.html' %}{{ 'website.com' | append: filename }}");
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
            TestDividedBy(_contextV20);
        }

        private void TestDividedBy(Context context)
        {
            Helper.AssertTemplateResult(expected: "4", template: "{{ 12 | divided_by:3 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "4", template: "{{ 14 | divided_by:3 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "5", template: "{{ 15 | divided_by:3 }}", syntax: context.SyntaxCompatibilityLevel);
            Assert.Null(StandardFilters.DividedBy(context: context, input: null, operand: 3));
            Assert.Null(StandardFilters.DividedBy(context: context, input: 4, operand: null));

            // Ensure we preserve floating point behavior for division by zero, and don't start throwing exceptions.
            Helper.AssertTemplateResult(expected: double.PositiveInfinity.ToString(), template: "{{ 1.0 | divided_by:0.0 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: double.NegativeInfinity.ToString(), template: "{{ -1.0 | divided_by:0.0 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "NaN", template: "{{ 0.0 | divided_by:0.0 }}", syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestDividedByStringV20()
        {
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = _contextV20.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ '12' | divided_by: 3 }}").Render(renderParams));
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 12 | divided_by: '3' }}").Render(renderParams));
        }

        [Test]
        public void TestDividedByStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "4", template: "{{ '12' | divided_by: 3 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "4", template: "{{ 12 | divided_by: '3' }}", syntax: context.SyntaxCompatibilityLevel);
            TestDividedBy(context);
        }

        [Test]
        public void TestInt32DividedByInt64()
        {
            int a = 20;
            long b = 5;
            var c = a / b;
            Assert.AreEqual(c, (long)4);


            Hash assigns = Hash.FromAnonymousObject(new { a = a, b = b });
            Helper.AssertTemplateResult("4", "{{ a | divided_by:b }}", assigns);
        }

        [Test]
        public void TestModulo()
        {
            TestModulo(_contextV20);
        }

        private void TestModulo(Context context)
        {
            Helper.AssertTemplateResult(expected: "1", template: "{{ 3 | modulo:2 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "7.77", template: "{{ 148387.77 | modulo:10 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "5.32", template: "{{ 3455.32 | modulo:10 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "3.12", template: "{{ 23423.12 | modulo:10 }}", syntax: context.SyntaxCompatibilityLevel);
            Assert.Null(StandardFilters.Modulo(context: context, input: null, operand: 3));
            Assert.Null(StandardFilters.Modulo(context: context, input: 4, operand: null));
        }

        public void TestModuloStringV20()
        {
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = _contextV20.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ '3' | modulo: 2 }}").Render(renderParams));
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 3 | modulo: '2' }}").Render(renderParams));
        }

        [Test]
        public void TestModuloStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "1", template: "{{ '3' | modulo: 2 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "1", template: "{{ 3 | modulo: '2' }}", syntax: context.SyntaxCompatibilityLevel);
            TestModulo(context);
        }

        [Test]
        public void TestUrlencode()
        {
            Assert.AreEqual("http%3A%2F%2Fdotliquidmarkup.org%2F", StandardFilters.UrlEncode("http://dotliquidmarkup.org/"));
            Assert.AreEqual("Tetsuro+Takara", StandardFilters.UrlEncode("Tetsuro Takara"));
            Assert.AreEqual("john%40liquid.com", StandardFilters.UrlEncode("john@liquid.com"));
            Assert.AreEqual(null, StandardFilters.UrlEncode(null));
        }
        
        [Test]
        public void TestUrldecode()
        {
            Assert.AreEqual("'Stop!' said Fred", StandardFilters.UrlDecode("%27Stop%21%27+said+Fred"));
            Assert.AreEqual(null, StandardFilters.UrlDecode(null));
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
        public void TestCapitalizeV20()
        {
            var context = _contextV20;
            Assert.AreEqual(null, StandardFilters.Capitalize(context: context, input: null));
            Assert.AreEqual("", StandardFilters.Capitalize(context: context, input: ""));
            Assert.AreEqual(" ", StandardFilters.Capitalize(context: context, input: " "));
            Assert.AreEqual("That Is One Sentence.", StandardFilters.Capitalize(context: context, input: "That is one sentence."));

            Helper.AssertTemplateResult(
                expected: "Title",
                template: "{{ 'title' | capitalize }}",
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestCapitalizeV21()
        {
            var context = _contextV21;
            Assert.AreEqual(null, StandardFilters.Capitalize(context: context, input: null));
            Assert.AreEqual("", StandardFilters.Capitalize(context: context, input: ""));
            Assert.AreEqual(" ", StandardFilters.Capitalize(context: context, input: " "));
            Assert.AreEqual(" My boss is Mr. Doe.", StandardFilters.Capitalize(context: context, input: " my boss is Mr. Doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my great title' | capitalize }}",
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestUniq()
        {
            CollectionAssert.AreEqual(new[] { "ants", "bugs", "bees" }, StandardFilters.Uniq(new string[] { "ants", "bugs", "bees", "bugs", "ants" }));
            CollectionAssert.AreEqual(new string[] {}, StandardFilters.Uniq(new string[] {}));
            Assert.AreEqual(null, StandardFilters.Uniq(null));
            Assert.AreEqual(new List<object> {5}, StandardFilters.Uniq(5));
        }

        [Test]
        public void TestAbs()
        {
            Assert.AreEqual(0, StandardFilters.Abs("notNumber"));
            Assert.AreEqual(10, StandardFilters.Abs(10));
            Assert.AreEqual(5, StandardFilters.Abs(-5));
            Assert.AreEqual(19.86, StandardFilters.Abs(19.86));
            Assert.AreEqual(19.86, StandardFilters.Abs(-19.86));
            Assert.AreEqual(10, StandardFilters.Abs("10"));
            Assert.AreEqual(5, StandardFilters.Abs("-5"));
            Assert.AreEqual(30.60, StandardFilters.Abs("30.60"));
            Assert.AreEqual(0, StandardFilters.Abs("30.60a"));

            Helper.AssertTemplateResult(
                expected: "17",
                template: "{{ -17 | abs }}");
            Helper.AssertTemplateResult(
                expected: "17",
                template: "{{ 17 | abs }}");
            Helper.AssertTemplateResult(
                expected: "4",
                template: "{{ 4 | abs }}");
            Helper.AssertTemplateResult(
                expected: "19.86",
                template: "{{ '-19.86' | abs }}");
        }

        [Test]
        public void TestAtLeast()
        {
            Assert.AreEqual("notNumber", StandardFilters.AtLeast("notNumber", 5));
            Assert.AreEqual(5, StandardFilters.AtLeast(5, 5));
            Assert.AreEqual(5, StandardFilters.AtLeast(3, 5));
            Assert.AreEqual(6, StandardFilters.AtLeast(6, 5));
            Assert.AreEqual(10, StandardFilters.AtLeast(10, 5));
            Assert.AreEqual(9.85, StandardFilters.AtLeast(9.85, 5));
            Assert.AreEqual(5, StandardFilters.AtLeast(3.56, 5));
            Assert.AreEqual(10, StandardFilters.AtLeast("10", 5));
            Assert.AreEqual(5, StandardFilters.AtLeast("4", 5));
            Assert.AreEqual("10a", StandardFilters.AtLeast("10a", 5));
            Assert.AreEqual("4b", StandardFilters.AtLeast("4b", 5));

            Helper.AssertTemplateResult(
                expected: "5",
                template: "{{ 4 | at_least: 5 }}");
            Helper.AssertTemplateResult(
                expected: "4",
                template: "{{ 4 | at_least: 3 }}");
        }
        
        [Test]
        public void TestAtMost()
        {
            Assert.AreEqual("notNumber", StandardFilters.AtMost("notNumber", 5));
            Assert.AreEqual(5, StandardFilters.AtMost(5, 5));
            Assert.AreEqual(3, StandardFilters.AtMost(3, 5));
            Assert.AreEqual(5, StandardFilters.AtMost(6, 5));
            Assert.AreEqual(5, StandardFilters.AtMost(10, 5));
            Assert.AreEqual(5, StandardFilters.AtMost(9.85, 5));
            Assert.AreEqual(3.56, StandardFilters.AtMost(3.56, 5));
            Assert.AreEqual(5, StandardFilters.AtMost("10", 5));
            Assert.AreEqual(4, StandardFilters.AtMost("4", 5));
            Assert.AreEqual("4a", StandardFilters.AtMost("4a", 5));
            Assert.AreEqual("10b", StandardFilters.AtMost("10b", 5));

            Helper.AssertTemplateResult(
                expected: "4",
                template: "{{ 4 | at_most: 5 }}");
            Helper.AssertTemplateResult(
                expected: "3",
                template: "{{ 4 | at_most: 3 }}");
        }
        
        [Test]
        public void TestCompact()
        {
            CollectionAssert.AreEqual(new[] { "business", "celebrities", "lifestyle", "sports", "technology" }, StandardFilters.Compact(new string[] { "business", null, "celebrities", null, null, "lifestyle", "sports", null, "technology", null}));
            CollectionAssert.AreEqual(new[] { "business", "celebrities"}, StandardFilters.Compact(new string[] { "business", "celebrities" }));
            Assert.AreEqual(new List<object> { 5 }, StandardFilters.Compact(5));
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.Compact(new string[] { }));
            Assert.AreEqual(null, StandardFilters.Compact(null));

            var siteAnonymousObject = new
            {
                site = new
                {
                    pages = new[]
                    {
                        new { title = "Shopify", category = "business" },
                        new { title = "Rihanna", category = "celebrities" },
                        new { title = "foo", category = null as string },
                        new { title = "World traveler", category = "lifestyle" },
                        new { title = "Soccer", category = "sports" },
                        new { title = "foo", category = null as string },
                        new { title = "Liquid", category = "technology" },
                    }
                }
            };

            Helper.AssertTemplateResult(
                expected: @"
- business
- celebrities
- 
- lifestyle
- sports
- 
- technology
",
                template: @"{% assign site_categories = site.pages | map: 'category' %}
{% for category in site_categories %}- {{ category }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(siteAnonymousObject));

            Helper.AssertTemplateResult(
                expected: @"
- business
- celebrities
- lifestyle
- sports
- technology
",
                template: @"{% assign site_categories = site.pages | map: 'category' | compact %}
{% for category in site_categories %}- {{ category }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(siteAnonymousObject));
        }
    }
}
