using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class StandardTagTests
    {
        [Test]
        public async Task TestTag()
        {
            Tag tag = new Tag();
            tag.Initialize("tag", null, null);
            Assert.AreEqual("tag", tag.Name);
            Assert.AreEqual(string.Empty, await tag.RenderAsync(new Context(CultureInfo.InvariantCulture)));
        }

        [Test]
        public async Task TestNoTransform()
        {
            await Helper.AssertTemplateResultAsync("this text should come out of the template without change...",
                "this text should come out of the template without change...");
            await Helper.AssertTemplateResultAsync("blah", "blah");
            await Helper.AssertTemplateResultAsync("<blah>", "<blah>");
            await Helper.AssertTemplateResultAsync("|,.:", "|,.:");
            await Helper.AssertTemplateResultAsync("", "");

            const string text = @"this shouldnt see any transformation either but has multiple lines
                as you can clearly see here ...";
            await Helper.AssertTemplateResultAsync(text, text);
        }

        [Test]
        public async Task TestHasABlockWhichDoesNothing()
        {
            await Helper.AssertTemplateResultAsync("the comment block should be removed  .. right?",
                "the comment block should be removed {%comment%} be gone.. {%endcomment%} .. right?");

            await Helper.AssertTemplateResultAsync("", "{%comment%}{%endcomment%}");
            await Helper.AssertTemplateResultAsync("", "{%comment%}{% endcomment %}");
            await Helper.AssertTemplateResultAsync("", "{% comment %}{%endcomment%}");
            await Helper.AssertTemplateResultAsync("", "{% comment %}{% endcomment %}");
            await Helper.AssertTemplateResultAsync("", "{%comment%}comment{%endcomment%}");
            await Helper.AssertTemplateResultAsync("", "{% comment %}comment{% endcomment %}");

            await Helper.AssertTemplateResultAsync("foobar", "foo{%comment%}comment{%endcomment%}bar");
            await Helper.AssertTemplateResultAsync("foobar", "foo{% comment %}comment{% endcomment %}bar");
            await Helper.AssertTemplateResultAsync("foobar", "foo{%comment%} comment {%endcomment%}bar");
            await Helper.AssertTemplateResultAsync("foobar", "foo{% comment %} comment {% endcomment %}bar");

            await Helper.AssertTemplateResultAsync("foo  bar", "foo {%comment%} {%endcomment%} bar");
            await Helper.AssertTemplateResultAsync("foo  bar", "foo {%comment%}comment{%endcomment%} bar");
            await Helper.AssertTemplateResultAsync("foo  bar", "foo {%comment%} comment {%endcomment%} bar");

            await Helper.AssertTemplateResultAsync("foobar", @"foo{%comment%}
                {%endcomment%}bar");
        }

        [Test]
        public async Task TestForWithDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "Graham Greene", "English" },
                { "F. Scott Fitzgerald", "American" }
            };
            await Helper.AssertTemplateResultAsync(" English  American ", "{%for item in authors%} {{ item }} {%endfor%}",
                Hash.FromAnonymousObject(new { authors = dictionary.Values }));
        }

        [Test]
        public async Task TestForWithNestedDictionary()
        {
            var dictionary = new Dictionary<string, object> { {
            "People", 
            new Dictionary<string, object> {
                    { "ID1", new Dictionary<string, object>{ { "First", "Jane" }, { "Last", "Green" } } },
                    { "ID2", new Dictionary<string, object>{ { "First", "Mike" }, { "Last", "Doe" } } }
                }
            } };

            await Helper.AssertTemplateResultAsync("JaneMike", "{% for item in People %}{{ item.First }}{%endfor%}",
                Hash.FromDictionary(dictionary));
        }


        public class TestDictObject : Drop
        {
            public TestDictObject()
            {
                Testdict = new Dictionary<string, string>() { { "aa", "bb" }, { "dd", "ee" }, { "ff", "gg" } };
            }
            public Dictionary<string, string> Testdict { get; set; }
        }

        [Test]
        public async Task TestDictionaryFor()
        {
            var template = Template.Parse("{%for item in bla.testdict %}{{ item[0] }}-{{ item[1]}} {%endfor%}");
            var result = await template.RenderAsync(Hash.FromAnonymousObject(new { bla = new TestDictObject() }));
            Assert.AreEqual("aa-bb dd-ee ff-gg ", result);
        }

        [Test]
        public async Task TestFor()
        {
            await Helper.AssertTemplateResultAsync(" yo  yo  yo  yo ", "{%for item in array%} yo {%endfor%}",
                Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4 } }));
            await Helper.AssertTemplateResultAsync("yoyo", "{%for item in array%}yo{%endfor%}", Hash.FromAnonymousObject(new { array = new[] { 1, 2 } }));
            await Helper.AssertTemplateResultAsync(" yo ", "{%for item in array%} yo {%endfor%}", Hash.FromAnonymousObject(new { array = new[] { 1 } }));
            await Helper.AssertTemplateResultAsync("", "{%for item in array%}{%endfor%}", Hash.FromAnonymousObject(new { array = new[] { 1, 2 } }));
            const string expected = @"
  yo

  yo

  yo
