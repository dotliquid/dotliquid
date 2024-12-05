using DotLiquid.FileSystems;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class InheritanceTests
    {
        private class TestFileSystem : IFileSystem
        {
            public string ReadTemplateFile(Context context, string templateName)
            {
                string templatePath = (string)context[templateName];

                switch (templatePath)
                {
                    case "simple":
                        return "test";
                    case "complex":
                        return @"some markup here...
                             {% block thing %}
                                 thing block
                             {% endblock %}
                             {% block another %}
                                 another block
                             {% endblock %}
                             ...and some markup here";
                    case "nested":
                        return @"{% extends 'complex' %}
                             {% block thing %}
                                another thing (from nested)
                             {% endblock %}";
                    case "outer":
                        return "{% block start %}{% endblock %}A{% block outer %}{% endblock %}Z";
                    case "middle":
                        return @"{% extends 'outer' %}
                             {% block outer %}B{% block middle %}{% endblock %}Y{% endblock %}";
                    case "middleunless":
                        return @"{% extends 'outer' %}
                             {% block outer %}B{% unless nomiddle %}{% block middle %}{% endblock %}{% endunless %}Y{% endblock %}";
                    default:
                        return @"{% extends 'complex' %}
                             {% block thing %}
                                thing block (from nested)
                             {% endblock %}";
                }
            }
        }

        private IFileSystem _originalFileSystem;

        [OneTimeSetUp]
        public void SetUp()
        {
            _originalFileSystem = Template.FileSystem;
            Template.FileSystem = new TestFileSystem();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Template.FileSystem = _originalFileSystem;
        }

        [Test]
        public void CanOutputTheContentsOfTheExtendedTemplate()
        {
            Template template = Template.Parse(
                                    @"{% extends 'simple' %}
                    {% block thing %}
                        yeah
                    {% endblock %}");

            Assert.That(template.Render(), Does.Contain("test"));
        }

        [Test]
        public void CanInherit()
        {
            Template template = Template.Parse(@"{% extends 'complex' %}");

            Assert.That(template.Render(), Does.Contain("thing block"));
        }

        [Test]
        public void CanInheritAndReplaceBlocks()
        {
            Template template = Template.Parse(
                                    @"{% extends 'complex' %}
                    {% block another %}
                      new content for another
                    {% endblock %}");

            Assert.That(template.Render(), Does.Contain("new content for another"));
        }

        [Test]
        public void CanProcessNestedInheritance()
        {
            Template template = Template.Parse(
                                    @"{% extends 'nested' %}
                  {% block thing %}
                  replacing block thing
                  {% endblock %}");

            Assert.That(template.Render(), Does.Contain("replacing block thing"));
            Assert.That(template.Render(), Does.Not.Contain("thing block"));
        }

        [Test]
        public void CanRenderSuper()
        {
            Template template = Template.Parse(
                                    @"{% extends 'complex' %}
                    {% block another %}
                        {{ block.super }} + some other content
                    {% endblock %}");

            Assert.That(template.Render(), Does.Contain("another block"));
            Assert.That(template.Render(), Does.Contain("some other content"));
        }

        [Test]
        public void CanDefineBlockInInheritedBlock()
        {
            Template template = Template.Parse(
                                    @"{% extends 'middle' %}
                  {% block middle %}C{% endblock %}");
            Assert.That(template.Render(), Is.EqualTo("ABCYZ"));
        }

        [Test]
        public void CanDefineContentInInheritedBlockFromAboveParent()
        {
            Template template = Template.Parse(@"{% extends 'middle' %}
                  {% block start %}!{% endblock %}");
            Assert.That(template.Render(), Is.EqualTo("!ABYZ"));
        }

        [Test]
        public void CanRenderBlockContainedInConditional()
        {
            Template template = Template.Parse(
                                    @"{% extends 'middleunless' %}
                  {% block middle %}C{% endblock %}");
            Assert.That(template.Render(), Is.EqualTo("ABCYZ"));

            template = Template.Parse(
                @"{% extends 'middleunless' %}
                  {% block start %}{% assign nomiddle = true %}{% endblock %}
                  {% block middle %}C{% endblock %}");
            Assert.That(template.Render(), Is.EqualTo("ABYZ"));
        }

        [Test]
        public void RepeatedRendersProduceSameResult()
        {
            Template template = Template.Parse(
                                    @"{% extends 'middle' %}
                  {% block start %}!{% endblock %}
                  {% block middle %}C{% endblock %}");
            Assert.That(template.Render(), Is.EqualTo("!ABCYZ"));
            Assert.That(template.Render(), Is.EqualTo("!ABCYZ"));
        }

        [Test]
        public void TestExtendFromTemplateFileSystem()
        {
            var fileSystem = new IncludeTagTests.TestTemplateFileSystem(new TestFileSystem());
            Template.FileSystem = fileSystem;
            for (int i = 0; i < 2; ++i)
            {
                Template template = Template.Parse(
                                    @"{% extends 'simple' %}
                    {% block thing %}
                        yeah
                    {% endblock %}");
                Assert.That(template.Render(), Does.Contain("test"));
            }
            Assert.That(1, Is.EqualTo(fileSystem.CacheHitTimes));
        }
    }
}
