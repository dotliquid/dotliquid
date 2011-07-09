using DotLiquid.Util;
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
		public void TestErbLikeTrimming()
		{
			Template t = Template.Parse("{% if true -%}\nhi tobi\n{% endif %}");
			Assert.AreEqual("hi tobi\n", t.Render());
		}

		[Test]
		public void TestRenderToStreamWriter()
		{
			Template template = Template.Parse("{{test}}");

			using (MemoryStreamWriter streamWriter = new MemoryStreamWriter())
			{
				template.Render(streamWriter, new RenderParameters { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" })});

				Assert.AreEqual("worked", streamWriter.ToString());
			}
		}

		[Test]
		public void TestRenderToStream()
		{
			Template template = Template.Parse("{{test}}");

			using (MemoryStreamWriter streamWriter = new MemoryStreamWriter())
			{
				template.Render(streamWriter.BaseStream, new RenderParameters { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

				Assert.AreEqual("worked", streamWriter.ToString());
			}
		}
	}
}