using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class MarkupParserTests
    {
        private readonly MarkupParser MarkupParser = new MarkupParser();


        [Test]
        public void TestNameExtraction()
        {
            var result = MarkupParser.Parse("hello");
            Assert.AreEqual("hello", result.Name);
        }

        [Test]
        public void TestFilterExtraction()
        {
            var result = MarkupParser.Parse("hello | textileze");
            Assert.AreEqual("hello", result.Name);
            Assert.AreEqual(1, result.Filters.Count);
            Assert.AreEqual("textileze", result.Filters[0].Name);
            Assert.AreEqual(0, result.Filters[0].Arguments.Length);
        }


        [Test, TestCaseSource("FilterCases")]
        public void TestFilters(String markup, String expectedName, FilterRequest[] filters)
        {
            var result = MarkupParser.Parse(markup);
            Assert.AreEqual(expectedName, result.Name);
            AssertFiltersAreEqual(filters, result.Filters);

        }

        [Test, TestCaseSource("NameCases")]
        public void TestNames(String markup, String expectedName)
        {
            var result = MarkupParser.Parse(markup);
            Assert.AreEqual(expectedName, result.Name);
        }

        




        // Helpers
        private static void AssertFiltersAreEqual(FilterRequest[] expected,
                                                 IList<FilterRequest> actual)
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

        // Test Cases
        public static Object[] FilterCases =
            {
                new Object[]
                    {
                        "hello | textileze | paragraph", "hello",
                        new[]
                            {
                                new FilterRequest("textileze", new string[] {}),
                                new FilterRequest("paragraph", new string[] {})
                            }
                    },
                new Object[]
                    {
                        " hello | strftime: '%Y'",
                        "hello",
                        new[] {new FilterRequest("strftime", new[] {"'%Y'"})}
                    },
                new Object[]
                    {
                        " 'typo' | link_to: 'Typo', true ",
                        "'typo'",
                        new[] {new FilterRequest("link_to", new[] {"'Typo'", "true"})}
                    },
                new Object[]
                    {
                        " 'typo' | link_to: 'Typo', false ",
                        "'typo'",
                        new[] {new FilterRequest("link_to", new[] {"'Typo'", "false"})}
                    },
                new Object[]
                    {
                        " 'foo' | repeat: 3 ",
                        "'foo'",
                        new[] {new FilterRequest("repeat", new[] {"3"})}
                    },
                new Object[]
                    {
                        " 'foo' | repeat: 3, 3 ",
                        "'foo'",
                        new[] {new FilterRequest("repeat", new[] {"3", "3"})}
                    },
                new Object[]
                    {
                        " 'foo' | repeat: 3, 3, 3 ",
                        "'foo'",
                        new[] {new FilterRequest("repeat", new[] {"3", "3", "3"})}
                    },
                new Object[]
                    {
                        " hello | strftime: '%Y, okay?'",
                        "hello",
                        new[] {new FilterRequest("strftime", new[] {"'%Y, okay?'"})}
                    },
                new Object[]
                    {
                        " hello | things: \"%Y, okay?\", 'the other one'",
                        "hello",
                        new[] {new FilterRequest("things", new[] {"\"%Y, okay?\"", "'the other one'"})}
                    },
                new Object[] 
                    {
                       " '2006-06-06' | date: \"%m/%d/%Y\"",
                       "'2006-06-06'",
                        new[] {new FilterRequest("date", new[] {"\"%m/%d/%Y\""})}
                    },
                new Object[] 
                    {
                       "hello | textileze | paragraph",
                       "hello",
                        new[] {
                            new FilterRequest("textileze", new string[] {}),
                            new FilterRequest("paragraph", new string[] {})
                        }
                    },
                new Object[] 
                    {
                       "hello|textileze|paragraph",
                       "hello",
                        new[] {
                            new FilterRequest("textileze", new string[] {}),
                            new FilterRequest("paragraph", new string[] {})
                        }
                    },
                new Object[]
                    {
                        "http://disney.com/logo.gif | image: 'med' ",
                        "http://disney.com/logo.gif",
                        new[] { new FilterRequest("image", new[] { "'med'" }) }
                    }

            };

        
        public static Object[] NameCases =
            {
                new Object[] {" 'hello' ", "'hello'"},
                new Object[] {" \"hello\" ", "\"hello\""},
                new Object[] {" 1000 ", "1000"},
                new Object[] {" 1000.01 ", "1000.01"},
                new Object[] {" 'hello! $!@.;\"ddasd\" ' ", "'hello! $!@.;\"ddasd\" '"},
                new Object[] {" test.test ", "test.test"}
            };
    
    }
}
