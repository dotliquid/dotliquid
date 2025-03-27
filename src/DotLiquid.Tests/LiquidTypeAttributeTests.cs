using DotLiquid.Tests.Helpers;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class LiquidTypeAttributeTests
    {
        #region Classes used in tests

        [LiquidType]
        private class MyLiquidTypeWithNoAllowedMembers
        {
            public string Name { get; set; }
        }

        [LiquidType("Name")]
        private class MyLiquidTypeWithAllowedMember
        {
            public string Name { get; set; }
        }

        [LiquidType("*")]
        private class MyLiquidTypeWithGlobalMemberAllowance
        {
            public string Name { get; set; }
        }

        [LiquidType("*")]
        private class MyLiquidTypeWithGlobalMemberAllowanceAndHiddenChild
        {
            public string Name { get; set; }
            public MyLiquidTypeWithNoAllowedMembers Child { get; set; }
        }

        [LiquidType("*")]
        private class MyLiquidTypeWithGlobalMemberAllowanceAndExposedChild
        {
            public string Name { get; set; }
            public MyLiquidTypeWithAllowedMember Child { get; set; }
        }

        [LiquidType("*")]
        private class MyLiquidTypeWithReservedKeyword
        {
            public string Type { get; set; }
        }

        [LiquidType("*")]
        private class MyLiquidTypeWithConflictingGetter
        {
            public string Name { get; set; }

            public string GetName() => $"GetName: {Name}";
        }

        #endregion

        [Test]
        public void TestLiquidTypeAttributeWithNoAllowedMembers()
        {
            Template template = Template.Parse("{{context.Name}}");
            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithNoAllowedMembers() { Name = "worked" } }));
            Assert.That(output, Is.EqualTo(""));
        }

        [Test]
        public void TestLiquidTypeAttributeWithAllowedMember()
        {
            Template template = Template.Parse("{{context.Name}}");
            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithAllowedMember() { Name = "worked" } }));
            Assert.That(output, Is.EqualTo("worked"));
        }

        [Test]
        public void TestLiquidTypeAttributeWithGlobalMemberAllowance()
        {
            Template template = Template.Parse("{{context.Name}}");
            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithGlobalMemberAllowance() { Name = "worked" } }));
            Assert.That(output, Is.EqualTo("worked"));
        }

        [Test]
        public void TestLiquidTypeAttributeWithGlobalMemberAllowanceDoesNotExposeHiddenChildMembers()
        {
            Template template = Template.Parse("|{{context.Name}}|{{context.Child.Name}}|");
            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithGlobalMemberAllowanceAndHiddenChild() { Name = "worked_parent", Child = new MyLiquidTypeWithNoAllowedMembers() { Name = "worked_child" } } }));
            Assert.That(output, Is.EqualTo("|worked_parent||"));
        }

        [Test]
        public void TestLiquidTypeAttributeWithGlobalMemberAllowanceDoesExposeValidChildMembers()
        {
            Template template = Template.Parse("|{{context.Name}}|{{context.Child.Name}}|");
            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidTypeWithGlobalMemberAllowanceAndExposedChild() { Name = "worked_parent", Child = new MyLiquidTypeWithAllowedMember() { Name = "worked_child" } } }));
            Assert.That(output, Is.EqualTo("|worked_parent|worked_child|"));
        }

        [Test]
        public void TestLiquidTypeWithReservedKeyword()
        {
            var reservedType = new MyLiquidTypeWithReservedKeyword() { Type = "worked" };
            var namingConvention = new NamingConventions.RubyNamingConvention();

            Helper.AssertTemplateResult(
              expected: "worked",
              template: "{{type}}",
                anonymousObject: reservedType,
                namingConvention: namingConvention);

            Helper.AssertTemplateResult(
                expected: "worked",
                template: "{{data.type}}",
                anonymousObject: new { data = reservedType },
                namingConvention: namingConvention);
        }

        [Test]
        public void TestLiquidTypeWithConflictingGetter()
        {
            var reservedType = new MyLiquidTypeWithConflictingGetter() { Name = "worked" };
            var namingConvention = new NamingConventions.RubyNamingConvention();

            Helper.AssertTemplateResult(
              expected: "worked",
              template: "{{name}}",
                anonymousObject: reservedType,
                namingConvention: namingConvention);

            Helper.AssertTemplateResult(
                expected: "worked",
                template: "{{data.name}}",
                anonymousObject: new { data = reservedType },
                namingConvention: namingConvention);
        }

        [Test]
        public void TestLiquidTypeAccessToGlobalToString()
        {
            Helper.AssertTemplateResult(
                expected: "DotLiquid.Tests.LiquidTypeAttributeTests+MyLiquidTypeWithGlobalMemberAllowance",
                template: "{{ value.to_string }}",
                anonymousObject: new { value = new MyLiquidTypeWithGlobalMemberAllowance() },
                namingConvention: new NamingConventions.RubyNamingConvention());
        }


        [Test]
        public void TestLiquidTypeRootKeys()
        {
            Helper.AssertTemplateResult(
                expected: "worked",
                template: "{{ name }}",
                localVariables: DropBase.FromSafeType(new MyLiquidTypeWithAllowedMember() { Name = "worked" }),
                namingConvention: new NamingConventions.RubyNamingConvention());

            Helper.AssertTemplateResult(
                expected: "worked",
                template: "{{ prop_allowed }}",
                localVariables: DropBase.FromSafeType(new Helper.DataObjectRegistered() { PropAllowed = "worked" }),
                namingConvention: new NamingConventions.RubyNamingConvention());
        }

        [Test]
        public void TestNonSafeTypeException()
        {
            Assert.Throws<Exceptions.ArgumentException>(() => DropBase.FromSafeType(string.Empty));
            Assert.That(DropBase.TryFromSafeType(string.Empty, out _), Is.False);

            Template.RegisterSafeType(typeof(TemplateTests.MySimpleType), o => o.ToString());
            Assert.Throws<Exceptions.ArgumentException>(() => DropBase.FromSafeType(new TemplateTests.MySimpleType()));
            Assert.That(DropBase.TryFromSafeType(new TemplateTests.MySimpleType(), out _), Is.False);
        }

        [Test]
        public void TestLiquidTypeParsesAllowedMembers()
        {
            Assert.That(DropProxy.TryFromLiquidType(new MyLiquidTypeWithNoAllowedMembers(), typeof(MyLiquidTypeWithNoAllowedMembers), out var noAllowedDrop), Is.True);
            Assert.That(noAllowedDrop.CreateTypeResolution(typeof(MyLiquidTypeWithNoAllowedMembers)).CachedProperties, Has.Exactly(0).Items);
            Assert.That(DropProxy.TryFromLiquidType(new MyLiquidTypeWithAllowedMember(), typeof(MyLiquidTypeWithAllowedMember), out var allowedDrop), Is.True);
            Assert.That(allowedDrop.CreateTypeResolution(typeof(MyLiquidTypeWithAllowedMember)).CachedProperties, Has.Exactly(1).Items);
        }

        [Test]
        public void TestNonLiquidTypeException()
        {
            Assert.That(DropProxy.TryFromLiquidType(string.Empty, typeof(string), out _), Is.False);
            Assert.That(DropProxy.TryFromLiquidType(new TemplateTests.MySimpleType(), typeof(TemplateTests.MySimpleType), out _), Is.False);
        }
    }
}
