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
            Assert.AreEqual(string.Empty, Template.Parse("{% literal %}{% endliteral %}").Render());

            // Next test is specific to legacy parser and was removed from Ruby Liquid. Test that it is ignored is in TestShortHandSyntaxIsIgnored
            Assert.AreEqual(string.Empty, Template.Parse("{{{}}}", SyntaxCompatibility.DotLiquid20).Render());
        }

        [Test]
        public void TestSimpleLiteralValue()
        {
            Assert.AreEqual("howdy", Template.Parse("{% literal %}howdy{% endliteral %}").Render());
        }

        [Test]
        public void TestLiteralsIgnoreLiquidMarkup()
        {
            Assert.AreEqual(
                expected: "{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}",
                actual: Template.Parse("{% literal %}{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}{% endliteral %}").Render());
        }

        [Test]
        public void TestShorthandSyntax()
        {
            Assert.AreEqual(
                expected: "{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}",
                actual: Template.Parse("{{{{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}}}}", SyntaxCompatibility.DotLiquid20).Render());
        }

        [Test]
        public void TestLiteralsDontRemoveComments()
        {
            Assert.AreEqual("{# comment #}", Template.Parse("{{{ {# comment #} }}}", SyntaxCompatibility.DotLiquid20).Render());
        }

        [Test]
        public void TestFromShorthand()
        {
            Assert.AreEqual("{% literal %}gnomeslab{% endliteral %}", Literal.FromShortHand("{{{gnomeslab}}}"));
            Assert.AreEqual(null, Literal.FromShortHand(null));
        }

        [Test]
        public void TestFromShorthandIgnoresImproperSyntax()
        {
            Assert.AreEqual("{% if 'hi' == 'hi' %}hi{% endif %}", Literal.FromShortHand("{% if 'hi' == 'hi' %}hi{% endif %}"));
        }
    }
}
