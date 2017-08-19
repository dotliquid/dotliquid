using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class PermissiveNamingConventionTests
    {
        [Test]
        public void PermissiveStringComparerPascalCaseTests()
        {
            var convention = new PermissiveNamingConvention();
            Assert.True(convention.OperatorEquals("AddDate", "AddDate"));
            Assert.True(convention.OperatorEquals("AddDate", "addDate"));
            Assert.True(convention.OperatorEquals("AddDate", "add_date"));
            Assert.False(convention.OperatorEquals("Add Date", "AddDate"));
        }

        [Test]
        public void PermissiveStringComparerCamelCaseTests()
        {
            var convention = new PermissiveNamingConvention();
            Assert.True(convention.OperatorEquals("addDate", "AddDate"));
            Assert.True(convention.OperatorEquals("addDate", "addDate"));
            Assert.True(convention.OperatorEquals("addDate", "add_date"));
            Assert.False(convention.OperatorEquals("add Date", "AddDate"));
        }

        [Test]
        public void PermissiveStringComparerSnakeCaseTests()
        {
            var convention = new PermissiveNamingConvention();
            Assert.True(convention.OperatorEquals("add_date", "AddDate"));
            Assert.True(convention.OperatorEquals("add_date", "addDate"));
            Assert.True(convention.OperatorEquals("add_date", "add_date"));
            Assert.False(convention.OperatorEquals("Add Date", "add_date"));
        }
    }
}
