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
            var template = "{{ Greeting }} {{ Name }}";
            Helper.AssertTemplateResult(
                expected: "Hello Tobi",
                template: template,
                anonymousObject: new { greeting = "Hello", name = "Tobi" },
                namingConvention: new RubyNamingConvention());

            var csNamingConvention = new CSharpNamingConvention();
            Helper.AssertTemplateResult(
                expected: "Hello Tobi",
                template: template,
                anonymousObject: new { Greeting = "Hello", Name = "Tobi" },
                namingConvention: csNamingConvention);
            Helper.AssertTemplateResult(
                expected: " ",
                template: template,
                anonymousObject: new { greeting = "Hello", name = "Tobi" },
                namingConvention: csNamingConvention);
        }

        [Test]
        public void TestTagsStudlyCapsAreNotAllowed()
        {
            lock (Template.NamingConvention)
            {
                var currentNamingConvention = Template.NamingConvention;
                Template.NamingConvention = new RubyNamingConvention();

                try
                {
                    Assert.Throws<SyntaxException>(() => Template.Parse("{% IF user = 'tobi' %}Hello Tobi{% EndIf %}"));
                }
                finally
                {
                    Template.NamingConvention = currentNamingConvention;
                }
            }
        }

        [Test]
        public void TestFiltersStudlyCapsAreNotAllowed()
        {
            Helper.AssertTemplateResult(
                expected:"HI TOBI",
                template: "{{ 'hi tobi' | upcase }}",
                namingConvention: new RubyNamingConvention());

            Helper.AssertTemplateResult(
                expected: "HI TOBI",
                template: "{{ 'hi tobi' | Upcase }}",
                namingConvention: new CSharpNamingConvention());
        }

        [Test]
        public void TestAssignsStudlyCaps()
        {
            var rubyNamingConvention = new RubyNamingConvention();

            Helper.AssertTemplateResult(
                expected: ".foo.",
                template: "{% assign FoO = values %}.{{ fOo[0] }}.",
                anonymousObject: new { values = new[] { "foo", "bar", "baz" } },
                namingConvention: rubyNamingConvention);
            Helper.AssertTemplateResult(
                expected: ".bar.",
                template: "{% assign fOo = values %}.{{ fOO[1] }}.",
                anonymousObject: new { values = new[] { "foo", "bar", "baz" } },
                namingConvention: rubyNamingConvention);

            var csNamingConvention = new CSharpNamingConvention();

            Helper.AssertTemplateResult(
                expected: ".foo.",
                template: "{% assign Foo = values %}.{{ Foo[0] }}.",
                anonymousObject: new { values = new[] { "foo", "bar", "baz" } },
                namingConvention: csNamingConvention);
            Helper.AssertTemplateResult(
                expected: ".bar.",
                template: "{% assign fOo = values %}.{{ fOo[1] }}.",
                anonymousObject: new { values = new[] { "foo", "bar", "baz" } },
                namingConvention: csNamingConvention);
        }
    }
}
