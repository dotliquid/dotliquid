using System;
using System.Globalization;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class VariableResolutionTests
    {
        [Test]
        public void TestSimpleVariable()
        {
            Template template = Template.Parse("{{test}}");
            ClassicAssert.AreEqual("worked", template.Render(Hash.FromAnonymousObject(new { test = "worked" })));
            ClassicAssert.AreEqual("worked wonderfully", template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully" })));
        }

        [Test]
        public void TestSimpleWithWhitespaces()
        {
            Template template = Template.Parse("  {{ test }}  ");
            ClassicAssert.AreEqual("  worked  ", template.Render(Hash.FromAnonymousObject(new { test = "worked" })));
            ClassicAssert.AreEqual("  worked wonderfully  ", template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully" })));
        }

        [Test]
        public void TestIgnoreUnknown()
        {
            Template template = Template.Parse("{{ test }}");
            ClassicAssert.AreEqual("", template.Render());
        }

        [Test]
        public void TestHashScoping()
        {
            Template template = Template.Parse("{{ test.test }}");
            ClassicAssert.AreEqual("worked", template.Render(Hash.FromAnonymousObject(new { test = new { test = "worked" } })));
        }

        [Test]
        public void TestPresetAssigns()
        {
            Template template = Template.Parse("{{ test }}");
            template.Assigns["test"] = "worked";
            ClassicAssert.AreEqual("worked", template.Render());
        }

        [Test]
        public void TestReuseParsedTemplate()
        {
            Template template = Template.Parse("{{ greeting }} {{ name }}");
            template.Assigns["greeting"] = "Goodbye";
            ClassicAssert.AreEqual("Hello Tobi", template.Render(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })));
            ClassicAssert.AreEqual("Hello ", template.Render(Hash.FromAnonymousObject(new { greeting = "Hello", unknown = "Tobi" })));
            ClassicAssert.AreEqual("Hello Brian", template.Render(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Brian" })));
            ClassicAssert.AreEqual("Goodbye Brian", template.Render(Hash.FromAnonymousObject(new { name = "Brian" })));
            CollectionAssert.AreEqual(Hash.FromAnonymousObject(new { greeting = "Goodbye" }), template.Assigns);
        }

        [Test]
        public void TestAssignsNotPollutedFromTemplate()
        {
            Template template = Template.Parse("{{ test }}{% assign test = 'bar' %}{{ test }}");
            template.Assigns["test"] = "baz";
            ClassicAssert.AreEqual("bazbar", template.Render());
            ClassicAssert.AreEqual("bazbar", template.Render());
            ClassicAssert.AreEqual("foobar", template.Render(Hash.FromAnonymousObject(new { test = "foo" })));
            ClassicAssert.AreEqual("bazbar", template.Render());
        }

        [Test]
        public void TestHashWithDefaultProc()
        {
            Template template = Template.Parse("Hello {{ test }}");
            Hash assigns = new Hash((h, k) => { throw new Exception("Unknown variable '" + k + "'"); });
            assigns["test"] = "Tobi";
            ClassicAssert.AreEqual("Hello Tobi", template.Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                RethrowErrors = true
            }));
            assigns.Remove("test");
            Exception ex = ClassicAssert.Throws<Exception>(() => template.Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                RethrowErrors = true
            }));
            ClassicAssert.AreEqual("Unknown variable 'test'", ex.Message);
        }
    }
}
