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
            var list = (List<string>)null;

            Assert.IsNull(list.TryGetAtIndex(0));
            Assert.IsNull(list.TryGetAtIndex(-1));
            Assert.IsNull(list.TryGetAtIndex(1));
        }

        [Test]
        public void TestGetAtIndexForEmptyList()
        {
            var list = new List<string>();

            Assert.IsNull(list.TryGetAtIndex(1));
            Assert.IsNull(list.TryGetAtIndex(-1));
            Assert.IsNull(list.TryGetAtIndex(1));
        }

        [Test]
        public void TestGetAtIndexForNonNullList()
        {
            const string item0 = "foo";
            const string item1 = "bar";
            var list = new List<string> { item0, item1 };

            Assert.AreEqual(item0, list.TryGetAtIndex(0));
            Assert.AreEqual(item1, list.TryGetAtIndex(1));
            Assert.IsNull(list.TryGetAtIndex(2));
            Assert.IsNull(list.TryGetAtIndex(-1));
        }

        [Test]
        public void TestGetAtIndexReverseForNullList()
        {
            var list = (List<string>)null;

            Assert.IsNull(list.TryGetAtIndexReverse(0));
            Assert.IsNull(list.TryGetAtIndexReverse(-1));
            Assert.IsNull(list.TryGetAtIndexReverse(1));
        }

        [Test]
        public void TestGetAtIndexReverseForEmptyList()
        {
            var list = new List<string>();

            Assert.IsNull(list.TryGetAtIndexReverse(1));
            Assert.IsNull(list.TryGetAtIndexReverse(-1));
            Assert.IsNull(list.TryGetAtIndexReverse(1));
        }

        [Test]
        public void TestGetAtIndexReverseForNonNullList()
        {
            const string item0 = "foo";
            const string item1 = "bar";
            var list = new List<string> { item0, item1 };

            Assert.AreEqual(item1, list.TryGetAtIndexReverse(0));
            Assert.AreEqual(item0, list.TryGetAtIndexReverse(1));
            Assert.IsNull(list.TryGetAtIndexReverse(2));
            Assert.IsNull(list.TryGetAtIndexReverse(-1));
        }
    }
}