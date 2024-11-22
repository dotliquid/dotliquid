using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ParsingQuirksTests
    {
        [Test]
        public void TestErrorWithCss()
        {
            const string text = " div { font-weight: bold; } ";
            Template template = Template.Parse(text);
            Assert.That(template.Render(), Is.EqualTo(text));
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(1));
            Assert.That(template.Root.NodeList[0], Is.InstanceOf<string>());
        }

        [Test]
        public void TestRaiseOnSingleCloseBrace()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse("text {{method} oh nos!"));
        }

        [Test]
        public void TestRaiseOnLabelAndNoCloseBrace()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse("TEST {{ "));
        }

        [Test]
        public void TestRaiseOnLabelAndNoCloseBracePercent()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse("TEST {% "));
        }

        [Test]
        public void TestErrorOnEmptyFilter()
        {
            Assert.DoesNotThrow(() =>
            {
                Template.Parse("{{test |a|b|}}");
                Template.Parse("{{test}}");
                Template.Parse("{{|test|}}");
            });
        }

        [Test]
        public void TestMeaninglessParens()
        {
            Hash assigns = Hash.FromAnonymousObject(new { b = "bar", c = "baz" });
            Helper.AssertTemplateResult(" YES ", "{% if a == 'foo' or (b == 'bar' and c == 'baz') or false %} YES {% endif %}", assigns);
        }

        [Test]
        public void TestUnexpectedCharactersSilentlyEatLogic()
        {
            Helper.AssertTemplateResult(" YES ", "{% if true && false %} YES {% endif %}");
            Helper.AssertTemplateResult("", "{% if false || true %} YES {% endif %}");
        }

        [Test]
        public void TestLiquidTagsInQuotes()
        {
            Helper.AssertTemplateResult("{{ {% %} }}", "{{ '{{ {% %} }}' }}");
            Helper.AssertTemplateResult("{{ {% %} }}", "{% assign x = '{{ {% %} }}' %}{{x}}");
        }

        [TestCase(".")]
        [TestCase("x.")]
        [TestCase("$x")]
        [TestCase("x?")]
        [TestCase("x¿")]
        [TestCase(".y")]
        public void TestVariableNotTerminatedFromInvalidVariableName(string variableName)
        {
            var template = Template.Parse("{{ " + variableName + " }}");
            SyntaxException ex = Assert.Throws<SyntaxException>(() => template.Render(new RenderParameters(System.Globalization.CultureInfo.InvariantCulture)
            {
                LocalVariables = Hash.FromAnonymousObject(new { x = "" }),
                ErrorsOutputMode = ErrorsOutputMode.Rethrow,
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22
            }));
            Assert.That(
                actual: ex.Message, Is.EqualTo(expected: string.Format(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), variableName)));

            template = Template.Parse("{{ x[" + variableName + "] }}");
            ex = Assert.Throws<SyntaxException>(() => template.Render(new RenderParameters(System.Globalization.CultureInfo.InvariantCulture)
            {
                LocalVariables = Hash.FromAnonymousObject(new { x = new { x = "" } }),
                ErrorsOutputMode = ErrorsOutputMode.Rethrow,
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22
            }));
            Assert.That(
                actual: ex.Message, Is.EqualTo(expected: string.Format(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), variableName)));
        }

        [Test]
        public void TestNestedVariableNotTerminated()
        {
            var template = Template.Parse("{{ x[[] }}");
            var ex = Assert.Throws<SyntaxException>(() => template.Render(new RenderParameters(System.Globalization.CultureInfo.InvariantCulture)
            {
                LocalVariables = Hash.FromAnonymousObject(new { x = new { x = "" } }),
                ErrorsOutputMode = ErrorsOutputMode.Rethrow,
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22
            }));
            Assert.That(
                actual: ex.Message, Is.EqualTo(expected: string.Format(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), "[")));
        }

        [TestCase("[\"]")]
        [TestCase("[\"\"")]
        [TestCase("[']")]
        public void TestVariableTokenizerNotTerminated(string variableName)
        {
            var ex = Assert.Throws<SyntaxException>(() => Tokenizer.GetVariableEnumerator(variableName).MoveNext());
            Assert.That(
                actual: ex.Message, Is.EqualTo(expected: string.Format(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), variableName)));
        }

        [Test]
        public void TestShortHandSyntaxIsIgnored()
        {
            // These tests are based on actual handling on Ruby Liquid, not indicative of wanted behavior. Behavior for legacy dotliquid parser is in TestEmptyLiteral
            Assert.That(Template.Parse("{{{}}}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("}"));
            Assert.That(Template.Parse("{##}", SyntaxCompatibility.DotLiquid22).Render(), Is.EqualTo("{##}"));
        }
    }
}
