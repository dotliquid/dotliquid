using NUnit.Framework;
using DotLiquid.Exceptions;
using DotLiquid.NamingConventions;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StudlyCapsTests
    {
        [Test]
        public void TestSimpleVariablesStudlyCaps()
        {
            Template.NamingConvention = new RubyNamingConvention();
            Template template = Template.Parse("{{ Greeting }} {{ Name }}");
            Assert.AreEqual("Hello Tobi", template.Render(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })));

            Template.NamingConvention = new CSharpNamingConvention();
            Assert.AreEqual("Hello Tobi", template.Render(Hash.FromAnonymousObject(new { Greeting = "Hello", Name = "Tobi" })));
            Assert.AreEqual(" ", template.Render(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })));
        }

        [Test]
        public void TestTagsStudlyCapsAreNotAllowed()
        {
            Template.NamingConvention = new RubyNamingConvention();
            Assert.Throws<SyntaxException>(() => Template.Parse("{% IF user = 'tobi' %}Hello Tobi{% EndIf %}"));
        }

        [Test]
        public void TestFiltersStudlyCapsAreNotAllowed()
        {
            Template.NamingConvention = new RubyNamingConvention();
            Template template = Template.Parse("{{ 'hi tobi' | upcase }}");
            Assert.AreEqual("HI TOBI", template.Render());

            Template.NamingConvention = new CSharpNamingConvention();
            template = Template.Parse("{{ 'hi tobi' | Upcase }}");
            Assert.AreEqual("HI TOBI", template.Render());
        }

        [Test]
        public void TestAssignsStudlyCaps()
        {
            Template.NamingConvention = new RubyNamingConvention();

            Helper.AssertTemplateResult(
                expected: ".foo.",
                template: "{% assign FoO = values %}.{{ fOo[0] }}.",
                localVariables: Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
            Helper.AssertTemplateResult(
                expected: ".bar.",
                template: "{% assign fOo = values %}.{{ fOO[1] }}.",
                localVariables: Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));

            Template.NamingConvention = new CSharpNamingConvention();

            Helper.AssertTemplateResult(
                expected: ".foo.",
                template: "{% assign Foo = values %}.{{ Foo[0] }}.",
                localVariables: Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
            Helper.AssertTemplateResult(
                expected: ".bar.",
                template: "{% assign fOo = values %}.{{ fOo[1] }}.",
                localVariables: Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
        }
    }
}
