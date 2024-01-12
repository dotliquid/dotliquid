using System;
using DotLiquid.NamingConventions;
using DotLiquid.Tags;
using DotLiquid.Tests.Framework;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class BlockTests
    {
        private INamingConvention NamingConvention { get; } = new RubyNamingConvention();
        [Test]
        public void TestBlankspace()
        {
            Template template = Template.Parse("  ", NamingConvention);
            CollectionAssert.AreEqual(new[] { "  " }, template.Root.NodeList);
        }

        [Test]
        public void TestVariableBeginning()
        {
            Template template = Template.Parse("{{funk}}  ", NamingConvention);
            Assert.AreEqual(2, template.Root.NodeList.Count);
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(Variable), typeof(string) });
        }

        [Test]
        public void TestVariableEnd()
        {
            Template template = Template.Parse("  {{funk}}", NamingConvention);
            Assert.AreEqual(2, template.Root.NodeList.Count);
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(string), typeof(Variable) });
        }

        [Test]
        public void TestVariableMiddle()
        {
            Template template = Template.Parse("  {{funk}}  ", NamingConvention);
            Assert.AreEqual(3, template.Root.NodeList.Count);
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(string), typeof(Variable), typeof(string) });
        }

        [Test]
        public void TestVariableManyEmbeddedFragments()
        {
            Template template = Template.Parse("  {{funk}} {{so}} {{brother}} ", NamingConvention);
            Assert.AreEqual(7, template.Root.NodeList.Count);
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
            Template template = Template.Parse("  {% comment %} {% endcomment %} ", NamingConvention);
            Assert.AreEqual(3, template.Root.NodeList.Count);
            ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(string), typeof(Comment), typeof(string) });
        }

        [Test]
        public void TestWithCustomTag()
        {
            Template.RegisterTag<Block>("testtag");
            Assert.DoesNotThrow(() => Template.Parse("{% testtag %} {% endtesttag %}", NamingConvention));
        }

        [Test]
        public void TestWithCustomTagFactory()
        {
            Template.RegisterTagFactory(new CustomTagFactory());
            Template result = null;
            Assert.DoesNotThrow(() => result = Template.Parse("{% custom %}", NamingConvention));
            Assert.AreEqual("I am a custom tag"+Environment.NewLine, result.Render());
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
