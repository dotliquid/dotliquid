using System.Collections.Generic;
using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
	[TestFixture]
	public class StandardTagTests
	{
		[Test]
		public void TestTag()
		{
			Tag tag = new Tag();
			tag.Initialize("tag", null, null);
			Assert.AreEqual("tag", tag.Name);
			Assert.AreEqual(string.Empty, tag.Render(new Context()));
		}

		[Test]
		public void TestNoTransform()
		{
			Helper.AssertTemplateResult("this text should come out of the template without change...",
				"this text should come out of the template without change...");
			Helper.AssertTemplateResult("blah", "blah");
			Helper.AssertTemplateResult("<blah>", "<blah>");
			Helper.AssertTemplateResult("|,.:", "|,.:");
			Helper.AssertTemplateResult("", "");

			const string text = @"this shouldnt see any transformation either but has multiple lines
				as you can clearly see here ...";
			Helper.AssertTemplateResult(text, text);
		}

		[Test]
		public void TestHasABlockWhichDoesNothing()
		{
			Helper.AssertTemplateResult("the comment block should be removed  .. right?",
				"the comment block should be removed {%comment%} be gone.. {%endcomment%} .. right?");

			Helper.AssertTemplateResult("", "{%comment%}{%endcomment%}");
			Helper.AssertTemplateResult("", "{%comment%}{% endcomment %}");
			Helper.AssertTemplateResult("", "{% comment %}{%endcomment%}");
			Helper.AssertTemplateResult("", "{% comment %}{% endcomment %}");
			Helper.AssertTemplateResult("", "{%comment%}comment{%endcomment%}");
			Helper.AssertTemplateResult("", "{% comment %}comment{% endcomment %}");

			Helper.AssertTemplateResult("foobar", "foo{%comment%}comment{%endcomment%}bar");
			Helper.AssertTemplateResult("foobar", "foo{% comment %}comment{% endcomment %}bar");
			Helper.AssertTemplateResult("foobar", "foo{%comment%} comment {%endcomment%}bar");
			Helper.AssertTemplateResult("foobar", "foo{% comment %} comment {% endcomment %}bar");

			Helper.AssertTemplateResult("foo  bar", "foo {%comment%} {%endcomment%} bar");
			Helper.AssertTemplateResult("foo  bar", "foo {%comment%}comment{%endcomment%} bar");
			Helper.AssertTemplateResult("foo  bar", "foo {%comment%} comment {%endcomment%} bar");

			Helper.AssertTemplateResult("foobar", @"foo{%comment%}
				{%endcomment%}bar");
		}

		[Test]
		public void TestForWithDictionary()
		{
			var dictionary = new Dictionary<string, string>
			{
				{ "Graham Greene", "English" },
				{ "F. Scott Fitzgerald", "American" }
			};
			Helper.AssertTemplateResult(" English  American ", "{%for item in authors%} {{ item }} {%endfor%}",
				Hash.FromAnonymousObject(new { authors = dictionary.Values }));
		}

		public class TestDictObject : Drop {
			public TestDictObject() {
				testdict = new Dictionary<string, string>() { { "aa", "bb" }, { "dd", "ee" }, { "ff", "gg" } };
			}
			public Dictionary<string, string> testdict { get; set; }
		}

		[Test]
		public void TestDictionaryFor()
		{
			var template = Template.Parse("{%for item in bla.testdict %}{{ item.Key }}-{{ item[1]}} {%endfor%}");
			var result = template.Render(Hash.FromAnonymousObject(new { bla = new TestDictObject() }));
			Assert.AreEqual("aa-bb dd-ee ff-gg ",result);
		}

		[Test]
		public void TestFor()
		{
			Helper.AssertTemplateResult(" yo  yo  yo  yo ", "{%for item in array%} yo {%endfor%}",
				Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4 } }));
			Helper.AssertTemplateResult("yoyo", "{%for item in array%}yo{%endfor%}", Hash.FromAnonymousObject(new { array = new[] { 1, 2 } }));
			Helper.AssertTemplateResult(" yo ", "{%for item in array%} yo {%endfor%}", Hash.FromAnonymousObject(new { array = new[] { 1 } }));
			Helper.AssertTemplateResult("", "{%for item in array%}{%endfor%}", Hash.FromAnonymousObject(new { array = new[] { 1, 2 } }));
			const string expected = @"
  yo

  yo

  yo
