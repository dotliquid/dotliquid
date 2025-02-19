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
            Assert.That(v22, Is.EqualTo(v20).AsCollection);
            return v22;
        }

        [Test]
        public void TestTokenizeStrings()
        {
            Assert.That(TokenizeValidateBackwardCompatibility(" "), Is.EqualTo(new[] { " " }).AsCollection);
            Assert.That(TokenizeValidateBackwardCompatibility("hello world"), Is.EqualTo(new[] { "hello world" }).AsCollection);
        }

        [Test]
        public void TestTokenizeVariables()
        {
            Assert.That(TokenizeValidateBackwardCompatibility("{{funk}}"), Is.EqualTo(new[] { "{{funk}}" }).AsCollection);
            Assert.That(TokenizeValidateBackwardCompatibility(" {{funk}} "), Is.EqualTo(new[] { " ", "{{funk}}", " " }).AsCollection);
            Assert.That(TokenizeValidateBackwardCompatibility(" {{funk}} {{so}} {{brother}} "), Is.EqualTo(new[] { " ", "{{funk}}", " ", "{{so}}", " ", "{{brother}}", " " }).AsCollection);
            Assert.That(TokenizeValidateBackwardCompatibility(" {{  funk  }} "), Is.EqualTo(new[] { " ", "{{  funk  }}", " " }).AsCollection);
        }

        [Test]
        public void TestTokenizeBlocks()
        {
            Assert.That(TokenizeValidateBackwardCompatibility("{%assign%}"), Is.EqualTo(new[] { "{%assign%}" }).AsCollection);
            Assert.That(TokenizeValidateBackwardCompatibility(" {%assign%} "), Is.EqualTo(new[] { " ", "{%assign%}", " " }).AsCollection);

            Assert.That(TokenizeValidateBackwardCompatibility(" {%comment%} {%endcomment%} "), Is.EqualTo(new[] { " ", "{%comment%}", " ", "{%endcomment%}", " " }).AsCollection);
            Assert.That(TokenizeValidateBackwardCompatibility("  {% comment %} {% endcomment %} "), Is.EqualTo(new[] { "  ", "{% comment %}", " ", "{% endcomment %}", " " }).AsCollection);
        }

        [Test]
        public void TestInstanceAssignsPersistOnSameTemplateObjectBetweenParses()
        {
            Template t = new Template();
            Assert.That(t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("from instance assigns"));
            Assert.That(t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("from instance assigns"));
        }

        [Test]
        public void TestThreadSafeInstanceAssignsNotPersistOnSameTemplateObjectBetweenParses()
        {
            Template t = new Template();
            t.MakeThreadSafe();
            Assert.That(t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("from instance assigns"));
            Assert.That(t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo(""));
        }

        [Test]
        public void TestInstanceAssignsPersistOnSameTemplateParsingBetweenRenders()
        {
            Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
            Assert.That(t.Render(), Is.EqualTo("foo"));
            Assert.That(t.Render(), Is.EqualTo("foofoo"));
        }

        [Test]
        public void TestThreadSafeInstanceAssignsNotPersistOnSameTemplateParsingBetweenRenders()
        {
            Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
            t.MakeThreadSafe();
            Assert.That(t.Render(), Is.EqualTo("foo"));
            Assert.That(t.Render(), Is.EqualTo("foo"));
        }

        [Test]
        public void TestCustomAssignsDoNotPersistOnSameTemplate()
        {
            Template t = new Template();
            Assert.That(t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(Hash.FromAnonymousObject(new { foo = "from custom assigns" })), Is.EqualTo("from custom assigns"));
            Assert.That(t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo(""));
        }

        [Test]
        public void TestCustomAssignsSquashInstanceAssigns()
        {
            Template t = new Template();
            Assert.That(t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("from instance assigns"));
            Assert.That(t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(Hash.FromAnonymousObject(new { foo = "from custom assigns" })), Is.EqualTo("from custom assigns"));
        }

        [Test]
        public void TestPersistentAssignsSquashInstanceAssigns()
        {
            Template t = new Template();
            Assert.That(t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("from instance assigns"));
            t.Assigns["foo"] = "from persistent assigns";
            Assert.That(t.ParseInternal("{{ foo }}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("from persistent assigns"));
        }

        [Test]
        public void TestLambdaIsCalledOnceFromPersistentAssignsOverMultipleParsesAndRenders()
        {
            Template t = new Template();
            int global = 0;
            t.Assigns["number"] = (Proc)(c => ++global);
            Assert.That(t.ParseInternal("{{number}}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("1"));
            Assert.That(t.ParseInternal("{{number}}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("1"));
            Assert.That(t.Render(), Is.EqualTo("1"));
        }

        [Test]
        public void TestLambdaIsCalledOnceFromCustomAssignsOverMultipleParsesAndRenders()
        {
            Template t = new Template();
            int global = 0;
            Hash assigns = Hash.FromAnonymousObject(new { number = (Proc)(c => ++global) });
            Assert.That(t.ParseInternal("{{number}}", SyntaxCompatibility.DotLiquid22).Render(assigns), Is.EqualTo("1"));
            Assert.That(t.ParseInternal("{{number}}", SyntaxCompatibility.DotLiquid22).Render(assigns), Is.EqualTo("1"));
            Assert.That(t.Render(assigns), Is.EqualTo("1"));
        }

        [Test]
        public void TestErbLikeTrimmingLeadingWhitespace()
        {
            string template = "foo\n\t  {%- if true %}hi tobi{% endif %}";
            Assert.That(Template.Parse(template, SyntaxCompatibility.DotLiquid20).Render(), Is.EqualTo("foo\nhi tobi"));
            Assert.That(Template.Parse(template, SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("foohi tobi"));
        }

        [Test]
        public void TestErbLikeTrimmingTrailingWhitespace()
        {
            string template = "{% if true -%}\n hi tobi\n{% endif %}";
            Assert.That(Template.Parse(template, SyntaxCompatibility.DotLiquid20).Render(), Is.EqualTo(" hi tobi\n"));
            Assert.That(Template.Parse(template, SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("hi tobi\n"));
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
            Assert.That(
                Template.Parse(template, SyntaxCompatibility.DotLiquid20).Render(Hash.FromAnonymousObject(new { tasks = new[] { "foo", "bar", "baz" } })), Is.EqualTo("<ul>\r\n    <li>foo</li>\r\n    <li>bar</li>\r\n    <li>baz</li>\r\n</ul>"));
            Assert.That(
                Template.Parse(template, SyntaxCompatibility.DotLiquid22).Render(Hash.FromAnonymousObject(new { tasks = new[] { "foo", "bar", "baz" } })), Is.EqualTo("<ul>\r\n<li>foo</li><li>bar</li><li>baz</li></ul>"));
        }

        [Test]
        public void TestRenderToStreamWriter()
        {
            Template template = Template.Parse("{{test}}");

            using (TextWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                template.Render(writer, new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

                Assert.That(writer.ToString(), Is.EqualTo("worked"));
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
                Assert.That(reader.ReadToEnd(), Is.EqualTo("worked"));
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

            Assert.That(output, Is.EqualTo("worked"));
        }

        [Test]
        public void TestRegisterSimpleTypeToString()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "ToString" });
            Template template = Template.Parse("{{context}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Doesn't automatically call ToString().
            Assert.That(output, Is.EqualTo(string.Empty));
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
            Assert.That(output, Is.EqualTo("Foo"));
        }

        [Test]
        public void TestRegisterSimpleTypeTransformer()
        {
            Template.RegisterSafeType(typeof(MySimpleType), o => o.ToString());
            Template template = Template.Parse("{{context}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Uses safe type transformer.
            Assert.That(output, Is.EqualTo("Foo"));
        }

        [Test]
        public void TestRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" }, m => m.ToString());

            Template template = Template.Parse("{{context}}{{context.Name}}"); //

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "Bar" } }));

            // Uses safe type transformer.
            Assert.That(output, Is.EqualTo("FooBar"));
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
            Assert.That(output, Is.EqualTo("FooBar FooBar2"));
        }

        [Test]
        public void TestOverrideDefaultBoolRenderingWithValueTypeTransformer()
        {
            Template.RegisterValueTypeTransformer(typeof(bool), m => (bool)m ? "Win" : "Fail");

            Template template = Template.Parse("{{var1}} {{var2}}");

            var output = template.Render(Hash.FromAnonymousObject(new { var1 = true, var2 = false }));

            Assert.That(output, Is.EqualTo("Win Fail"));
        }

        [Test]
        public void TestHtmlEncodingFilter()
        {
            Template.RegisterValueTypeTransformer(typeof(string), m => WebUtility.HtmlEncode((string)m));

            Template template = Template.Parse("{{var1}} {{var2}}");

            var output = template.Render(Hash.FromAnonymousObject(new { var1 = "<html>", var2 = "Some <b>bold</b> text." }));

            Assert.That(output, Is.EqualTo("&lt;html&gt; Some &lt;b&gt;bold&lt;/b&gt; text."));
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

            Assert.That(output, Is.EqualTo("worked"));
        }

        [Test]
        public void TestRegisterInterfaceTransformIntoAnonymousType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(IMySimpleInterface2), x => new { Name = ((IMySimpleInterface2)x).Name });
            Template template = Template.Parse("{{context.Name}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.That(output, Is.EqualTo("worked"));
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

            Assert.That(output, Is.EqualTo(""));
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

            Assert.That(output, Is.EqualTo("worked"));
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
                Assert.That(Template.DefaultSyntaxCompatibilityLevel, Is.EqualTo(SyntaxCompatibility.DotLiquid20));

                // RenderParameters Applies Template Defaults 
                Template.DefaultSyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid21;
                var renderParamsDefault = new RenderParameters(CultureInfo.CurrentCulture);
                Assert.That(renderParamsDefault.SyntaxCompatibilityLevel, Is.EqualTo(Template.DefaultSyntaxCompatibilityLevel));

                // Context Applies Template Defaults
                var context = new Context(CultureInfo.CurrentCulture);
                Assert.That(context.SyntaxCompatibilityLevel, Is.EqualTo(Template.DefaultSyntaxCompatibilityLevel));

                Template.DefaultSyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid20;
                renderParamsDefault.Evaluate(template, out Context defaultContext, out Hash defaultRegisters, out System.Collections.Generic.IEnumerable<System.Type> defaultFilters);
                // Context applies RenderParameters
                Assert.That(defaultContext.SyntaxCompatibilityLevel, Is.EqualTo(renderParamsDefault.SyntaxCompatibilityLevel));
                // RenderParameters not affected by later changes to Template defaults
                Assert.That(renderParamsDefault.SyntaxCompatibilityLevel, Is.Not.EqualTo(Template.DefaultSyntaxCompatibilityLevel));
                // But newly constructed RenderParameters is
                Assert.That(new RenderParameters(CultureInfo.CurrentCulture).SyntaxCompatibilityLevel, Is.EqualTo(Template.DefaultSyntaxCompatibilityLevel));

                // RenderParameters overrides template defaults when specified
                var renderParamsExplicit = new RenderParameters(CultureInfo.CurrentCulture) { SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid21 };
                Assert.That(renderParamsExplicit.SyntaxCompatibilityLevel, Is.EqualTo(SyntaxCompatibility.DotLiquid21));
                renderParamsExplicit.Evaluate(template, out Context explicitContext, out Hash explicitRegisters, out System.Collections.Generic.IEnumerable<System.Type> explicitFilters);
                Assert.That(explicitContext.SyntaxCompatibilityLevel, Is.EqualTo(renderParamsExplicit.SyntaxCompatibilityLevel));
            });
        }

        [Test]
        public void TestFilterSafelist()
        {
            Assert.That(Template.TryGetSafelistedFilter("test_alias", out var testAliasType), Is.False);
            Assert.That(testAliasType, Is.Null);

            // Safelist using default alias
            Template.SafelistFilter(typeof(ShopifyFilters));
            Assert.That(Template.TryGetSafelistedFilter("ShopifyFilters", out var shopifyFiltersType), Is.True);
            Assert.That(shopifyFiltersType, Is.EqualTo(typeof(ShopifyFilters)));

            // Safelist using explicit alias
            Template.SafelistFilter(typeof(ShopifyFilters), "test_alias");
            Assert.That(Template.TryGetSafelistedFilter("test_alias", out testAliasType), Is.True);
            Assert.That(testAliasType, Is.EqualTo(typeof(ShopifyFilters)));

            Assert.That(Template.GetSafelistedFilterAliases(), Has.Member("ShopifyFilters"));
            Assert.That(Template.GetSafelistedFilterAliases(), Has.Member("test_alias"));
        }
    }
}
