using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotLiquid.Util;
using NUnit.Framework;


namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class PermissiveStringComparerTests
    {
        [Test]
        public void PermissiveStringComparerPascalCaseTests()
        {
            var comparer = new PermissiveStringComparer();
            Assert.True(comparer.Equals("AddDate", "AddDate"));
            Assert.True(comparer.Equals("AddDate", "addDate"));
            Assert.True(comparer.Equals("AddDate", "add_date"));
            Assert.False(comparer.Equals("Add Date", "AddDate"));
        }

        [Test]
        public void PermissiveStringComparerCamelCaseTests()
        {
            var comparer = new PermissiveStringComparer();
            Assert.True(comparer.Equals("addDate", "AddDate"));
            Assert.True(comparer.Equals("addDate", "addDate"));
            Assert.True(comparer.Equals("addDate", "add_date"));
            Assert.False(comparer.Equals("add Date", "AddDate"));
        }

        [Test]
        public void PermissiveStringComparerSnakeCaseTests()
        {
            var comparer = new PermissiveStringComparer();
            Assert.True(comparer.Equals("add_date", "AddDate"));
            Assert.True(comparer.Equals("add_date", "addDate"));
            Assert.True(comparer.Equals("add_date", "add_date"));
            Assert.False(comparer.Equals("Add Date", "add_date"));
        }
    }
}
