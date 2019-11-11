using NUnit.Framework;
using DotLiquid.Exceptions;
using DotLiquid.NamingConventions;
using System.Threading.Tasks;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StudlyCapsTests
    {
        [Test]
        public async Task TestSimpleVariablesStudlyCaps()
        {
            Template.NamingConvention = new RubyNamingConvention();
            Template template = Template.Parse("{{ Greeting }} {{ Name }}");
            Assert.AreEqual("Hello Tobi", await template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })));

            Template.NamingConvention = new CSharpNamingConvention();
            Assert.AreEqual("Hello Tobi", await template.RenderAsync(Hash.FromAnonymousObject(new { Greeting = "Hello", Name = "Tobi" })));
            Assert.AreEqual(" ", await template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })));
        }

        [Test]
        public void TestTagsStudlyCapsAreNotAllowed()
        {
            Template.NamingConvention = new RubyNamingConvention();
            Assert.Throws<SyntaxException>(() => Template.Parse("{% IF user = 'tobi' %}Hello Tobi{% EndIf %}"));
        }

        [Test]
        public async Task TestFiltersStudlyCapsAreNotAllowed()
        {
            Template.NamingConvention = new RubyNamingConvention();
            Template template = Template.Parse("{{ 'hi tobi' | upcase }}");
            Assert.AreEqual("HI TOBI", await template.RenderAsync());

            Template.NamingConvention = new CSharpNamingConvention();
            template = Template.Parse("{{ 'hi tobi' | Upcase }}");
            Assert.AreEqual("HI TOBI", await template.RenderAsync());
        }

        [Test]
        public async Task TestAssignsStudlyCaps()
        {
            Template.NamingConvention = new RubyNamingConvention();

            await Helper.AssertTemplateResultAsync(".foo.", "{% assign FoO = values %}.{{ fOo[0] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
            await Helper.AssertTemplateResultAsync(".bar.", "{% assign fOo = values %}.{{ fOO[1] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));

            Template.NamingConvention = new CSharpNamingConvention();

            await Helper.AssertTemplateResultAsync(".foo.", "{% assign Foo = values %}.{{ Foo[0] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
            await Helper.AssertTemplateResultAsync(".bar.", "{% assign fOo = values %}.{{ fOo[1] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
        }
    }
}