";
			const string template = @"{%for item in array%}
  yo
{%endfor%}";
			Helper.AssertTemplateResult(expected, template, Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
		}

		[Test]
		public void TestForWithRange()
		{
			Helper.AssertTemplateResult(" 1  2  3 ", "{%for item in (1..3) %} {{item}} {%endfor%}");
		}

		[Test]
		public void TestForWithVariable()
		{
			Helper.AssertTemplateResult(" 1  2  3 ", "{%for item in array%} {{item}} {%endfor%}",
				Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
			Helper.AssertTemplateResult("123", "{%for item in array%}{{item}}{%endfor%}",
				Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
			Helper.AssertTemplateResult("123", "{% for item in array %}{{item}}{% endfor %}",
				Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
			Helper.AssertTemplateResult("abcd", "{%for item in array%}{{item}}{%endfor%}",
				Hash.FromAnonymousObject(new { array = new[] { "a", "b", "c", "d" } }));
			Helper.AssertTemplateResult("a b c", "{%for item in array%}{{item}}{%endfor%}",
				Hash.FromAnonymousObject(new { array = new[] { "a", " ", "b", " ", "c" } }));
			Helper.AssertTemplateResult("abc", "{%for item in array%}{{item}}{%endfor%}",
				Hash.FromAnonymousObject(new { array = new[] { "a", "", "b", "", "c" } }));
		}

		[Test]
		public void TestForHelpers()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } });
			Helper.AssertTemplateResult(" 1/3  2/3  3/3 ",
				"{%for item in array%} {{forloop.index}}/{{forloop.length}} {%endfor%}", assigns);
			Helper.AssertTemplateResult(" 1  2  3 ", "{%for item in array%} {{forloop.index}} {%endfor%}", assigns);
			Helper.AssertTemplateResult(" 0  1  2 ", "{%for item in array%} {{forloop.index0}} {%endfor%}", assigns);
			Helper.AssertTemplateResult(" 2  1  0 ", "{%for item in array%} {{forloop.rindex0}} {%endfor%}", assigns);
			Helper.AssertTemplateResult(" 3  2  1 ", "{%for item in array%} {{forloop.rindex}} {%endfor%}", assigns);
			Helper.AssertTemplateResult(" true  false  false ", "{%for item in array%} {{forloop.first}} {%endfor%}", assigns);
			Helper.AssertTemplateResult(" false  false  true ", "{%for item in array%} {{forloop.last}} {%endfor%}", assigns);
		}

		[Test]
		public void TestForAndIf()
		{
			Helper.AssertTemplateResult("+--", "{%for item in array%}{% if forloop.first %}+{% else %}-{% endif %}{%endfor%}",
				Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
		}

		[Test]
		public void TestLimiting()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } });
			Helper.AssertTemplateResult("12", "{%for i in array limit:2 %}{{ i }}{%endfor%}", assigns);
			Helper.AssertTemplateResult("1234", "{%for i in array limit:4 %}{{ i }}{%endfor%}", assigns);
			Helper.AssertTemplateResult("3456", "{%for i in array limit:4 offset:2 %}{{ i }}{%endfor%}", assigns);
			Helper.AssertTemplateResult("3456", "{%for i in array limit: 4 offset: 2 %}{{ i }}{%endfor%}", assigns);
		}

		[Test]
		public void TestDynamicVariableLimiting()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } });
			assigns["limit"] = 2;
			assigns["offset"] = 2;
			Helper.AssertTemplateResult("34", "{%for i in array limit: limit offset: offset %}{{ i }}{%endfor%}", assigns);
		}

		[Test]
		public void TestNestedFor()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = new[] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 } } });
			Helper.AssertTemplateResult("123456", "{%for item in array%}{%for i in item%}{{ i }}{%endfor%}{%endfor%}", assigns);
		}

		[Test]
		public void TestOffsetOnly()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } });
			Helper.AssertTemplateResult("890", "{%for i in array offset:7 %}{{ i }}{%endfor%}", assigns);
		}

		[Test]
		public void TestPauseResume()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = Hash.FromAnonymousObject(new { items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } }) });
			const string markup = @"{%for i in array.items limit: 3 %}{{i}}{%endfor%}
				next
				{%for i in array.items offset:continue limit: 3 %}{{i}}{%endfor%}
				next
				{%for i in array.items offset:continue limit: 3 %}{{i}}{%endfor%}";
			const string expected = @"123
				next
				456
				next
				789";
			Helper.AssertTemplateResult(expected, markup, assigns);
		}

		[Test]
		public void TestPauseResumeLimit()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = Hash.FromAnonymousObject(new { items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } }) });
			const string markup = @"{%for i in array.items limit:3 %}{{i}}{%endfor%}
				next
				{%for i in array.items offset:continue limit:3 %}{{i}}{%endfor%}
				next
				{%for i in array.items offset:continue limit:1 %}{{i}}{%endfor%}";
			const string expected = @"123
				next
				456
				next
				7";
			Helper.AssertTemplateResult(expected, markup, assigns);
		}

		[Test]
		public void TestPauseResumeBigLimit()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = Hash.FromAnonymousObject(new { items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } }) });
			const string markup = @"
				{%for i in array.items limit:3 %}{{i}}{%endfor%}
				next
				{%for i in array.items offset:continue limit:3 %}{{i}}{%endfor%}
				next
				{%for i in array.items offset:continue limit:1000 %}{{i}}{%endfor%}";
			const string expected = @"
				123
				next
				456
				next
				7890";
			Helper.AssertTemplateResult(expected, markup, assigns);
		}

		[Test]
		public void TestPauseResumeBigOffset()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = Hash.FromAnonymousObject(new { items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } }) });
			const string markup = @"
				{%for i in array.items limit:3 %}{{i}}{%endfor%}
				next
				{%for i in array.items offset:continue limit:3 %}{{i}}{%endfor%}
				next
				{%for i in array.items offset:continue limit:1000 offset:1000 %}{{i}}{%endfor%}";
			const string expected = @"
				123
				next
				456
				next
				";
			Helper.AssertTemplateResult(expected, markup, assigns);
		}

        [Test]
        public void TestForWithBreak()
        {
            var assigns = Hash.FromAnonymousObject(new { array = new { items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } } });
            var markup = "{% for i in array.items %}{{ i }}{% if i > 3 %}{% break %}{% endif %}{% endfor %}";
            var expected = "1234";
            Helper.AssertTemplateResult(expected, markup, assigns);
        }

        [Test]
        public void TestForWithContinue()
        {
            var assigns = Hash.FromAnonymousObject(new { array = new { items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } } });
            var markup = "{% for i in array.items %}{% if i == 3 %}{% continue %}{% endif %}{{ i }}{% endfor %}";
            var expected = "1245678910";
            Helper.AssertTemplateResult(expected, markup, assigns);
        }

        [Test]
        public void TestBreakOutsideFor()
        {
            var markup = "123{% break %}456";
            var expected = "123";
            Helper.AssertTemplateResult(expected, markup);
        }

        [Test]
        public void TestContinueOutsideFor()
        {
        var markup = "123{% continue %}456";
        var expected = "123";
        Helper.AssertTemplateResult(expected, markup);
        }

		[Test]
		public void TestAssign()
		{
			Hash assigns = Hash.FromAnonymousObject(new { var = "content" });
			Helper.AssertTemplateResult("var2:  var2:content", "var2:{{var2}} {%assign var2 = var%} var2:{{var2}}", assigns);
		}

		[Test]
		public void TestHyphenatedAssign()
		{
			Hash assigns = Hash.FromDictionary(new Dictionary<string, object> { { "a-b", "1" } });
			Helper.AssertTemplateResult("a-b:1 a-b:2", "a-b:{{a-b}} {%assign a-b = 2 %}a-b:{{a-b}}", assigns);
		}

		[Test]
		public void TestAssignWithColonAndSpaces()
		{
			Hash assigns = Hash.FromAnonymousObject(new { var = new Dictionary<string, object> { { "a:b c", new { paged = 1 } } } });
			Helper.AssertTemplateResult("var2: 1", "{%assign var2 = var['a:b c'].paged %}var2: {{var2}}", assigns);
		}

		[Test]
		public void TestCapture()
		{
			Hash assigns = Hash.FromAnonymousObject(new { var = "content" });
			Helper.AssertTemplateResult("content foo content foo ",
				"{{ var2 }}{% capture var2 %}{{ var }} foo {% endcapture %}{{ var2 }}{{ var2 }}", assigns);
		}

		[Test]
		public void TestCaptureDetectsBadSyntax()
		{
			Assert.Throws<SyntaxException>(() =>
				Helper.AssertTemplateResult("content foo content foo ", "{{ var2 }}{% capture %}{{ var }} foo {% endcapture %}{{ var2 }}{{ var2 }}", Hash.FromAnonymousObject(new { var = "content" })));
		}

		[Test]
		public void TestCase()
		{
			Hash assigns = Hash.FromAnonymousObject(new { condition = 2 });
			Helper.AssertTemplateResult(" its 2 ", "{% case condition %}{% when 1 %} its 1 {% when 2 %} its 2 {% endcase %}",
				assigns);

			assigns = Hash.FromAnonymousObject(new { condition = 1 });
			Helper.AssertTemplateResult(" its 1 ", "{% case condition %}{% when 1 %} its 1 {% when 2 %} its 2 {% endcase %}",
				assigns);

			assigns = Hash.FromAnonymousObject(new { condition = 3 });
			Helper.AssertTemplateResult("", "{% case condition %}{% when 1 %} its 1 {% when 2 %} its 2 {% endcase %}", assigns);

			assigns = Hash.FromAnonymousObject(new { condition = "string here" });
			Helper.AssertTemplateResult(" hit ", "{% case condition %}{% when 'string here' %} hit {% endcase %}", assigns);

			assigns = Hash.FromAnonymousObject(new { condition = "bad string here" });
			Helper.AssertTemplateResult("", "{% case condition %}{% when 'string here' %} hit {% endcase %}", assigns);
		}

		[Test]
		public void TestCaseWithElse()
		{
			Hash assigns = Hash.FromAnonymousObject(new { condition = 5 });
			Helper.AssertTemplateResult(" hit ", "{% case condition %}{% when 5 %} hit {% else %} else {% endcase %}", assigns);

			assigns = Hash.FromAnonymousObject(new { condition = 6 });
			Helper.AssertTemplateResult(" else ", "{% case condition %}{% when 5 %} hit {% else %} else {% endcase %}", assigns);

			assigns = Hash.FromAnonymousObject(new { condition = 6 });
			Helper.AssertTemplateResult(" else ", "{% case condition %} {% when 5 %} hit {% else %} else {% endcase %}", assigns);
		}

		[Test]
		public void TestCaseOnSize()
		{
			Helper.AssertTemplateResult("", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
				Hash.FromAnonymousObject(new { a = new object[] { } }));
			Helper.AssertTemplateResult("1", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1 } }));
			Helper.AssertTemplateResult("2", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1, 1 } }));
			Helper.AssertTemplateResult("", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1 } }));
			Helper.AssertTemplateResult("", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1, 1 } }));
			Helper.AssertTemplateResult("", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1, 1, 1 } }));
		}

		[Test]
		public void TestCaseOnSizeWithElse()
		{
			Helper.AssertTemplateResult("else", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
				Hash.FromAnonymousObject(new { a = new object[] { } }));
			Helper.AssertTemplateResult("1", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1 } }));
			Helper.AssertTemplateResult("2", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1, 1 } }));
			Helper.AssertTemplateResult("else", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1 } }));
			Helper.AssertTemplateResult("else", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1, 1 } }));
			Helper.AssertTemplateResult("else", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
				Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1, 1, 1 } }));
		}

		[Test]
		public void TestCaseOnLengthWithElse()
		{
			Helper.AssertTemplateResult("else",
				"{% case a.empty? %}{% when true %}true{% when false %}false{% else %}else{% endcase %}", new Hash());
			Helper.AssertTemplateResult("false",
				"{% case false %}{% when true %}true{% when false %}false{% else %}else{% endcase %}", new Hash());
			Helper.AssertTemplateResult("true",
				"{% case true %}{% when true %}true{% when false %}false{% else %}else{% endcase %}", new Hash());
			Helper.AssertTemplateResult("else",
				"{% case NULL %}{% when true %}true{% when false %}false{% else %}else{% endcase %}", new Hash());
		}

		[Test]
		public void TestAssignFromCase()
		{
			// Example from the shopify forums
			const string code = "{% case collection.handle %}{% when 'menswear-jackets' %}{% assign ptitle = 'menswear' %}{% when 'menswear-t-shirts' %}{% assign ptitle = 'menswear' %}{% else %}{% assign ptitle = 'womenswear' %}{% endcase %}{{ ptitle }}";
			Template template = Template.Parse(code);
			Assert.AreEqual("menswear", template.Render(Hash.FromAnonymousObject(new { collection = new { handle = "menswear-jackets" } })));
			Assert.AreEqual("menswear", template.Render(Hash.FromAnonymousObject(new { collection = new { handle = "menswear-t-shirts" } })));
			Assert.AreEqual("womenswear", template.Render(Hash.FromAnonymousObject(new { collection = new { handle = "x" } })));
			Assert.AreEqual("womenswear", template.Render(Hash.FromAnonymousObject(new { collection = new { handle = "y" } })));
			Assert.AreEqual("womenswear", template.Render(Hash.FromAnonymousObject(new { collection = new { handle = "z" } })));
		}

		[Test]
		public void TestCaseWhenOr()
		{
			const string code1 = "{% case condition %}{% when 1 or 2 or 3 %} its 1 or 2 or 3 {% when 4 %} its 4 {% endcase %}";
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 1 }));
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 2 }));
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 3 }));
			Helper.AssertTemplateResult(" its 4 ", code1, Hash.FromAnonymousObject(new { condition = 4 }));
			Helper.AssertTemplateResult("", code1, Hash.FromAnonymousObject(new { condition = 5 }));

			const string code2 =
				"{% case condition %}{% when 1 or 'string' or null %} its 1 or 2 or 3 {% when 4 %} its 4 {% endcase %}";
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = 1 }));
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = "string" }));
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = (object) null }));
			Helper.AssertTemplateResult("", code2, Hash.FromAnonymousObject(new { condition = "something else" }));
		}

		[Test]
		public void TestCaseWhenComma()
		{
			const string code1 = "{% case condition %}{% when 1, 2, 3 %} its 1 or 2 or 3 {% when 4 %} its 4 {% endcase %}";
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 1 }));
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 2 }));
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 3 }));
			Helper.AssertTemplateResult(" its 4 ", code1, Hash.FromAnonymousObject(new { condition = 4 }));
			Helper.AssertTemplateResult("", code1, Hash.FromAnonymousObject(new { condition = 5 }));

			const string code2 =
				"{% case condition %}{% when 1, 'string', null %} its 1 or 2 or 3 {% when 4 %} its 4 {% endcase %}";
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = 1 }));
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = "string" }));
			Helper.AssertTemplateResult(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = (object) null }));
			Helper.AssertTemplateResult("", code2, Hash.FromAnonymousObject(new { condition = "something else" }));
		}

		[Test]
		public void TestAssign2()
		{
			Assert.AreEqual("variable", Template.Parse("{% assign a = 'variable' %}{{a}}").Render());
		}

		[Test]
		public void TestAssignAnEmptyString()
		{
			Assert.AreEqual("", Template.Parse("{% assign a = '' %}{{a}}").Render());
		}

		[Test]
		public void TestAssignIsGlobal()
		{
			Assert.AreEqual("variable", Template.Parse("{%for i in (1..2) %}{% assign a = 'variable'%}{% endfor %}{{a}}").Render());
		}

		[Test]
		public void TestCaseDetectsBadSyntax()
		{
			Assert.Throws<SyntaxException>(() => Helper.AssertTemplateResult("", "{% case false %}{% when %}true{% endcase %}", new Hash()));
			Assert.Throws<SyntaxException>(() => Helper.AssertTemplateResult("", "{% case false %}{% huh %}true{% endcase %}", new Hash()));
		}

		[Test]
		public void TestCycle()
		{
			Helper.AssertTemplateResult("one", "{%cycle 'one', 'two'%}");
			Helper.AssertTemplateResult("one two", "{%cycle 'one', 'two'%} {%cycle 'one', 'two'%}");
			Helper.AssertTemplateResult(" two", "{%cycle '', 'two'%} {%cycle '', 'two'%}");

			Helper.AssertTemplateResult("one two one", "{%cycle 'one', 'two'%} {%cycle 'one', 'two'%} {%cycle 'one', 'two'%}");

			Helper.AssertTemplateResult("text-align: left text-align: right",
				"{%cycle 'text-align: left', 'text-align: right' %} {%cycle 'text-align: left', 'text-align: right'%}");
		}

		[Test]
		public void TestMultipleCycles()
		{
			Helper.AssertTemplateResult("1 2 1 1 2 3 1",
				"{%cycle 1,2%} {%cycle 1,2%} {%cycle 1,2%} {%cycle 1,2,3%} {%cycle 1,2,3%} {%cycle 1,2,3%} {%cycle 1,2,3%}");
		}

		[Test]
		public void TestMultipleNamedCycles()
		{
			Helper.AssertTemplateResult("one one two two one one",
				"{%cycle 1: 'one', 'two' %} {%cycle 2: 'one', 'two' %} {%cycle 1: 'one', 'two' %} {%cycle 2: 'one', 'two' %} {%cycle 1: 'one', 'two' %} {%cycle 2: 'one', 'two' %}");
		}

		[Test]
		public void TestMultipleNamedCyclesWithNamesFromContext()
		{
			Hash assigns = Hash.FromAnonymousObject(new { var1 = 1, var2 = 2 });
			Helper.AssertTemplateResult("one one two two one one",
				"{%cycle var1: 'one', 'two' %} {%cycle var2: 'one', 'two' %} {%cycle var1: 'one', 'two' %} {%cycle var2: 'one', 'two' %} {%cycle var1: 'one', 'two' %} {%cycle var2: 'one', 'two' %}",
				assigns);
		}

		[Test]
		public void TestSizeOfArray()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4 } });
			Helper.AssertTemplateResult("array has 4 elements", "array has {{ array.size }} elements", assigns);
		}

		[Test]
		public void TestSizeOfHash()
		{
			Hash assigns = Hash.FromAnonymousObject(new { hash = Hash.FromAnonymousObject(new { a = 1, b = 2, c = 3, d = 4 }) });
			Helper.AssertTemplateResult("hash has 4 elements", "hash has {{ hash.size }} elements", assigns);
		}

		[Test]
		public void TestIllegalSymbols()
		{
			Helper.AssertTemplateResult("", "{% if true == empty %}?{% endif %}", new Hash());
			Helper.AssertTemplateResult("", "{% if true == null %}?{% endif %}", new Hash());
			Helper.AssertTemplateResult("", "{% if empty == true %}?{% endif %}", new Hash());
			Helper.AssertTemplateResult("", "{% if null == true %}?{% endif %}", new Hash());
		}

		[Test]
		public void TestForReversed()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } });
			Helper.AssertTemplateResult("321", "{%for item in array reversed %}{{item}}{%endfor%}", assigns);
		}

		[Test]
		public void TestIfChanged()
		{
			Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 1, 2, 2, 3, 3 } });
			Helper.AssertTemplateResult("123", "{%for item in array%}{%ifchanged%}{{item}}{% endifchanged %}{%endfor%}", assigns);

			assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 1, 1, 1 } });
			Helper.AssertTemplateResult("1", "{%for item in array%}{%ifchanged%}{{item}}{% endifchanged %}{%endfor%}", assigns);
		}
	}
}