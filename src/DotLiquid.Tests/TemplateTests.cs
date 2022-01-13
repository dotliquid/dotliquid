using System.Globalization;
using System.IO;
using System.Net;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class TemplateTests
    {
        private System.Collections.Generic.List<string> TokenizeValidateBackwardCompatibility(string input)
        {
            var v20 = Tokenizer.Tokenize(input, SyntaxCompatibility.DotLiquid20);
            var v22 = Tokenizer.Tokenize(input, SyntaxCompatibility.DotLiquid22);
            CollectionAssert.AreEqual(v20, v22);
            return v22;
        }

        [Test]
        public void TestTokenizeStrings()
        {
            CollectionAssert.AreEqual(new[] { " " }, TokenizeValidateBackwardCompatibility(" "));
            CollectionAssert.AreEqual(new[] { "hello world" }, TokenizeValidateBackwardCompatibility("hello world"));
        }

        [Test]
        public void TestTokenizeVariables()
        {
            CollectionAssert.AreEqual(new[] { "{{funk}}" }, TokenizeValidateBackwardCompatibility("{{funk}}"));
            CollectionAssert.AreEqual(new[] { " ", "{{funk}}", " " }, TokenizeValidateBackwardCompatibility(" {{funk}} "));
            CollectionAssert.AreEqual(new[] { " ", "{{funk}}", " ", "{{so}}", " ", "{{brother}}", " " }, TokenizeValidateBackwardCompatibility(" {{funk}} {{so}} {{brother}} "));
            CollectionAssert.AreEqual(new[] { " ", "{{  funk  }}", " " }, TokenizeValidateBackwardCompatibility(" {{  funk  }} "));
        }

        [Test]
        public void TestTokenizeBlocks()
        {
            CollectionAssert.AreEqual(new[] { "{%assign%}" }, TokenizeValidateBackwardCompatibility("{%assign%}"));
            CollectionAssert.AreEqual(new[] { " ", "{%assign%}", " " }, TokenizeValidateBackwardCompatibility(" {%assign%} "));

            CollectionAssert.AreEqual(new[] { " ", "{%comment%}", " ", "{%endcomment%}", " " }, TokenizeValidateBackwardCompatibility(" {%comment%} {%endcomment%} "));
            CollectionAssert.AreEqual(new[] { "  ", "{% comment %}", " ", "{% endcomment %}", " " }, TokenizeValidateBackwardCompatibility("  {% comment %} {% endcomment %} "));
        }

        [Test]
        public void TestInstanceAssignsPersistOnSameTemplateObjectBetweenParses()
        {
            Template t = new Template();
            Assert.AreEqual("from instance assigns", t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}", SyntaxCompatibility.DotLiquid22).Render());
            Assert.AreEqual("from instance assigns", t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render());
        }

        [Test]
        public void TestThreadSafeInstanceAssignsNotPersistOnSameTemplateObjectBetweenParses()
        {
            Template t = new Template();
            t.MakeThreadSafe();
            Assert.AreEqual("from instance assigns", t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}", SyntaxCompatibility.DotLiquid22).Render());
            Assert.AreEqual("", t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render());
        }

        [Test]
        public void TestInstanceAssignsPersistOnSameTemplateParsingBetweenRenders()
        {
            Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
            Assert.AreEqual("foo", t.Render());
            Assert.AreEqual("foofoo", t.Render());
        }

        [Test]
        public void TestThreadSafeInstanceAssignsNotPersistOnSameTemplateParsingBetweenRenders()
        {
            Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
            t.MakeThreadSafe();
            Assert.AreEqual("foo", t.Render());
            Assert.AreEqual("foo", t.Render());
        }

        [Test]
        public void TestCustomAssignsDoNotPersistOnSameTemplate()
        {
            Template t = new Template();
            Assert.AreEqual("from custom assigns", t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(Hash.FromAnonymousObject(new { foo = "from custom assigns" })));
            Assert.AreEqual("", t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render());
        }

        [Test]
        public void TestCustomAssignsSquashInstanceAssigns()
        {
            Template t = new Template();
            Assert.AreEqual("from instance assigns", t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}", SyntaxCompatibility.DotLiquid22).Render());
            Assert.AreEqual("from custom assigns", t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(Hash.FromAnonymousObject(new { foo = "from custom assigns" })));
        }

        [Test]
        public void TestPersistentAssignsSquashInstanceAssigns()
        {
            Template t = new Template();
            Assert.AreEqual("from instance assigns",
                t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}", SyntaxCompatibility.DotLiquid22).Render());
            t.Assigns["foo"] = "from persistent assigns";
            Assert.AreEqual("from persistent assigns", t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render());
        }

        [Test]
        public void TestLambdaIsCalledOnceFromPersistentAssignsOverMultipleParsesAndRenders()
        {
            Template t = new Template();
            int global = 0;
            t.Assigns["number"] = (Proc)(c => ++global);
            Assert.AreEqual("1", t.ParseInternal("{{number}}", SyntaxCompatibility.DotLiquid22).Render());
            Assert.AreEqual("1", t.ParseInternal("{{number}}", SyntaxCompatibility.DotLiquid22).Render());
            Assert.AreEqual("1", t.Render());
        }

        [Test]
        public void TestLambdaIsCalledOnceFromCustomAssignsOverMultipleParsesAndRenders()
        {
            Template t = new Template();
            int global = 0;
            Hash assigns = Hash.FromAnonymousObject(new { number = (Proc)(c => ++global) });
            Assert.AreEqual("1", t.ParseInternal("{{number}}", SyntaxCompatibility.DotLiquid22).Render(assigns));
            Assert.AreEqual("1", t.ParseInternal("{{number}}", SyntaxCompatibility.DotLiquid22).Render(assigns));
            Assert.AreEqual("1", t.Render(assigns));
        }

        [Test]
        public void TestErbLikeTrimmingLeadingWhitespace()
        {
            string template = "foo\n\t  {%- if true %}hi tobi{% endif %}";
            Assert.AreEqual("foo\nhi tobi", Template.Parse(template, SyntaxCompatibility.DotLiquid20).Render());
            Assert.AreEqual("foohi tobi", Template.Parse(template, SyntaxCompatibility.DotLiquid22).Render());
        }

        [Test]
        public void TestErbLikeTrimmingTrailingWhitespace()
        {
            string template = "{% if true -%}\n hi tobi\n{% endif %}";
            Assert.AreEqual(" hi tobi\n", Template.Parse(template, SyntaxCompatibility.DotLiquid20).Render());
            Assert.AreEqual("hi tobi\n", Template.Parse(template, SyntaxCompatibility.DotLiquid22).Render());
        }

        [Test]
        public void TestErbLikeTrimmingLeadingAndTrailingWhitespace()
        {
            string template = @"<ul>
{% for item in tasks -%}
    {%- if true -%}
    <li>{{ item }}</li>
    {%- endif -%}
{% endfor -%}
</ul>";
            Assert.AreEqual(
                "<ul>\r\n    <li>foo</li>\r\n    <li>bar</li>\r\n    <li>baz</li>\r\n</ul>",
                Template.Parse(template, SyntaxCompatibility.DotLiquid20).Render(Hash.FromAnonymousObject(new { tasks = new[] { "foo", "bar", "baz" } })));
            Assert.AreEqual(
                "<ul>\r\n<li>foo</li><li>bar</li><li>baz</li></ul>",
                Template.Parse(template, SyntaxCompatibility.DotLiquid22).Render(Hash.FromAnonymousObject(new { tasks = new[] { "foo", "bar", "baz" } })));
        }

        [Test]
        public void TestRenderToStreamWriter()
        {
            Template template = Template.Parse("{{test}}");

            using (TextWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                template.Render(writer, new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

                Assert.AreEqual("worked", writer.ToString());
            }
        }

        [Test]
        public void TestRenderToStream()
        {
            Template template = Template.Parse("{{test}}");

            var output = new MemoryStream();
            template.Render(output, new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

            output.Seek(0, SeekOrigin.Begin);

            using (TextReader reader = new StreamReader(output))
            {
                Assert.AreEqual("worked", reader.ReadToEnd());
            }
        }

        public class MySimpleType
        {
            public string Name { get; set; }

            public override string ToString()
            {
                return "Foo";
            }
        }

        [Test]
        public void TestRegisterSimpleType()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" });
            Template template = Template.Parse("{{context.Name}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "worked" } }));

            Assert.AreEqual("worked", output);
        }

        [Test]
        public void TestRegisterSimpleTypeToString()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "ToString" });
            Template template = Template.Parse("{{context}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Doesn't automatically call ToString().
            Assert.AreEqual(string.Empty, output);
        }

        [Test]
        public void TestRegisterSimpleTypeToStringWhenTransformReturnsComplexType()
        {
            Template.RegisterSafeType(typeof(MySimpleType), o =>
                {
                    return o;
                });

            Template template = Template.Parse("{{context}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Does automatically call ToString because Variable.Render calls ToString on objects during rendering.
            Assert.AreEqual("Foo", output);
        }

        [Test]
        public void TestRegisterSimpleTypeTransformer()
        {
            Template.RegisterSafeType(typeof(MySimpleType), o => o.ToString());
            Template template = Template.Parse("{{context}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Uses safe type transformer.
            Assert.AreEqual("Foo", output);
        }

        [Test]
        public void TestRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" }, m => m.ToString());

            Template template = Template.Parse("{{context}}{{context.Name}}"); //

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "Bar" } }));

            // Uses safe type transformer.
            Assert.AreEqual("FooBar", output);
        }

        public class NestedMySimpleType
        {
            public string Name { get; set; }

            public NestedMySimpleType Nested { get; set; }

            public override string ToString()
            {
                return "Foo";
            }
        }

        [Test]
        public void TestNestedRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template.RegisterSafeType(typeof(NestedMySimpleType), new[] { "Name", "Nested" }, m => m.ToString());

            Template template = Template.Parse("{{context}}{{context.Name}} {{context.Nested}}{{context.Nested.Name}}"); //

            var inner = new NestedMySimpleType() { Name = "Bar2" };

            var output = template.Render(Hash.FromAnonymousObject(new { context = new NestedMySimpleType() { Nested = inner, Name = "Bar" } }));

            // Uses safe type transformer.
            Assert.AreEqual("FooBar FooBar2", output);
        }

        [Test]
        public void TestOverrideDefaultBoolRenderingWithValueTypeTransformer()
        {
            Template.RegisterValueTypeTransformer(typeof(bool), m => (bool)m ? "Win" : "Fail");

            Template template = Template.Parse("{{var1}} {{var2}}");

            var output = template.Render(Hash.FromAnonymousObject(new { var1 = true, var2 = false }));

            Assert.AreEqual("Win Fail", output);
        }

        [Test]
        public void TestHtmlEncodingFilter()
        {
            Template.RegisterValueTypeTransformer(typeof(string), m => WebUtility.HtmlEncode((string)m));

            Template template = Template.Parse("{{var1}} {{var2}}");

            var output = template.Render(Hash.FromAnonymousObject(new { var1 = "<html>", var2 = "Some <b>bold</b> text." }));

            Assert.AreEqual("&lt;html&gt; Some &lt;b&gt;bold&lt;/b&gt; text.", output);
        }

        public interface IMySimpleInterface2
        {
            string Name { get; }
        }

        public class MySimpleType2 : IMySimpleInterface2
        {
            public string Name { get; set; }
        }

        [Test]
        public void TestRegisterSimpleTypeTransformIntoAnonymousType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(MySimpleType2), x => new { Name = ((MySimpleType2)x).Name });
            Template template = Template.Parse("{{context.Name}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.AreEqual("worked", output);
        }

        [Test]
        public void TestRegisterInterfaceTransformIntoAnonymousType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(IMySimpleInterface2), x => new { Name = ((IMySimpleInterface2)x).Name });
            Template template = Template.Parse("{{context.Name}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.AreEqual("worked", output);
        }

        public class MyUnsafeType2
        {
            public string Name { get; set; }
        }

        [Test]
        public void TestRegisterSimpleTypeTransformIntoUnsafeType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(MySimpleType2), x => new MyUnsafeType2 { Name = ((MySimpleType2)x).Name });
            Template template = Template.Parse("{{context.Name}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.AreEqual("", output);
        }

        public interface MyGenericInterface<T>
        {
            T Value { get; set; }
        }

        public class MyGenericImpl<T> : MyGenericInterface<T>
        {
            public T Value { get; set; }
        }

        [Test]
        public void TestRegisterGenericInterface()
        {
            Template.RegisterSafeType(typeof(MyGenericInterface<>), new[] { "Value" });
            Template template = Template.Parse("{{context.Value}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyGenericImpl<string> { Value = "worked" } }));

            Assert.AreEqual("worked", output);
        }

        [Test]
        public void TestFirstAndLastOfObjectArray()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" });

            var array = new
            {
                People = new[] {
                    new MySimpleType { Name = "Jane" },
                    new MySimpleType { Name = "Mike" },
                }
            };

            Helper.AssertTemplateResult(
                expected: "Jane",
                template: "{{ People.first.Name }}",
                localVariables: Hash.FromAnonymousObject(array));

            Helper.AssertTemplateResult(
                expected: "Mike",
                template: "{{ People.last.Name }}",
                localVariables: Hash.FromAnonymousObject(array));
        }

        [Test]
        public void TestSyntaxCompatibilityLevel()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var template = Template.Parse("{{ foo }}");
                template.MakeThreadSafe();

                // Template defaults to legacy DotLiquid 2.0 Handling
                Assert.AreEqual(SyntaxCompatibility.DotLiquid20, Template.DefaultSyntaxCompatibilityLevel);

                // RenderParameters Applies Template Defaults 
                Template.DefaultSyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid21;
                var renderParamsDefault = new RenderParameters(CultureInfo.CurrentCulture);
                Assert.AreEqual(Template.DefaultSyntaxCompatibilityLevel, renderParamsDefault.SyntaxCompatibilityLevel);

                // Context Applies Template Defaults
                var context = new Context(CultureInfo.CurrentCulture);
                Assert.AreEqual(Template.DefaultSyntaxCompatibilityLevel, context.SyntaxCompatibilityLevel);

                Template.DefaultSyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid20;
                renderParamsDefault.Evaluate(template, out Context defaultContext, out Hash defaultRegisters, out System.Collections.Generic.IEnumerable<System.Type> defaultFilters);
                // Context applies RenderParameters
                Assert.AreEqual(renderParamsDefault.SyntaxCompatibilityLevel, defaultContext.SyntaxCompatibilityLevel);
                // RenderParameters not affected by later changes to Template defaults
                Assert.AreNotEqual(Template.DefaultSyntaxCompatibilityLevel, renderParamsDefault.SyntaxCompatibilityLevel);
                // But newly constructed RenderParameters is
                Assert.AreEqual(Template.DefaultSyntaxCompatibilityLevel, new RenderParameters(CultureInfo.CurrentCulture).SyntaxCompatibilityLevel);

                // RenderParameters overrides template defaults when specified
                var renderParamsExplicit = new RenderParameters(CultureInfo.CurrentCulture) { SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid21 };
                Assert.AreEqual(SyntaxCompatibility.DotLiquid21, renderParamsExplicit.SyntaxCompatibilityLevel);
                renderParamsExplicit.Evaluate(template, out Context explicitContext, out Hash explicitRegisters, out System.Collections.Generic.IEnumerable<System.Type> explicitFilters);
                Assert.AreEqual(renderParamsExplicit.SyntaxCompatibilityLevel, explicitContext.SyntaxCompatibilityLevel);
            });
        }

        [Test]
        public void TestFilterSafelist()
        {
            Assert.IsFalse(Template.TryGetSafelistedFilter("test_alias", out var testAliasType));
            Assert.IsNull(testAliasType);

            // Safelist using default alias
            Template.SafelistFilter(typeof(ShopifyFilters));
            Assert.IsTrue(Template.TryGetSafelistedFilter("ShopifyFilters", out var shopifyFiltersType));
            Assert.AreEqual(typeof(ShopifyFilters), shopifyFiltersType);

            // Safelist using explicit alias
            Template.SafelistFilter(typeof(ShopifyFilters), "test_alias");
            Assert.IsTrue(Template.TryGetSafelistedFilter("test_alias", out testAliasType));
            Assert.AreEqual(typeof(ShopifyFilters), testAliasType);

            CollectionAssert.Contains(Template.GetSafelistedFilterAliases(), "ShopifyFilters");
            CollectionAssert.Contains(Template.GetSafelistedFilterAliases(), "test_alias");
        }
    }
}
