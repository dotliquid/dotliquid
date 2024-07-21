using System;
using DotLiquid.Tags;
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
            Assert.That(template.Root.NodeList, Is.EquivalentTo(new[] { "  " }));
        }

        [Test]
        public void TestVariableBeginning()
        {
            Template template = Template.Parse("{{funk}}  ");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(2));
            Assert.That(template.Root.NodeList[0], Is.InstanceOf<Variable>());
            Assert.That(template.Root.NodeList[1], Is.InstanceOf<string>());
        }

        [Test]
        public void TestVariableEnd()
        {
            Template template = Template.Parse("  {{funk}}");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(2));
            Assert.That(template.Root.NodeList[0], Is.InstanceOf<string>());
            Assert.That(template.Root.NodeList[1], Is.InstanceOf<Variable>());
        }

        [Test]
        public void TestVariableMiddle()
        {
            Template template = Template.Parse("  {{funk}}  ");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(3));
            Assert.That(template.Root.NodeList[0], Is.InstanceOf<string>());
            Assert.That(template.Root.NodeList[1], Is.InstanceOf<Variable>());
            Assert.That(template.Root.NodeList[2], Is.InstanceOf<string>());
        }

        [Test]
        public void TestVariableManyEmbeddedFragments()
        {
            Template template = Template.Parse("  {{funk}} {{so}} {{brother}} ");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(7));
            Assert.That(template.Root.NodeList[0], Is.InstanceOf<string>());
            Assert.That(template.Root.NodeList[1], Is.InstanceOf<Variable>());
            Assert.That(template.Root.NodeList[2], Is.InstanceOf<string>());
            Assert.That(template.Root.NodeList[3], Is.InstanceOf<Variable>());
            Assert.That(template.Root.NodeList[4], Is.InstanceOf<string>());
            Assert.That(template.Root.NodeList[5], Is.InstanceOf<Variable>());
            Assert.That(template.Root.NodeList[6], Is.InstanceOf<string>());
        }

        [Test]
        public void TestWithBlock()
        {
            Template template = Template.Parse("  {% comment %} {% endcomment %} ");
            Assert.That(template.Root.NodeList.Count, Is.EqualTo(3));
            Assert.That(template.Root.NodeList[0], Is.InstanceOf<string>());
            Assert.That(template.Root.NodeList[1], Is.InstanceOf<Comment>());
            Assert.That(template.Root.NodeList[2], Is.InstanceOf<string>());
        }

        [Test]
        public void TestWithCustomTag()
        {
            Template.RegisterTag<Block>("testtag");
            Assert.That(() => Template.Parse("{% testtag %} {% endtesttag %}"), Throws.Nothing);
        }

        [Test]
        public void TestWithCustomTagFactory()
        {
            Template.RegisterTagFactory(new CustomTagFactory());
            Template result = null;
            Assert.That(() => result = Template.Parse("{% custom %}"), Throws.Nothing);
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
