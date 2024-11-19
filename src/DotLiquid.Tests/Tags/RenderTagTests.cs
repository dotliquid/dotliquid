using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class RenderTagTests
    {
        private class CountingFileSystem : IFileSystem
        {
            public int Count { get; private set; }

            public string ReadTemplateFile(Context context, string templateName)
            {
                Count++;
                return "from CountingFileSystem";
            }
        }

        private class TestEnumerable : Drop, IEnumerable<IDictionary<string, int>>
        {
            private readonly IList<IDictionary<string, int>> _data = new List<IDictionary<string, int>>
            {
                new Dictionary<string, int> { { "foo", 1 }, { "bar", 2 } },
                new Dictionary<string, int> { { "foo", 2 }, { "bar", 1 } },
                new Dictionary<string, int> { { "foo", 3 }, { "bar", 3 } },
            };

            public IEnumerator<IDictionary<string, int>> GetEnumerator() => _data.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public string Name => nameof(TestEnumerable);
        }

        [Test]
        public void TestRenderWithNoArguments()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["source"] = "rendered content"
            }, () =>
            {
                Helper.AssertTemplateResult("rendered content", "{% render 'source' %}");
            });
        }

        [Test]
        public void TestRenderTagLookForFileSystemInRegisterFirst()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["pick_a_source"] = "from global file system"
            }, () =>
            {
                var context = new Context(CultureInfo.InvariantCulture);
                context.Registers["file_system"] = new Helper.DictionaryFileSystem(new Dictionary<string, string>
                {
                    ["pick_a_source"] = "from register file system"
                });
                Assert.AreEqual("from register file system", Template.Parse("{% render 'pick_a_source' %}").Render(RenderParameters.FromContext(context, context.FormatProvider)));
            });
        }

        [Test]
        public void TestRenderPassesNamedArgumentsIntoInnerScope()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["product"] = "{{ inner_product.title }}"
            }, () =>
            {
                Helper.AssertTemplateResult("My Product", "{% render 'product', inner_product: outer_product %}", Hash.FromAnonymousObject(new
                {
                    outer_product = new { title = "My Product" }
                }));
            });
        }

        [Test]
        public void TestRenderAcceptsLiteralsAsArguments()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["snippet"] = "{{ price }}"
            }, () =>
            {
                Helper.AssertTemplateResult("123", "{% render 'snippet', price: 123 %}");
            });
        }

        [Test]
        public void TestRenderAcceptsMultipleNamedArguments()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["snippet"] = "{{ one }} {{ two }}"
            }, () =>
            {
                Helper.AssertTemplateResult("1 2", "{% render 'snippet', one: 1, two: 2 %}");
            });
        }

        [Test]
        public void TestRenderDoesNotInheritParentScopeVariables()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["snippet"] = "{{ outer_variable }}"
            }, () =>
            {
                Helper.AssertTemplateResult("", "{% assign outer_variable = 'should not be visible' %}{% render 'snippet' %}");
            });
        }

        [Test]
        public void TestRenderDoesNotInheritVariableWithSameNameAsSnippet()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["snippet"] = "{{ outer_variable }}"
            }, () =>
            {
                Helper.AssertTemplateResult("", "{% assign snippet = 'should not be visible' %}{% render 'snippet' %}");
            });
        }

        [Test]
        public void TestRenderDoesNotMutateParentScope()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["snippet"] = "{% assign inner = 1 %}"
            }, () =>
            {
                Helper.AssertTemplateResult("", "{% render 'snippet' %}{{ inner }}");
            });
        }

        [Test]
        public void TestNestedRenderTag()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["one"] = "one {% render 'two' %}",
                ["two"] = "two"
            }, () =>
            {
                Helper.AssertTemplateResult("one two", "{% render 'one' %}");
            });
        }

        [Test]
        public void TestRecursivelyRenderedTemplateDoesNotProduceEndlessLoop()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["loop"] = "{% render 'loop' %}",
            }, () =>
            {
                Assert.Throws<StackLevelException>(() => Template.Parse("{% render 'loop' %}").Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    ErrorsOutputMode = ErrorsOutputMode.Rethrow
                }));
            });
        }

        [Test]
        public void TestDynamicallyChoosenTemplatesAreNotAllowed()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse("{% assign name = 'snippet' %}{% render name %}"));
        }

        [Test]
        public void TestIncludeTagCachesSecondReadOfSamePartial()
        {
            var fs = new CountingFileSystem();
            Helper.WithFileSystem(fs, () =>
            {
                Helper.AssertTemplateResult("from CountingFileSystemfrom CountingFileSystem", "{% render 'snippet' %}{% render 'snippet' %}");
                Assert.AreEqual(1, fs.Count);
            });
        }

        [Test]
        public void TestRenderTagDoesntCachePartialsAcrossRenders()
        {
            var fs = new CountingFileSystem();
            Helper.WithFileSystem(fs, () =>
            {
                Helper.AssertTemplateResult("from CountingFileSystem", "{% include 'pick_a_source' %}");
                Assert.AreEqual(1, fs.Count);
                Helper.AssertTemplateResult("from CountingFileSystem", "{% include 'pick_a_source' %}");
                Assert.AreEqual(2, fs.Count);
            });
        }

        [Test]
        public void TestRenderTagWithinIfStatement()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["snippet"] = "my message",
            }, () =>
            {
                Helper.AssertTemplateResult("my message", "{% if true %}{% render 'snippet' %}{% endif %}");
            });
        }

        [Test]
        public void TestBreakThroughRender()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["break"] = "{% break %}",
            }, () =>
            {
                Helper.AssertTemplateResult("1", "{% for i in (1..3) %}{{ i }}{% break %}{{ i }}{% endfor %}");
                Helper.AssertTemplateResult("112233", "{% for i in (1..3) %}{{ i }}{% render 'break' %}{{ i }}{% endfor %}");
            });
        }

        [Test]
        public void TestIncrementIsIsolatedBetweenRenders()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["incr"] = "{% increment foo %}",
            }, () =>
            {
                Helper.AssertTemplateResult("010", "{% increment foo %}{% increment foo %}{% render 'incr' %}");
            });
        }

        [Test]
        public void TestDecrementIsIsolatedBetweenRenders()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["decr"] = "{% decrement foo %}",
            }, () =>
            {
                Helper.AssertTemplateResult("-1-2-1", "{% decrement foo %}{% decrement foo %}{% render 'decr' %}");
            });
        }

        [Test]
        public void TestIncludesWillNotRenderInsideRenderTag()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["test_include"] = "{% include 'foo' %}",
                ["foo"] = "bar",
            }, () =>
            {
                Helper.AssertTemplateResult("Liquid error: include usage is not allowed in this context", "{% render 'test_include' %}");
            });
        }

        [Test]
        public void TestIncludesWillNotRenderInsideNestedSiblingTags()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["nested_render_with_sibling_include"] = "{% render 'test_include' %}{% include 'foo' %}",
                ["test_include"] = "{% include 'foo' %}",
                ["foo"] = "bar",
            }, () =>
            {
                Helper.AssertTemplateResult("Liquid error: include usage is not allowed in this context" +
                    "Liquid error: include usage is not allowed in this context",
                    "{% render 'nested_render_with_sibling_include' %}");
            });
        }

        [Test]
        public void TestRenderTagWith()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["product"] = "Product: {{ product.title }} ",
                ["product_alias"] = "Product: {{ product.title }} ",
            }, () =>
            {
                var vars = Hash.FromAnonymousObject(new
                {
                    products = new[]
                    {
                        new { title = "Draft 151cm" },
                        new { title = "Element 155cm" },
                    }
                });
                Helper.AssertTemplateResult("Product: Draft 151cm ", "{% render 'product' with products[0] %}", vars);
            });
        }

        [Test]
        public void TestRenderTagWithAlias()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["product"] = "Product: {{ product.title }} ",
                ["product_alias"] = "Product: {{ product.title }} ",
            }, () =>
            {
                var vars = Hash.FromAnonymousObject(new
                {
                    products = new[]
                    {
                        new { title = "Draft 151cm" },
                        new { title = "Element 155cm" },
                    }
                });
                Helper.AssertTemplateResult("Product: Draft 151cm ", "{% render 'product_alias' with products[0] as product %}", vars);
            });
        }

        [Test]
        public void TestRenderTagForAlias()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["product"] = "Product: {{ product.title }} ",
                ["product_alias"] = "Product: {{ product.title }} ",
            }, () =>
            {
                var vars = Hash.FromAnonymousObject(new
                {
                    products = new[]
                    {
                        new { title = "Draft 151cm" },
                        new { title = "Element 155cm" },
                    }
                });
                Helper.AssertTemplateResult("Product: Draft 151cm Product: Element 155cm ", "{% render 'product_alias' for products as product %}", vars);
            });
        }

        [Test]
        public void TestRenderTagFor()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["product"] = "Product: {{ product.title }} ",
                ["product_alias"] = "Product: {{ product.title }} ",
            }, () =>
            {
                var vars = Hash.FromAnonymousObject(new
                {
                    products = new[]
                    {
                        new { title = "Draft 151cm" },
                        new { title = "Element 155cm" },
                    }
                });
                Helper.AssertTemplateResult("Product: Draft 151cm Product: Element 155cm ", "{% render 'product' for products %}", vars);
            });
        }

        [Test]
        public void TestRenderTagForloop()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["product"] = "Product: {{ product.title }} {% if forloop.first %}first{% endif %} {% if forloop.last %}last{% endif %} index:{{ forloop.index }} ",
            }, () =>
            {
                var vars = Hash.FromAnonymousObject(new
                {
                    products = new[]
                    {
                        new { title = "Draft 151cm" },
                        new { title = "Element 155cm" },
                    }
                });
                Helper.AssertTemplateResult("Product: Draft 151cm first  index:1 Product: Element 155cm  last index:2 ","{% render 'product' for products %}", vars);
            });
        }

        [Test]
        public void TestRenderTagForDrop()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["loop"] = "{{ value.foo }}",
            }, () =>
            {
                var vars = Hash.FromAnonymousObject(new
                {
                    loop = new TestEnumerable()
                });
                Helper.AssertTemplateResult("123","{% render 'loop' for loop as value %}", vars);
            });
        }

        [Test]
        public void TestRenderTagWithDrop()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
            {
                ["loop"] = "{{ value.Name }}",
            }, () =>
            {
                var vars = Hash.FromAnonymousObject(new
                {
                    loop = new TestEnumerable()
                });
                Helper.AssertTemplateResult("TestEnumerable","{% render 'loop' with loop as value %}", vars);
            });
        }
    }
}
