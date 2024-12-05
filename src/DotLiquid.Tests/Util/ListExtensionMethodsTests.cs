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

            Assert.That(list.TryGetAtIndex(0), Is.Null);
            Assert.That(list.TryGetAtIndex(-1), Is.Null);
            Assert.That(list.TryGetAtIndex(1), Is.Null);
        }

        [Test]
        public void TestGetAtIndexForEmptyList()
        {
            var list = new List<string>();

            Assert.That(list.TryGetAtIndex(1), Is.Null);
            Assert.That(list.TryGetAtIndex(-1), Is.Null);
            Assert.That(list.TryGetAtIndex(1), Is.Null);
        }

        [Test]
        public void TestGetAtIndexForNonNullList()
        {
            const string item0 = "foo";
            const string item1 = "bar";
            var list = new List<string> { item0, item1 };

            Assert.That(list.TryGetAtIndex(0), Is.EqualTo(item0));
            Assert.That(list.TryGetAtIndex(1), Is.EqualTo(item1));
            Assert.That(list.TryGetAtIndex(2), Is.Null);
            Assert.That(list.TryGetAtIndex(-1), Is.Null);
        }

        [Test]
        public void TestGetAtIndexReverseForNullList()
        {
            var list = (List<string>)null;

            Assert.That(list.TryGetAtIndexReverse(0), Is.Null);
            Assert.That(list.TryGetAtIndexReverse(-1), Is.Null);
            Assert.That(list.TryGetAtIndexReverse(1), Is.Null);
        }

        [Test]
        public void TestGetAtIndexReverseForEmptyList()
        {
            var list = new List<string>();

            Assert.That(list.TryGetAtIndexReverse(1), Is.Null);
            Assert.That(list.TryGetAtIndexReverse(-1), Is.Null);
            Assert.That(list.TryGetAtIndexReverse(1), Is.Null);
        }

        [Test]
        public void TestGetAtIndexReverseForNonNullList()
        {
            const string item0 = "foo";
            const string item1 = "bar";
            var list = new List<string> { item0, item1 };

            Assert.That(list.TryGetAtIndexReverse(0), Is.EqualTo(item1));
            Assert.That(list.TryGetAtIndexReverse(1), Is.EqualTo(item0));
            Assert.That(list.TryGetAtIndexReverse(2), Is.Null);
            Assert.That(list.TryGetAtIndexReverse(-1), Is.Null);
        }
    }
}