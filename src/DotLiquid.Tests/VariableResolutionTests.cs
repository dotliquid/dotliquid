using System;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class VariableResolutionTests
    {
        [Test]
        public async Task TestSimpleVariable()
        {
            Template template = Template.Parse("{{test}}");
            Assert.AreEqual("worked", await template.RenderAsync(Hash.FromAnonymousObject(new { test = "worked" })));
            Assert.AreEqual("worked wonderfully", await template.RenderAsync(Hash.FromAnonymousObject(new { test = "worked wonderfully" })));
        }

        [Test]
        public async Task TestSimpleWithWhitespaces()
        {
            Template template = Template.Parse("  {{ test }}  ");
            Assert.AreEqual("  worked  ", await template.RenderAsync(Hash.FromAnonymousObject(new { test = "worked" })));
            Assert.AreEqual("  worked wonderfully  ", await template.RenderAsync(Hash.FromAnonymousObject(new { test = "worked wonderfully" })));
        }

        [Test]
        public async Task TestIgnoreUnknown()
        {
            Template template = Template.Parse("{{ test }}");
            Assert.AreEqual("", await template.RenderAsync());
        }

        [Test]
        public async Task TestHashScoping()
        {
            Template template = Template.Parse("{{ test.test }}");
            Assert.AreEqual("worked", await template.RenderAsync(Hash.FromAnonymousObject(new { test = new { test = "worked" } })));
        }

        [Test]
        public async Task TestPresetAssigns()
        {
            Template template = Template.Parse("{{ test }}");
            template.Assigns["test"] = "worked";
            Assert.AreEqual("worked", await template.RenderAsync());
        }

        [Test]
        public async Task TestReuseParsedTemplate()
        {
            Template template = Template.Parse("{{ greeting }} {{ name }}");
            template.Assigns["greeting"] = "Goodbye";
            Assert.AreEqual("Hello Tobi", await template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })));
            Assert.AreEqual("Hello ", await template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", unknown = "Tobi" })));
            Assert.AreEqual("Hello Brian", await template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Brian" })));
            Assert.AreEqual("Goodbye Brian", await template.RenderAsync(Hash.FromAnonymousObject(new { name = "Brian" })));
            CollectionAssert.AreEqual(Hash.FromAnonymousObject(new { greeting = "Goodbye" }), template.Assigns);
        }

        [Test]
        public async Task TestAssignsNotPollutedFromTemplate()
        {
            Template template = Template.Parse("{{ test }}{% assign test = 'bar' %}{{ test }}");
            template.Assigns["test"] = "baz";
            Assert.AreEqual("bazbar", await template.RenderAsync());
            Assert.AreEqual("bazbar", await template.RenderAsync());
            Assert.AreEqual("foobar", await template.RenderAsync(Hash.FromAnonymousObject(new { test = "foo" })));
            Assert.AreEqual("bazbar", await template.RenderAsync());
        }

        [Test]
        public async Task TestHashWithDefaultProc()
        {
            Template template = Template.Parse("Hello {{ test }}");
            Hash assigns = new Hash((h, k) => { throw new Exception("Unknown variable '" + k + "'"); });
            assigns["test"] = "Tobi";
            Assert.AreEqual("Hello Tobi", await template.RenderAsync(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                RethrowErrors = true
            }));
            assigns.Remove("test");
            Exception ex = Assert.Throws<Exception>(() => template.RenderAsync(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                RethrowErrors = true
            }).GetAwaiter().GetResult());
            Assert.AreEqual("Unknown variable 'test'", ex.Message);
        }
    }
}
