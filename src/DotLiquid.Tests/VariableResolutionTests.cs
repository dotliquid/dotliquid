using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class VariableResolutionTests
    {
        [Test]
        public void TestSimpleVariable()
        {
            Template template = Template.Parse("{{test}}");
            Assert.That(template.Render(Hash.FromAnonymousObject(new { test = "worked" })), Is.EqualTo("worked"));
            Assert.That(template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully" })), Is.EqualTo("worked wonderfully"));
        }

        [Test]
        public void TestSimpleWithWhitespaces()
        {
            Template template = Template.Parse("  {{ test }}  ");
            Assert.That(template.Render(Hash.FromAnonymousObject(new { test = "worked" })), Is.EqualTo("  worked  "));
            Assert.That(template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully" })), Is.EqualTo("  worked wonderfully  "));
        }

        [Test]
        public void TestIgnoreUnknown()
        {
            Template template = Template.Parse("{{ test }}");
            Assert.That(template.Render(), Is.EqualTo(""));
        }

        [Test]
        public void TestHashScoping()
        {
            Template template = Template.Parse("{{ test.test }}");
            Assert.That(template.Render(Hash.FromAnonymousObject(new { test = new { test = "worked" } })), Is.EqualTo("worked"));
        }

        [Test]
        public void TestPresetAssigns()
        {
            Template template = Template.Parse("{{ test }}");
            template.Assigns["test"] = "worked";
            Assert.That(template.Render(), Is.EqualTo("worked"));
        }

        [Test]
        public void TestReuseParsedTemplate()
        {
            Template template = Template.Parse("{{ greeting }} {{ name }}");
            template.Assigns["greeting"] = "Goodbye";
            Assert.That(template.Render(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })), Is.EqualTo("Hello Tobi"));
            Assert.That(template.Render(Hash.FromAnonymousObject(new { greeting = "Hello", unknown = "Tobi" })), Is.EqualTo("Hello "));
            Assert.That(template.Render(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Brian" })), Is.EqualTo("Hello Brian"));
            Assert.That(template.Render(Hash.FromAnonymousObject(new { name = "Brian" })), Is.EqualTo("Goodbye Brian"));
            Assert.That(template.Assigns, Is.EqualTo(Hash.FromAnonymousObject(new { greeting = "Goodbye" })).AsCollection);
        }

        [Test]
        public void TestAssignsNotPollutedFromTemplate()
        {
            Template template = Template.Parse("{{ test }}{% assign test = 'bar' %}{{ test }}");
            template.Assigns["test"] = "baz";
            Assert.That(template.Render(), Is.EqualTo("bazbar"));
            Assert.That(template.Render(), Is.EqualTo("bazbar"));
            Assert.That(template.Render(Hash.FromAnonymousObject(new { test = "foo" })), Is.EqualTo("foobar"));
            Assert.That(template.Render(), Is.EqualTo("bazbar"));
        }

        [Test]
        public void TestHashWithDefaultProc()
        {
            Template template = Template.Parse("Hello {{ test }}");
            Hash assigns = new Hash((h, k) => { throw new Exception("Unknown variable '" + k + "'"); });
            assigns["test"] = "Tobi";
            Assert.That(template.Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                RethrowErrors = true
            }), Is.EqualTo("Hello Tobi"));
            assigns.Remove("test");
            Exception ex = Assert.Throws<Exception>(() => template.Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                RethrowErrors = true
            }));
            Assert.That(ex.Message, Is.EqualTo("Unknown variable 'test'"));
        }
    }
}
