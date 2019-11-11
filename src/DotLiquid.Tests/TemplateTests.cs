using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class TemplateTests
    {
        [Test]
        public void TestTokenizeStrings()
        {
            CollectionAssert.AreEqual(new[] { " " }, Template.Tokenize(" "));
            CollectionAssert.AreEqual(new[] { "hello world" }, Template.Tokenize("hello world"));
        }

        [Test]
        public void TestTokenizeVariables()
        {
            CollectionAssert.AreEqual(new[] { "{{funk}}" }, Template.Tokenize("{{funk}}"));
            CollectionAssert.AreEqual(new[] { " ", "{{funk}}", " " }, Template.Tokenize(" {{funk}} "));
            CollectionAssert.AreEqual(new[] { " ", "{{funk}}", " ", "{{so}}", " ", "{{brother}}", " " }, Template.Tokenize(" {{funk}} {{so}} {{brother}} "));
            CollectionAssert.AreEqual(new[] { " ", "{{  funk  }}", " " }, Template.Tokenize(" {{  funk  }} "));
        }

        [Test]
        public void TestTokenizeBlocks()
        {
            CollectionAssert.AreEqual(new[] { "{%comment%}" }, Template.Tokenize("{%comment%}"));
            CollectionAssert.AreEqual(new[] { " ", "{%comment%}", " " }, Template.Tokenize(" {%comment%} "));

            CollectionAssert.AreEqual(new[] { " ", "{%comment%}", " ", "{%endcomment%}", " " }, Template.Tokenize(" {%comment%} {%endcomment%} "));
            CollectionAssert.AreEqual(new[] { "  ", "{% comment %}", " ", "{% endcomment %}", " " }, Template.Tokenize("  {% comment %} {% endcomment %} "));
        }

        [Test]
        public async Task TestInstanceAssignsPersistOnSameTemplateObjectBetweenParses()
        {
            Template t = new Template();
            Assert.AreEqual("from instance assigns",await  t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").RenderAsync());
            Assert.AreEqual("from instance assigns", await t.ParseInternal("{{ foo }}").RenderAsync());
        }

        [Test]
        public async Task TestThreadSafeInstanceAssignsNotPersistOnSameTemplateObjectBetweenParses()
        {
            Template t = new Template();
            t.MakeThreadSafe();
            Assert.AreEqual("from instance assigns", await t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").RenderAsync());
            Assert.AreEqual("", await t.ParseInternal("{{ foo }}").RenderAsync());
        }

        [Test]
        public async Task TestInstanceAssignsPersistOnSameTemplateParsingBetweenRenders()
        {
            Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
            Assert.AreEqual("foo", await t.RenderAsync());
            Assert.AreEqual("foofoo", await t.RenderAsync());
        }

        [Test]
        public async Task TestThreadSafeInstanceAssignsNotPersistOnSameTemplateParsingBetweenRenders()
        {
            Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
            t.MakeThreadSafe();
            Assert.AreEqual("foo", await t.RenderAsync());
            Assert.AreEqual("foo", await t.RenderAsync());
        }

        [Test]
        public async Task TestCustomAssignsDoNotPersistOnSameTemplate()
        {
            Template t = new Template();
            Assert.AreEqual("from custom assigns", await t.ParseInternal("{{ foo }}").RenderAsync(Hash.FromAnonymousObject(new { foo = "from custom assigns" })));
            Assert.AreEqual("", await t.ParseInternal("{{ foo }}").RenderAsync());
        }

        [Test]
        public async Task TestCustomAssignsSquashInstanceAssigns()
        {
            Template t = new Template();
            Assert.AreEqual("from instance assigns", await t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").RenderAsync());
            Assert.AreEqual("from custom assigns", await t.ParseInternal("{{ foo }}").RenderAsync(Hash.FromAnonymousObject(new { foo = "from custom assigns" })));
        }

        [Test]
        public async Task TestPersistentAssignsSquashInstanceAssigns()
        {
            Template t = new Template();
            Assert.AreEqual("from instance assigns",
                await t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").RenderAsync());
            t.Assigns["foo"] = "from persistent assigns";
            Assert.AreEqual("from persistent assigns", await t.ParseInternal("{{ foo }}").RenderAsync());
        }

        [Test]
        public async Task TestLambdaIsCalledOnceFromPersistentAssignsOverMultipleParsesAndRenders()
        {
            Template t = new Template();
            int global = 0;
            t.Assigns["number"] = (Proc)(c => ++global);
            Assert.AreEqual("1", await t.ParseInternal("{{number}}").RenderAsync());
            Assert.AreEqual("1", await t.ParseInternal("{{number}}").RenderAsync());
            Assert.AreEqual("1", await t.RenderAsync());
        }

        [Test]
        public async Task TestLambdaIsCalledOnceFromCustomAssignsOverMultipleParsesAndRenders()
        {
            Template t = new Template();
            int global = 0;
            Hash assigns = Hash.FromAnonymousObject(new { number = (Proc)(c => ++global) });
            Assert.AreEqual("1", await t.ParseInternal("{{number}}").RenderAsync(assigns));
            Assert.AreEqual("1", await t.ParseInternal("{{number}}").RenderAsync(assigns));
            Assert.AreEqual("1", await t.RenderAsync(assigns));
        }

        [Test]
        public async Task TestErbLikeTrimmingLeadingWhitespace()
        {
            Template t = Template.Parse("foo\n\t  {%- if true %}hi tobi{% endif %}");
            Assert.AreEqual("foo\nhi tobi", await t.RenderAsync());
        }

        [Test]
        public async Task TestErbLikeTrimmingTrailingWhitespace()
        {
            Template t = Template.Parse("{% if true -%}\nhi tobi\n{% endif %}");
            Assert.AreEqual("hi tobi\n", await t.RenderAsync());
        }

        [Test]
        public async Task TestErbLikeTrimmingLeadingAndTrailingWhitespace()
        {
            Template t = Template.Parse(@"<ul>
{% for item in tasks -%}
    {%- if true -%}
    <li>{{ item }}</li>
    {%- endif -%}
{% endfor -%}
</ul>");
            Assert.AreEqual(@"<ul>
    <li>foo</li>
    <li>bar</li>
    <li>baz</li>
</ul>", await t.RenderAsync(Hash.FromAnonymousObject(new { tasks = new[] { "foo", "bar", "baz" } })));
        }

        [Test]
        public async Task TestRenderToStreamWriter()
        {
            Template template = Template.Parse("{{test}}");

            using (TextWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                await template.RenderAsync(writer, new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

                Assert.AreEqual("worked", writer.ToString());
            }
        }

        [Test]
        public async Task TestRenderToStream()
        {
            Template template = Template.Parse("{{test}}");

            var output = new MemoryStream();
            await template.RenderAsync(output, new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

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
        public async Task TestRegisterSimpleType()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" });
            Template template = Template.Parse("{{context.Name}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "worked" } }));

            Assert.AreEqual("worked", output);
        }

        [Test]
        public async Task TestRegisterSimpleTypeToString()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "ToString" });
            Template template = Template.Parse("{{context}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Doesn't automatically call ToString().
            Assert.AreEqual(string.Empty, output);
        }

        [Test]
        public async Task TestRegisterSimpleTypeToStringWhenTransformReturnsComplexType()
        {
            Template.RegisterSafeType(typeof(MySimpleType), o =>
                {
                    return o;
                });

            Template template = Template.Parse("{{context}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Does automatically call ToString because Variable.Render calls ToString on objects during rendering.
            Assert.AreEqual("Foo", output);
        }

        [Test]
        public async Task TestRegisterSimpleTypeTransformer()
        {
            Template.RegisterSafeType(typeof(MySimpleType), o => o.ToString());
            Template template = Template.Parse("{{context}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Uses safe type transformer.
            Assert.AreEqual("Foo", output);
        }

        [Test]
        public async Task TestRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" }, m => m.ToString());

            Template template = Template.Parse("{{context}}{{context.Name}}"); //

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "Bar" } }));

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
        public async Task TestNestedRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template.RegisterSafeType(typeof(NestedMySimpleType), new[] { "Name", "Nested" }, m => m.ToString());

            Template template = Template.Parse("{{context}}{{context.Name}} {{context.Nested}}{{context.Nested.Name}}"); //

            var inner = new NestedMySimpleType() { Name = "Bar2" };

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new NestedMySimpleType() { Nested = inner, Name = "Bar" } }));

            // Uses safe type transformer.
            Assert.AreEqual("FooBar FooBar2", output);
        }

        [Test]
        public async Task TestOverrideDefaultBoolRenderingWithValueTypeTransformer()
        {
            Template.RegisterValueTypeTransformer(typeof(bool), m => (bool)m ? "Win" : "Fail");

            Template template = Template.Parse("{{var1}} {{var2}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { var1 = true, var2 = false }));

            Assert.AreEqual("Win Fail", output);
        }

        [Test]
        public async Task TestHtmlEncodingFilter()
        {
            Template.RegisterValueTypeTransformer(typeof(string), m => WebUtility.HtmlEncode((string)m));

            Template template = Template.Parse("{{var1}} {{var2}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { var1 = "<html>", var2 = "Some <b>bold</b> text." }));

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
        public async Task TestRegisterSimpleTypeTransformIntoAnonymousType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(MySimpleType2), x => new { Name = ((MySimpleType2)x).Name });
            Template template = Template.Parse("{{context.Name}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.AreEqual("worked", output);
        }

        [Test]
        public async Task TestRegisterInterfaceTransformIntoAnonymousType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(IMySimpleInterface2), x => new { Name = ((IMySimpleInterface2)x).Name });
            Template template = Template.Parse("{{context.Name}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.AreEqual("worked", output);
        }

        public class MyUnsafeType2
        {
            public string Name { get; set; }
        }

        [Test]
        public async Task TestRegisterSimpleTypeTransformIntoUnsafeType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(MySimpleType2), x => new MyUnsafeType2 { Name = ((MySimpleType2)x).Name });
            Template template = Template.Parse("{{context.Name}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

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
        public async Task TestRegisterGenericInterface()
        {
            Template.RegisterSafeType(typeof(MyGenericInterface<>), new[] { "Value" });
            Template template = Template.Parse("{{context.Value}}");

            var output = await template.RenderAsync(Hash.FromAnonymousObject(new { context = new MyGenericImpl<string> { Value = "worked" } }));

            Assert.AreEqual("worked", output);
        }
    }
}
