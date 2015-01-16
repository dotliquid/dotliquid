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
			Assert.AreEqual("  ", template.Render());
		}

		[Test]
		public void TestVariableBeginning()
		{
			Template template = Template.Parse("{{funk}}  ");
			Assert.AreEqual(2, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
				new[] { typeof(Variable), typeof(StringRenderable) });
		}

		[Test]
		public void TestVariableEnd()
		{
			Template template = Template.Parse("  {{funk}}");
			Assert.AreEqual(2, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(StringRenderable), typeof(Variable) });
		}

		[Test]
		public void TestVariableMiddle()
		{
			Template template = Template.Parse("  {{funk}}  ");
			Assert.AreEqual(3, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(StringRenderable), typeof(Variable), typeof(StringRenderable) });
		}

		[Test]
		public void TestVariableManyEmbeddedFragments()
		{
			Template template = Template.Parse("  {{funk}} {{so}} {{brother}} ");
			Assert.AreEqual(7, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
				new[]
				{
					typeof(StringRenderable), typeof(Variable), typeof(StringRenderable),
					typeof(Variable), typeof(StringRenderable), typeof(Variable),
					typeof(StringRenderable)
				});
		}

		[Test]
		public void TestWithBlock()
		{
			Template template = Template.Parse("  {% comment %} {% endcomment %} ");
			Assert.AreEqual(3, template.Root.NodeList.Count);
			ExtendedCollectionAssert.AllItemsAreInstancesOfTypes(template.Root.NodeList,
                new[] { typeof(StringRenderable), typeof(Comment), typeof(StringRenderable) });
		}

		[Test]
		public void TestWithCustomTag()
		{
			Template.RegisterTag<Block>("testtag");
			Assert.DoesNotThrow(() => Template.Parse("{% testtag %} {% endtesttag %}"));
		}
	}
}