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
            Assert.DoesNotThrow(() => Template.Parse("{% testtag %} {% endtesttag %}"));
        }

        [Test]
        public void TestWithCustomTagFactory()
        {
            Template.RegisterTagFactory(new CustomTagFactory());
            Template result = null;
            Assert.DoesNotThrow(() => result = Template.Parse("{% custom %}"));
            Assert.That(result.Render(), Is.EqualTo("I am a custom tag" + Environment.NewLine));
        }

        public class CustomTagFactory : ITagFactory
        {
            public string TagName
            {
                get { return "custom"; }
            }

            public Tag Create()
            {
                return new CustomTag();
            }

            public class CustomTag : Tag
            {
                public override void Render(Context context, System.IO.TextWriter result)
                {
                    result.WriteLine("I am a custom tag");
                }
            }
        }

    }
}
