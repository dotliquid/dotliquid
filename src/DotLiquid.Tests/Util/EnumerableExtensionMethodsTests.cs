using System;
using System.Collections.Generic;

using DotLiquid.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.IsFalse(EnumerableExtensionMethods.Any(Array.Empty<string>()));
            ClassicAssert.IsTrue(EnumerableExtensionMethods.Any(new string[] { string.Empty }));
        }

        [Test]
        public void TestAnyHandlesIEnumerableGenerics()
        {
            var list = new List<string>();
            ClassicAssert.IsFalse(EnumerableExtensionMethods.Any(list));
            list.Add(string.Empty);
            ClassicAssert.IsTrue(EnumerableExtensionMethods.Any(list));
        }
    }
}
