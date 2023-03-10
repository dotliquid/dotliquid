using DotLiquid.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    class EnumerableExtensionMethodsTests
    {
        [Test]
        public void TestAnyNullSourceThrows()
        {
            Assert.Throws<ArgumentNullException>(() => EnumerableExtensionMethods.Any(null));
        }


        [Test]
        public void TestAnyHandlesIEnumerable()
        {
            Assert.IsFalse(EnumerableExtensionMethods.Any(Array.Empty<string>()));
            Assert.IsTrue(EnumerableExtensionMethods.Any(new string[] { string.Empty }));
        }

        [Test]
        public void TestAnyHandlesIEnumerableGenerics()
        {
            var list = new List<string>();
            Assert.IsFalse(EnumerableExtensionMethods.Any(list));
            list.Add(string.Empty);
            Assert.IsTrue(EnumerableExtensionMethods.Any(list));
        }
    }
}
