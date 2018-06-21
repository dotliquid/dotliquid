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

        [LiquidType("*")]
        public class MyLiquidTypeWithGlobalMemberAllowance
        {
            public string Name { get; set; }
        }

        [LiquidType("*")]
        public class MyLiquidTypeWithGlobalMemberAllowanceAndHiddenChild
        {
            public string Name { get; set; }
            public MyLiquidTypeWithNoAllowedMembers Child { get; set; }
        }

        [LiquidType("*")]
        public class MyLiquidTypeWithGlobalMemberAllowanceAndExposedChild
        {
            public string Name { get; set; }
            public MyLiquidTypeWithAllowedMember Child { get; set; }
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

        [Test]
        public void TestLiquidTypeAttributeWithGlobalMemberAllowance()
        {
            Template template = Template.Parse("{{context.Name}}");
            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithGlobalMemberAllowance() { Name = "worked" } }));
            Assert.AreEqual("worked", output);
        }

        [Test]
        public void TestLiquidTypeAttributeWithGlobalMemberAllowanceDoesNotExposeHiddenChildMembers()
        {
            Template template = Template.Parse("|{{context.Name}}|{{context.Child.Name}}|");
            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithGlobalMemberAllowanceAndHiddenChild() { Name = "worked_parent", Child = new MyLiquidTypeWithNoAllowedMembers() { Name = "worked_child" } } }));
            Assert.AreEqual("|worked_parent||", output);
        }

        [Test]
        public void TestLiquidTypeAttributeWithGlobalMemberAllowanceDoesExposeValidChildMembers()
        {
            Template template = Template.Parse("|{{context.Name}}|{{context.Child.Name}}|");
            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithGlobalMemberAllowanceAndExposedChild() { Name = "worked_parent", Child = new MyLiquidTypeWithAllowedMember() { Name = "worked_child" } } }));
            Assert.AreEqual("|worked_parent|worked_child|", output);
        }
    }
}
