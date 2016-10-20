using System;
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
            Assert.AreEqual("worked", template.Render(Hash.FromAnonymousObject(new { test = "worked" })));
            Assert.AreEqual("worked wonderfully", template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully" })));
        }

        [Test]
        public void TestSimpleWithWhitespaces()
        {
            Template template = Template.Parse("  {{ test }}  ");
            Assert.AreEqual("  worked  ", template.Render(Hash.FromAnonymousObject(new { test = "worked" })));
            Assert.AreEqual("  worked wonderfully  ", template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully" })));
        }

        [Test]
        public void TestIgnoreUnknown()
        {
            Template template = Template.Parse("{{ test }}");
            Assert.AreEqual("", template.Render());
        }

        [Test]
        public void TestHashScoping()
        {
            Template template = Template.Parse("{{ test.test }}");
            Assert.AreEqual("worked", template.Render(Hash.FromAnonymousObject(new { test = new { test = "worked" } })));
        }

        [Test]
        public void TestHashWithDefaultProc()
        {
            Template template = Template.Parse("Hello {{ test }}");
            Hash assigns = new Hash((h, k) => { throw new Exception("Unknown variable '" + k + "'"); });
            assigns["test"] = "Tobi";
            Assert.AreEqual("Hello Tobi", template.Render(new RenderParameters
            {
                LocalVariables = assigns,
                RethrowErrors = true
            }));
            assigns.Remove("test");
            Exception ex = Assert.Throws<Exception>(() => template.Render(new RenderParameters
            {
                LocalVariables = assigns,
                RethrowErrors = true
            }));
            Assert.AreEqual("Unknown variable 'test'", ex.Message);
        }
    }
}
