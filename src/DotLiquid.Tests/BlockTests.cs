using System;
using DotLiquid.Tags;
using DotLiquid.Tests.Framework;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class BlockTests
    {
        [Test]
        public void TestBlankspace()
        {
            Template template = Template.Parse("  ");
            Assert.That(template.Root.NodeList, Is.EqualTo(new[] { "  " }).AsCollection);
        }

        [Test]
        public void TestVariableBeginning()
        {
            Template template = Template.Parse("{{funk}}  ");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(2));
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(Variable), typeof(string) });
        }

        [Test]
        public void TestVariableEnd()
        {
            Template template = Template.Parse("  {{funk}}");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(2));
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(string), typeof(Variable) });
        }

        [Test]
        public void TestVariableMiddle()
        {
            Template template = Template.Parse("  {{funk}}  ");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(3));
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(string), typeof(Variable), typeof(string) });
        }

        [Test]
        public void TestVariableManyEmbeddedFragments()
        {
            Template template = Template.Parse("  {{funk}} {{so}} {{brother}} ");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(7));
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[]
                {
                    typeof(string), typeof(Variable), typeof(string),
                    typeof(Variable), typeof(string), typeof(Variable),
                    typeof(string)
                });
        }

        [Test]
        public void TestWithBlock()
        {
            Template template = Template.Parse("  {% comment %} {% endcomment %} ");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(3));
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(string), typeof(Comment), typeof(string) });
        }

        [Test]
        public void TestWithCustomTag()
        {
            Template.RegisterTag<Block>("testtag");
            Assert.That(Template.GetTagType("testtag"), Is.EqualTo(typeof(Block)));
            Assert.DoesNotThrow(() => Template.Parse("{% testtag %} {% endtesttag %}"));
        }

        [Test]
        public void TestWithCustomTagFactory()
        {
            Template.RegisterTagFactory(new Helpers.CustomTagFactory());
            Assert.That(Template.GetTagType("custom"), Is.Null);

            Template result = null;
            Assert.DoesNotThrow(() => result = Template.Parse("{% custom %}"));
            Assert.That(result.Render(), Is.EqualTo("I am a custom tag" + Environment.NewLine));
        }
    }
}
