using System.Collections.Generic;
using DotLiquid.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class ListExtensionMethodsTests
    {
        [Test]
        public void TestGetAtIndexForNullList()
        {
            var list = (List<string>)null;

            ClassicAssert.IsNull(list.TryGetAtIndex(0));
            ClassicAssert.IsNull(list.TryGetAtIndex(-1));
            ClassicAssert.IsNull(list.TryGetAtIndex(1));
        }

        [Test]
        public void TestGetAtIndexForEmptyList()
        {
            var list = new List<string>();

            ClassicAssert.IsNull(list.TryGetAtIndex(1));
            ClassicAssert.IsNull(list.TryGetAtIndex(-1));
            ClassicAssert.IsNull(list.TryGetAtIndex(1));
        }

        [Test]
        public void TestGetAtIndexForNonNullList()
        {
            const string item0 = "foo";
            const string item1 = "bar";
            var list = new List<string> { item0, item1 };

            ClassicAssert.AreEqual(item0, list.TryGetAtIndex(0));
            ClassicAssert.AreEqual(item1, list.TryGetAtIndex(1));
            ClassicAssert.IsNull(list.TryGetAtIndex(2));
            ClassicAssert.IsNull(list.TryGetAtIndex(-1));
        }

        [Test]
        public void TestGetAtIndexReverseForNullList()
        {
            var list = (List<string>)null;

            ClassicAssert.IsNull(list.TryGetAtIndexReverse(0));
            ClassicAssert.IsNull(list.TryGetAtIndexReverse(-1));
            ClassicAssert.IsNull(list.TryGetAtIndexReverse(1));
        }

        [Test]
        public void TestGetAtIndexReverseForEmptyList()
        {
            var list = new List<string>();

            ClassicAssert.IsNull(list.TryGetAtIndexReverse(1));
            ClassicAssert.IsNull(list.TryGetAtIndexReverse(-1));
            ClassicAssert.IsNull(list.TryGetAtIndexReverse(1));
        }

        [Test]
        public void TestGetAtIndexReverseForNonNullList()
        {
            const string item0 = "foo";
            const string item1 = "bar";
            var list = new List<string> { item0, item1 };

            ClassicAssert.AreEqual(item1, list.TryGetAtIndexReverse(0));
            ClassicAssert.AreEqual(item0, list.TryGetAtIndexReverse(1));
            ClassicAssert.IsNull(list.TryGetAtIndexReverse(2));
            ClassicAssert.IsNull(list.TryGetAtIndexReverse(-1));
        }
    }
}