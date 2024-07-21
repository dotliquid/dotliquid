using DotLiquid.Exceptions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual(text, template.Render());
            ClassicAssert.AreEqual(1, template.Root.NodeList.Count);
            ClassicAssert.IsInstanceOf<string>(template.Root.NodeList[0]);
        }

        [Test]
        public void TestRaiseOnSingleCloseBrace()
        {
            ClassicAssert.Throws<SyntaxException>(() => Template.Parse("text {{method} oh nos!"));
        }

        [Test]
        public void TestRaiseOnLabelAndNoCloseBrace()
        {
            ClassicAssert.Throws<SyntaxException>(() => Template.Parse("TEST {{ "));
        }

        [Test]
        public void TestRaiseOnLabelAndNoCloseBracePercent()
        {
            ClassicAssert.Throws<SyntaxException>(() => Template.Parse("TEST {% "));
        }

        [Test]
        public void TestErrorOnEmptyFilter()
        {
            ClassicAssert.DoesNotThrow(() =>
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
            SyntaxException ex = ClassicAssert.Throws<SyntaxException>(() => template.Render(new RenderParameters(System.Globalization.CultureInfo.InvariantCulture)
            {
                LocalVariables = Hash.FromAnonymousObject(new { x = "" }),
                ErrorsOutputMode = ErrorsOutputMode.Rethrow,
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22
            }));
            ClassicAssert.AreEqual(
                expected: string.Format(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), variableName),
                actual: ex.Message);

            template = Template.Parse("{{ x[" + variableName + "] }}");
            ex = ClassicAssert.Throws<SyntaxException>(() => template.Render(new RenderParameters(System.Globalization.CultureInfo.InvariantCulture)
            {
                LocalVariables = Hash.FromAnonymousObject(new { x = new { x = "" } }),
                ErrorsOutputMode = ErrorsOutputMode.Rethrow,
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22
            }));
            ClassicAssert.AreEqual(
                expected: string.Format(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), variableName),
                actual: ex.Message);
        }

        [Test]
        public void TestNestedVariableNotTerminated()
        {
            var template = Template.Parse("{{ x[[] }}");
            var ex = ClassicAssert.Throws<SyntaxException>(() => template.Render(new RenderParameters(System.Globalization.CultureInfo.InvariantCulture)
            {
                LocalVariables = Hash.FromAnonymousObject(new { x = new { x = "" } }),
                ErrorsOutputMode = ErrorsOutputMode.Rethrow,
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22
            }));
            ClassicAssert.AreEqual(
                expected: string.Format(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), "["),
                actual: ex.Message);
        }

        [TestCase("[\"]")]
        [TestCase("[\"\"")]
        [TestCase("[']")]
        public void TestVariableTokenizerNotTerminated(string variableName)
        {
            var ex = ClassicAssert.Throws<SyntaxException>(() => Tokenizer.GetVariableEnumerator(variableName).MoveNext());
            ClassicAssert.AreEqual(
                expected: string.Format(Liquid.ResourceManager.GetString("VariableNotTerminatedException"), variableName),
                actual: ex.Message);
        }

        [Test]
        public void TestShortHandSyntaxIsIgnored()
        {
            // These tests are based on actual handling on Ruby Liquid, not indicative of wanted behavior. Behavior for legacy dotliquid parser is in TestEmptyLiteral
            ClassicAssert.AreEqual("}", Template.Parse("{{{}}}", SyntaxCompatibility.DotLiquid22).Render());
            ClassicAssert.AreEqual("{##}", Template.Parse("{##}", SyntaxCompatibility.DotLiquid22).Render());
        }
    }
}
