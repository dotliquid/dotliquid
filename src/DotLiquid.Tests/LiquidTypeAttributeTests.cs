using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	internal class LiquidTypeAttributeTests
	{
		[LiquidType]
		public class MyLiquidTypeWithNoAllowedMembers
		{
			public string Name { get; set; }
		}

		[LiquidType("Name")]
		public class MyLiquidTypeWithAllowedMember
		{
			public string Name { get; set; }
		}

		[Test]
		public void TestLiquidTypeAttributeWithNoAllowedMembers()
		{
			Template template = Template.Parse("{{context.Name}}");
			var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithNoAllowedMembers() { Name = "worked" } }));
			Assert.AreEqual("", output);
		}

		[Test]
		public void TestLiquidTypeAttributeWithAllowedMember()
		{
			Template template = Template.Parse("{{context.Name}}");
			var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithAllowedMember() { Name = "worked" } }));
			Assert.AreEqual("worked", output);
		}
	}
}