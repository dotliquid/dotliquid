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
				string templatePath = (string) context[templateName];

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

		[TestFixtureSetUp]
		public void SetUp()
		{
			_originalFileSystem = Template.FileSystem;
			Template.FileSystem = new TestFileSystem();
		}

		[TestFixtureTearDown]
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

			StringAssert.Contains("test", template.Render());
		}

		[Test]
		public void CanInherit()
		{
			Template template = Template.Parse(@"{% extends 'complex' %}");

			StringAssert.Contains("thing block", template.Render());
		}

		[Test]
		public void CanInheritAndReplaceBlocks()
		{
			Template template = Template.Parse(
				@"{% extends 'complex' %}
                    {% block another %}
                        new content for another
                    {% endblock %}");

			StringAssert.Contains("new content for another", template.Render());
		}

		[Test]
		public void CanProcessNestedInheritance()
		{
			Template template = Template.Parse(
				@"{% extends 'nested' %}
                    {% block thing %}
                        replacing block thing
                    {% endblock %}");

			StringAssert.Contains("replacing block thing", template.Render());
			StringAssert.DoesNotContain("thing block", template.Render());
		}

		[Test]
		public void CanRenderSuper()
		{
			Template template = Template.Parse(
				@"{% extends 'complex' %}
                    {% block another %}
                        {{ block.super }} + some other content
                    {% endblock %}");

			StringAssert.Contains("another block", template.Render());
			StringAssert.Contains("some other content", template.Render());
		}

	    [Test]
	    public void CanDefineBlockInInheritedBlock()
	    {
	        Template template = Template.Parse(
	            @"{% extends 'middle' %}
                  {% block middle %}C{% endblock %}");
            Assert.AreEqual("ABCYZ", template.Render());
	    }

        [Test]
        public void CanDefineContentInInheritedBlockFromAboveParent()
        {
            Template template = Template.Parse(
                @"{% extends 'middle' %}
                  {% block start %}!{% endblock %}");
            Assert.AreEqual("!ABYZ", template.Render());
        }

        [Test]
        public void CanRenderBlockContainedInConditional()
        {
            Template template = Template.Parse(
                @"{% extends 'middleunless' %}
                  {% block middle %}C{% endblock %}");
            Assert.AreEqual("ABCYZ", template.Render());

            template = Template.Parse(
                @"{% extends 'middleunless' %}
                  {% block start %}{% assign nomiddle = true %}{% endblock %}
                  {% block middle %}C{% endblock %}");
            Assert.AreEqual("ABYZ", template.Render());
        }

        [Test]
        public void RepeatedRendersProduceSameResult()
        {
            Template template = Template.Parse(
                @"{% extends 'middle' %}
                  {% block start %}!{% endblock %}
                  {% block middle %}C{% endblock %}");
            Assert.AreEqual("!ABCYZ", template.Render());
            Assert.AreEqual("!ABCYZ", template.Render());
        }
	}
}