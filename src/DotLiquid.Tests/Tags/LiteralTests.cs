using System;
using System.Collections.Generic;
using DotLiquid.Exceptions;
using NUnit.Framework;
using DotLiquid.Tags;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class LiteralTests
    {
        [Test]
        public void TestEmptyLiteral()
        {
            Assert.That(Template.Parse("{% literal %}{% endliteral %}").Render(), Is.EqualTo(string.Empty));

            // Next test is specific to legacy parser and was removed from Ruby Liquid. Test that it is ignored is in TestShortHandSyntaxIsIgnored
            Assert.That(Template.Parse("{{{}}}", SyntaxCompatibility.DotLiquid20).Render(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestSimpleLiteralValue()
        {
            Assert.That(Template.Parse("{% literal %}howdy{% endliteral %}").Render(), Is.EqualTo("howdy"));
        }

        [Test]
        public void TestLiteralsIgnoreLiquidMarkup()
        {
            Assert.That(
                actual: Template.Parse("{% literal %}{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}{% endliteral %}").Render(), Is.EqualTo(expected: "{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}"));
        }

        [Test]
        public void TestShorthandSyntax()
        {
            Assert.That(
                actual: Template.Parse("{{{{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}}}}", SyntaxCompatibility.DotLiquid20).Render(), Is.EqualTo(expected: "{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}"));
        }

        [Test]
        public void TestLiteralsDontRemoveComments()
        {
            Assert.That(Template.Parse("{{{ {# comment #} }}}", SyntaxCompatibility.DotLiquid20).Render(), Is.EqualTo("{# comment #}"));
        }

        [Test]
        public void TestFromShorthand()
        {
            Assert.That(Literal.FromShortHand("{{{gnomeslab}}}"), Is.EqualTo("{% literal %}gnomeslab{% endliteral %}"));
            Assert.That(Literal.FromShortHand(null), Is.EqualTo(null));
        }

        [Test]
        public void TestFromShorthandIgnoresImproperSyntax()
        {
            Assert.That(Literal.FromShortHand("{% if 'hi' == 'hi' %}hi{% endif %}"), Is.EqualTo("{% if 'hi' == 'hi' %}hi{% endif %}"));
        }
    }
}