";
            const string template = @"{%for item in array%}
  yo
{%endfor%}";
            await Helper.AssertTemplateResultAsync(expected, template, Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
        }

        [Test]
        public async Task TestForWithRange()
        {
            await Helper.AssertTemplateResultAsync(" 1  2  3 ", "{%for item in (1..3) %} {{item}} {%endfor%}");
        }

        [Test]
        public async Task TestForWithVariable()
        {
            await Helper.AssertTemplateResultAsync(" 1  2  3 ", "{%for item in array%} {{item}} {%endfor%}",
                Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
            await Helper.AssertTemplateResultAsync("123", "{%for item in array%}{{item}}{%endfor%}",
                Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
            await Helper.AssertTemplateResultAsync("123", "{% for item in array %}{{item}}{% endfor %}",
                Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
            await Helper.AssertTemplateResultAsync("abcd", "{%for item in array%}{{item}}{%endfor%}",
                Hash.FromAnonymousObject(new { array = new[] { "a", "b", "c", "d" } }));
            await Helper.AssertTemplateResultAsync("a b c", "{%for item in array%}{{item}}{%endfor%}",
                Hash.FromAnonymousObject(new { array = new[] { "a", " ", "b", " ", "c" } }));
            await Helper.AssertTemplateResultAsync("abc", "{%for item in array%}{{item}}{%endfor%}",
                Hash.FromAnonymousObject(new { array = new[] { "a", "", "b", "", "c" } }));
        }

        [Test]
        public async Task TestForHelpers()
        {
            Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } });
            await Helper.AssertTemplateResultAsync(" 1/3  2/3  3/3 ",
                "{%for item in array%} {{forloop.index}}/{{forloop.length}} {%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync(" 1  2  3 ", "{%for item in array%} {{forloop.index}} {%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync(" 0  1  2 ", "{%for item in array%} {{forloop.index0}} {%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync(" 2  1  0 ", "{%for item in array%} {{forloop.rindex0}} {%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync(" 3  2  1 ", "{%for item in array%} {{forloop.rindex}} {%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync(" true  false  false ", "{%for item in array%} {{forloop.first}} {%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync(" false  false  true ", "{%for item in array%} {{forloop.last}} {%endfor%}", assigns);
        }

        [Test]
        public async Task TestForAndIf()
        {
            await Helper.AssertTemplateResultAsync("+--", "{%for item in array%}{% if forloop.first %}+{% else %}-{% endif %}{%endfor%}",
                Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } }));
        }

        [Test]
        public async Task TestLimiting()
        {
            Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } });
            await Helper.AssertTemplateResultAsync("12", "{%for i in array limit:2 %}{{ i }}{%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync("1234", "{%for i in array limit:4 %}{{ i }}{%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync("3456", "{%for i in array limit:4 offset:2 %}{{ i }}{%endfor%}", assigns);
            await Helper.AssertTemplateResultAsync("3456", "{%for i in array limit: 4 offset: 2 %}{{ i }}{%endfor%}", assigns);
        }

        [Test]
        public async Task TestDynamicVariableLimiting()
        {
            Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } });
            assigns["limit"] = 2;
            assigns["offset"] = 2;
            await Helper.AssertTemplateResultAsync("34", "{%for i in array limit: limit offset: offset %}{{ i }}{%endfor%}", assigns);
        }

        [Test]
        public async Task TestNestedFor()
        {
            Hash assigns = Hash.FromAnonymousObject(new { array = new[] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 } } });
            await Helper.AssertTemplateResultAsync("123456", "{%for item in array%}{%for i in item%}{{ i }}{%endfor%}{%endfor%}", assigns);
        }

        [Test]
        public async Task TestOffsetOnly()
        {
            Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 } });
            await Helper.AssertTemplateResultAsync("890", "{%for i in array offset:7 %}{{ i }}{%endfor%}", assigns);
        }

        [Test]
        public async Task TestPauseResume()
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
            await Helper.AssertTemplateResultAsync(expected, markup, assigns);
        }

        [Test]
        public async Task TestPauseResumeLimit()
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
            await Helper.AssertTemplateResultAsync(expected, markup, assigns);
        }

        [Test]
        public async Task TestPauseResumeBigLimit()
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
            await Helper.AssertTemplateResultAsync(expected, markup, assigns);
        }

        [Test]
        public async Task TestPauseResumeBigOffset()
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
            await Helper.AssertTemplateResultAsync(expected, markup, assigns);
        }

        [Test]
        public async Task TestForWithBreak()
        {
            var assigns = Hash.FromAnonymousObject(new { array = new { items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } } });
            var markup = "{% for i in array.items %}{{ i }}{% if i > 3 %}{% break %}{% endif %}{% endfor %}";
            var expected = "1234";
            await Helper.AssertTemplateResultAsync(expected, markup, assigns);
        }

        [Test]
        public async Task TestForWithContinue()
        {
            var assigns = Hash.FromAnonymousObject(new { array = new { items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } } });
            var markup = "{% for i in array.items %}{% if i == 3 %}{% continue %}{% endif %}{{ i }}{% endfor %}";
            var expected = "1245678910";
            await Helper.AssertTemplateResultAsync(expected, markup, assigns);
        }

        [Test]
        public async Task TestBreakOutsideFor()
        {
            var markup = "123{% break %}456";
            var expected = "123";
            await Helper.AssertTemplateResultAsync(expected, markup);
        }

        [Test]
        public async Task TestContinueOutsideFor()
        {
        var markup = "123{% continue %}456";
        var expected = "123";
        await Helper.AssertTemplateResultAsync(expected, markup);
        }

        [Test]
        public async Task TestAssign()
        {
            Hash assigns = Hash.FromAnonymousObject(new { var = "content" });
            await Helper.AssertTemplateResultAsync("var2:  var2:content", "var2:{{var2}} {%assign var2 = var%} var2:{{var2}}", assigns);
        }

        [Test]
        public async Task TestHyphenatedAssign()
        {
            Hash assigns = Hash.FromDictionary(new Dictionary<string, object> { { "a-b", "1" } });
            await Helper.AssertTemplateResultAsync("a-b:1 a-b:2", "a-b:{{a-b}} {%assign a-b = 2 %}a-b:{{a-b}}", assigns);
        }

        [Test]
        public async Task TestAssignWithColonAndSpaces()
        {
            Hash assigns = Hash.FromAnonymousObject(new { var = new Dictionary<string, object> { { "a:b c", new { paged = 1 } } } });
            await Helper.AssertTemplateResultAsync("var2: 1", "{%assign var2 = var['a:b c'].paged %}var2: {{var2}}", assigns);
        }

        [Test]
        public async Task TestCapture()
        {
            Hash assigns = Hash.FromAnonymousObject(new { var = "content" });
            await Helper.AssertTemplateResultAsync("content foo content foo ",
                "{{ var2 }}{% capture var2 %}{{ var }} foo {% endcapture %}{{ var2 }}{{ var2 }}", assigns);
        }

        [Test]
        public void TestCaptureDetectsBadSyntax()
        {
            Assert.Throws<SyntaxException>(() =>
                Helper.AssertTemplateResultAsync("content foo content foo ", "{{ var2 }}{% capture %}{{ var }} foo {% endcapture %}{{ var2 }}{{ var2 }}", Hash.FromAnonymousObject(new { var = "content" })).GetAwaiter().GetResult());
        }

        [Test]
        public async Task TestCase()
        {
            Hash assigns = Hash.FromAnonymousObject(new { condition = 2 });
            await Helper.AssertTemplateResultAsync(" its 2 ", "{% case condition %}{% when 1 %} its 1 {% when 2 %} its 2 {% endcase %}",
                assigns);

            assigns = Hash.FromAnonymousObject(new { condition = 1 });
            await Helper.AssertTemplateResultAsync(" its 1 ", "{% case condition %}{% when 1 %} its 1 {% when 2 %} its 2 {% endcase %}",
                assigns);

            assigns = Hash.FromAnonymousObject(new { condition = 3 });
            await Helper.AssertTemplateResultAsync("", "{% case condition %}{% when 1 %} its 1 {% when 2 %} its 2 {% endcase %}", assigns);

            assigns = Hash.FromAnonymousObject(new { condition = "string here" });
            await Helper.AssertTemplateResultAsync(" hit ", "{% case condition %}{% when 'string here' %} hit {% endcase %}", assigns);

            assigns = Hash.FromAnonymousObject(new { condition = "bad string here" });
            await Helper.AssertTemplateResultAsync("", "{% case condition %}{% when 'string here' %} hit {% endcase %}", assigns);
        }

        [Test]
        public async Task TestCaseWithElse()
        {
            Hash assigns = Hash.FromAnonymousObject(new { condition = 5 });
            await Helper.AssertTemplateResultAsync(" hit ", "{% case condition %}{% when 5 %} hit {% else %} else {% endcase %}", assigns);

            assigns = Hash.FromAnonymousObject(new { condition = 6 });
            await Helper.AssertTemplateResultAsync(" else ", "{% case condition %}{% when 5 %} hit {% else %} else {% endcase %}", assigns);

            assigns = Hash.FromAnonymousObject(new { condition = 6 });
            await Helper.AssertTemplateResultAsync(" else ", "{% case condition %} {% when 5 %} hit {% else %} else {% endcase %}", assigns);
        }

        [Test]
        public async Task TestCaseOnSize()
        {
            await Helper.AssertTemplateResultAsync("", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
                Hash.FromAnonymousObject(new { a = new object[] { } }));
            await Helper.AssertTemplateResultAsync("1", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1 } }));
            await Helper.AssertTemplateResultAsync("2", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1, 1 } }));
            await Helper.AssertTemplateResultAsync("", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1 } }));
            await Helper.AssertTemplateResultAsync("", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1, 1 } }));
            await Helper.AssertTemplateResultAsync("", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1, 1, 1 } }));
        }

        [Test]
        public async Task TestCaseOnSizeWithElse()
        {
            await Helper.AssertTemplateResultAsync("else", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
                Hash.FromAnonymousObject(new { a = new object[] { } }));
            await Helper.AssertTemplateResultAsync("1", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1 } }));
            await Helper.AssertTemplateResultAsync("2", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1, 1 } }));
            await Helper.AssertTemplateResultAsync("else", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1 } }));
            await Helper.AssertTemplateResultAsync("else", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1, 1 } }));
            await Helper.AssertTemplateResultAsync("else", "{% case a.size %}{% when 1 %}1{% when 2 %}2{% else %}else{% endcase %}",
                Hash.FromAnonymousObject(new { a = new[] { 1, 1, 1, 1, 1 } }));
        }

        [Test]
        public async Task TestCaseOnLengthWithElse()
        {
            await Helper.AssertTemplateResultAsync("else",
                "{% case a.empty? %}{% when true %}true{% when false %}false{% else %}else{% endcase %}", new Hash());
            await Helper.AssertTemplateResultAsync("false",
                "{% case false %}{% when true %}true{% when false %}false{% else %}else{% endcase %}", new Hash());
            await Helper.AssertTemplateResultAsync("true",
                "{% case true %}{% when true %}true{% when false %}false{% else %}else{% endcase %}", new Hash());
            await Helper.AssertTemplateResultAsync("else",
                "{% case NULL %}{% when true %}true{% when false %}false{% else %}else{% endcase %}", new Hash());
        }

        [Test]
        public async Task TestAssignFromCase()
        {
            // Example from the shopify forums
            const string code = "{% case collection.handle %}{% when 'menswear-jackets' %}{% assign ptitle = 'menswear' %}{% when 'menswear-t-shirts' %}{% assign ptitle = 'menswear' %}{% else %}{% assign ptitle = 'womenswear' %}{% endcase %}{{ ptitle }}";
            Template template = Template.Parse(code);
            Assert.AreEqual("menswear", await template.RenderAsync(Hash.FromAnonymousObject(new { collection = new { handle = "menswear-jackets" } })));
            Assert.AreEqual("menswear", await template.RenderAsync(Hash.FromAnonymousObject(new { collection = new { handle = "menswear-t-shirts" } })));
            Assert.AreEqual("womenswear", await template.RenderAsync(Hash.FromAnonymousObject(new { collection = new { handle = "x" } })));
            Assert.AreEqual("womenswear", await template.RenderAsync(Hash.FromAnonymousObject(new { collection = new { handle = "y" } })));
            Assert.AreEqual("womenswear", await template.RenderAsync(Hash.FromAnonymousObject(new { collection = new { handle = "z" } })));
        }

        [Test]
        public async Task TestCaseWhenOr()
        {
            const string code1 = "{% case condition %}{% when 1 or 2 or 3 %} its 1 or 2 or 3 {% when 4 %} its 4 {% endcase %}";
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 1 }));
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 2 }));
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 3 }));
            await Helper.AssertTemplateResultAsync(" its 4 ", code1, Hash.FromAnonymousObject(new { condition = 4 }));
            await Helper.AssertTemplateResultAsync("", code1, Hash.FromAnonymousObject(new { condition = 5 }));

            const string code2 =
                "{% case condition %}{% when 1 or 'string' or null %} its 1 or 2 or 3 {% when 4 %} its 4 {% endcase %}";
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = 1 }));
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = "string" }));
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = (object) null }));
            await Helper.AssertTemplateResultAsync("", code2, Hash.FromAnonymousObject(new { condition = "something else" }));
        }

        [Test]
        public async Task TestCaseWhenComma()
        {
            const string code1 = "{% case condition %}{% when 1, 2, 3 %} its 1 or 2 or 3 {% when 4 %} its 4 {% endcase %}";
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 1 }));
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 2 }));
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code1, Hash.FromAnonymousObject(new { condition = 3 }));
            await Helper.AssertTemplateResultAsync(" its 4 ", code1, Hash.FromAnonymousObject(new { condition = 4 }));
            await Helper.AssertTemplateResultAsync("", code1, Hash.FromAnonymousObject(new { condition = 5 }));

            const string code2 =
                "{% case condition %}{% when 1, 'string', null %} its 1 or 2 or 3 {% when 4 %} its 4 {% endcase %}";
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = 1 }));
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = "string" }));
            await Helper.AssertTemplateResultAsync(" its 1 or 2 or 3 ", code2, Hash.FromAnonymousObject(new { condition = (object) null }));
            await Helper.AssertTemplateResultAsync("", code2, Hash.FromAnonymousObject(new { condition = "something else" }));
        }

        [Test]
        public async Task TestAssign2()
        {
            Assert.AreEqual("variable", await Template.Parse("{% assign a = 'variable' %}{{a}}").RenderAsync());
        }

        [Test]
        public async Task TestAssignAnEmptyString()
        {
            Assert.AreEqual("", await Template.Parse("{% assign a = '' %}{{a}}").RenderAsync());
        }

        [Test]
        public async Task TestAssignIsGlobal()
        {
            Assert.AreEqual("variable", await Template.Parse("{%for i in (1..2) %}{% assign a = 'variable'%}{% endfor %}{{a}}").RenderAsync());
        }

        [Test]
        public void TestCaseDetectsBadSyntax()
        {
            Assert.Throws<SyntaxException>(() => Helper.AssertTemplateResultAsync("", "{% case false %}{% when %}true{% endcase %}", new Hash()).GetAwaiter().GetResult());
            Assert.Throws<SyntaxException>(() => Helper.AssertTemplateResultAsync("", "{% case false %}{% huh %}true{% endcase %}", new Hash()).GetAwaiter().GetResult());
        }

        [Test]
        public async Task TestCycle()
        {
            await Helper.AssertTemplateResultAsync("one", "{%cycle 'one', 'two'%}");
            await Helper.AssertTemplateResultAsync("one two", "{%cycle 'one', 'two'%} {%cycle 'one', 'two'%}");
            await Helper.AssertTemplateResultAsync(" two", "{%cycle '', 'two'%} {%cycle '', 'two'%}");

            await Helper.AssertTemplateResultAsync("one two one", "{%cycle 'one', 'two'%} {%cycle 'one', 'two'%} {%cycle 'one', 'two'%}");

            await Helper.AssertTemplateResultAsync("text-align: left text-align: right",
                "{%cycle 'text-align: left', 'text-align: right' %} {%cycle 'text-align: left', 'text-align: right'%}");
        }

        [Test]
        public async Task TestMultipleCycles()
        {
            await Helper.AssertTemplateResultAsync("1 2 1 1 2 3 1",
                "{%cycle 1,2%} {%cycle 1,2%} {%cycle 1,2%} {%cycle 1,2,3%} {%cycle 1,2,3%} {%cycle 1,2,3%} {%cycle 1,2,3%}");
        }

        [Test]
        public async Task TestMultipleNamedCycles()
        {
            await Helper.AssertTemplateResultAsync("one one two two one one",
                "{%cycle 1: 'one', 'two' %} {%cycle 2: 'one', 'two' %} {%cycle 1: 'one', 'two' %} {%cycle 2: 'one', 'two' %} {%cycle 1: 'one', 'two' %} {%cycle 2: 'one', 'two' %}");
        }

        [Test]
        public async Task TestMultipleNamedCyclesWithNamesFromContext()
        {
            Hash assigns = Hash.FromAnonymousObject(new { var1 = 1, var2 = 2 });
            await Helper.AssertTemplateResultAsync("one one two two one one",
                "{%cycle var1: 'one', 'two' %} {%cycle var2: 'one', 'two' %} {%cycle var1: 'one', 'two' %} {%cycle var2: 'one', 'two' %} {%cycle var1: 'one', 'two' %} {%cycle var2: 'one', 'two' %}",
                assigns);
        }

        [Test]
        public async Task TestSizeOfArray()
        {
            Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3, 4 } });
            await Helper.AssertTemplateResultAsync("array has 4 elements", "array has {{ array.size }} elements", assigns);
        }

        [Test]
        public async Task TestSizeOfHash()
        {
            Hash assigns = Hash.FromAnonymousObject(new { hash = Hash.FromAnonymousObject(new { a = 1, b = 2, c = 3, d = 4 }) });
            await Helper.AssertTemplateResultAsync("hash has 4 elements", "hash has {{ hash.size }} elements", assigns);
        }

        [Test]
        public async Task TestIllegalSymbols()
        {
            await Helper.AssertTemplateResultAsync("", "{% if true == empty %}?{% endif %}", new Hash());
            await Helper.AssertTemplateResultAsync("", "{% if true == null %}?{% endif %}", new Hash());
            await Helper.AssertTemplateResultAsync("", "{% if empty == true %}?{% endif %}", new Hash());
            await Helper.AssertTemplateResultAsync("", "{% if null == true %}?{% endif %}", new Hash());
        }

        [Test]
        public async Task TestForReversed()
        {
            Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } });
            await Helper.AssertTemplateResultAsync("321", "{%for item in array reversed %}{{item}}{%endfor%}", assigns);
        }

        [Test]
        public async Task TestIfChanged()
        {
            Hash assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 1, 2, 2, 3, 3 } });
            await Helper.AssertTemplateResultAsync("123", "{%for item in array%}{%ifchanged%}{{item}}{% endifchanged %}{%endfor%}", assigns);

            assigns = Hash.FromAnonymousObject(new { array = new[] { 1, 1, 1, 1 } });
            await Helper.AssertTemplateResultAsync("1", "{%for item in array%}{%ifchanged%}{{item}}{% endifchanged %}{%endfor%}", assigns);
        }
    }
}
