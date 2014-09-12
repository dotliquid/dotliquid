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
			CollectionAssert.AreEqual(new[] { "  " }, template.Root.NodeList);
		}

		[Test]
		public void TestVariableBeginning()
		{
			Template template = Template.Parse("{{funk}}  ");
			Assert.AreEqual(2, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
				new[] { typeof(Variable), typeof(string) });
		}

		[Test]
		public void TestVariableEnd()
		{
			Template template = Template.Parse("  {{funk}}");
			Assert.AreEqual(2, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
				new[] { typeof(string), typeof(Variable) });
		}

		[Test]
		public void TestVariableMiddle()
		{
			Template template = Template.Parse("  {{funk}}  ");
			Assert.AreEqual(3, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
				new[] { typeof(string), typeof(Variable), typeof(string) });
		}

		[Test]
		public void TestVariableManyEmbeddedFragments()
		{
			Template template = Template.Parse("  {{funk}} {{so}} {{brother}} ");
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
			Template template = Template.Parse("  {% comment %} {% endcomment %} ");
			Assert.AreEqual(3, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
				new[] { typeof(string), typeof(Comment), typeof(string) });
		}

		[Test]
		public void TestWithCustomTag()
		{
		    var config = new TemplateConfiguration().RegisterTag<Block>("testtag");
			Assert.DoesNotThrow(() => Template.Parse("{% testtag %} {% endtesttag %}", config));
		}
	}
}