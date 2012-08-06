using System.IO;
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
		public void TestInstanceAssignsPersistOnSameTemplateObjectBetweenParses()
		{
			Template t = new Template();
			Assert.AreEqual("from instance assigns", t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").Render());
			Assert.AreEqual("from instance assigns", t.ParseInternal("{{ foo }}").Render());
		}

		[Test]
		public void TestInstanceAssignsPersistOnSameTemplateParsingBetweenRenders()
		{
			Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
			Assert.AreEqual("foo", t.Render());
			Assert.AreEqual("foofoo", t.Render());
		}

		[Test]
		public void TestCustomAssignsDoNotPersistOnSameTemplate()
		{
			Template t = new Template();
			Assert.AreEqual("from custom assigns", t.ParseInternal("{{ foo }}").Render(Hash.FromAnonymousObject(new { foo = "from custom assigns" })));
			Assert.AreEqual("", t.ParseInternal("{{ foo }}").Render());
		}

		[Test]
		public void TestCustomAssignsSquashInstanceAssigns()
		{
			Template t = new Template();
			Assert.AreEqual("from instance assigns", t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").Render());
			Assert.AreEqual("from custom assigns", t.ParseInternal("{{ foo }}").Render(Hash.FromAnonymousObject(new { foo = "from custom assigns" })));
		}

		[Test]
		public void TestPersistentAssignsSquashInstanceAssigns()
		{
			Template t = new Template();
			Assert.AreEqual("from instance assigns",
				t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").Render());
			t.Assigns["foo"] = "from persistent assigns";
			Assert.AreEqual("from persistent assigns", t.ParseInternal("{{ foo }}").Render());
		}

		[Test]
		public void TestLambdaIsCalledOnceFromPersistentAssignsOverMultipleParsesAndRenders()
		{
			Template t = new Template();
			int global = 0;
			t.Assigns["number"] = (Proc) (c => ++global);
			Assert.AreEqual("1", t.ParseInternal("{{number}}").Render());
			Assert.AreEqual("1", t.ParseInternal("{{number}}").Render());
			Assert.AreEqual("1", t.Render());
		}

		[Test]
		public void TestLambdaIsCalledOnceFromCustomAssignsOverMultipleParsesAndRenders()
		{
			Template t = new Template();
			int global = 0;
			Hash assigns = Hash.FromAnonymousObject(new { number = (Proc) (c => ++global) });
			Assert.AreEqual("1", t.ParseInternal("{{number}}").Render(assigns));
			Assert.AreEqual("1", t.ParseInternal("{{number}}").Render(assigns));
			Assert.AreEqual("1", t.Render(assigns));
		}

		[Test]
		public void TestErbLikeTrimmingLeadingWhitespace()
		{
			Template t = Template.Parse("foo\n\t  {%- if true %}hi tobi{% endif %}");
			Assert.AreEqual("foo\nhi tobi", t.Render());
		}

		[Test]
		public void TestErbLikeTrimmingTrailingWhitespace()
		{
			Template t = Template.Parse("{% if true -%}\nhi tobi\n{% endif %}");
			Assert.AreEqual("hi tobi\n", t.Render());
		}

		[Test]
		public void TestErbLikeTrimmingLeadingAndTrailingWhitespace()
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
</ul>", t.Render(Hash.FromAnonymousObject(new { tasks = new [] { "foo", "bar", "baz" } })));
		}

		[Test]
		public void TestRenderToStreamWriter()
		{
			Template template = Template.Parse("{{test}}");

			using (TextWriter writer = new StringWriter())
			{
				template.Render(writer, new RenderParameters { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

				Assert.AreEqual("worked", writer.ToString());
			}
		}

		[Test]
		public void TestRenderToStream()
		{
			Template template = Template.Parse("{{test}}");

			var output = new MemoryStream();
			template.Render(output, new RenderParameters { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

			output.Seek(0, SeekOrigin.Begin);

			using (TextReader reader = new StreamReader(output))
			{
				Assert.AreEqual("worked", reader.ReadToEnd());
			}
		}

		public class MySimpleType
		{
			public string Name { get; set; }
		}

        [Test]
		public void TestRegisterSimpleType()
		{
			Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" });
			Template template = Template.Parse("{{context.Name}}");

			var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "worked" } }));

			Assert.AreEqual("worked", output);
		}

        public class MySimpleType2
        {
            public string Name { get; set; }
        }

        [Test]
        public void TestRegisterSimpleTypeTransformIntoAnonymousType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(MySimpleType2), x => new { Name = ((MySimpleType2)x).Name } );
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
	}
}