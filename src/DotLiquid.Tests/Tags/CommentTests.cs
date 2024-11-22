using DotLiquid.Tags;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class CommentTests
    {
        [Test]
        public void TestEmptyComment()
        {
            Assert.That(Template.Parse("{% comment %}{% endcomment %}").Render(), Is.EqualTo(string.Empty));

            // Next test is specific to legacy parser and was removed from Ruby Liquid. Test that it is ignored is in TestShortHandSyntaxIsIgnored
            Assert.That(Template.Parse("{##}", SyntaxCompatibility.DotLiquid20).Render(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestSimpleCommentValue()
        {
            Assert.That(Template.Parse("{% comment %}howdy{% endcomment %}").Render(), Is.EqualTo(""));
        }

        [Test]
        public void TestCommentsIgnoreLiquidMarkup()
        {
            Assert.That(
                actual: Template.Parse("{% comment %}{% if 'gnomeslab' contains 'liquid' %}yes{% else %}no{ % endif %}{% endcomment %}").Render(), Is.EqualTo(expected: ""));
        }

        [Test]
        public void TestCommentShorthand()
        {
            Assert.That(Comment.FromShortHand("{# gnomeslab #}"), Is.EqualTo("{% comment %}gnomeslab{% endcomment %}"));
            Assert.That(Comment.FromShortHand(null), Is.EqualTo(null));

            Assert.That(
                actual: Template.Parse("{#{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}#}", SyntaxCompatibility.DotLiquid20).Render(), Is.EqualTo(expected: ""));
        }
    }
}
