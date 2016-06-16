using System.Collections.Generic;

using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class ListExtensionMethodsTests
    {
        [Test]
        public void TestGetAtIndexForNullList()
        {
            var list = (List<object>)null;

            Assert.IsNull(list.TryGetAtIndex(0));
            Assert.IsNull(list.TryGetAtIndex(-1));
            Assert.IsNull(list.TryGetAtIndex(100));
        }

        [Test]
        public void TestGetAtIndexForEmptyList()
        {
            var list = new List<string>();

            Assert.IsNull(list.TryGetAtIndex(1));
            Assert.IsNull(list.TryGetAtIndex(-1));
            Assert.IsNull(list.TryGetAtIndex(100));
        }

        [Test]
        public void TestGetAtIndexForNonNullList()
        {
            const string item = "foo";
            var list = new List<string> { item };

            Assert.AreEqual(item, list.TryGetAtIndex(0));
            Assert.IsNull(list.TryGetAtIndex(1));
            Assert.IsNull(list.TryGetAtIndex(-1));
            Assert.IsNull(list.TryGetAtIndex(100));
        }
    }
}