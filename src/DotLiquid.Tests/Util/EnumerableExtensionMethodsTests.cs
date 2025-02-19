using System;
using System.Collections.Generic;

using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class EnumerableExtensionMethodsTests
    {
        [Test]
        public void TestAnyNullSourceThrows()
        {
            Assert.Throws<ArgumentNullException>(() => EnumerableExtensionMethods.Any(null));
        }


        [Test]
        public void TestAnyHandlesIEnumerable()
        {
            Assert.That(EnumerableExtensionMethods.Any(Array.Empty<string>()), Is.False);
            Assert.That(EnumerableExtensionMethods.Any(new string[] { string.Empty }), Is.True);
        }

        [Test]
        public void TestAnyHandlesIEnumerableGenerics()
        {
            var list = new List<string>();
            Assert.That(EnumerableExtensionMethods.Any(list), Is.False);
            list.Add(string.Empty);
            Assert.That(EnumerableExtensionMethods.Any(list), Is.True);
        }
    }
}
