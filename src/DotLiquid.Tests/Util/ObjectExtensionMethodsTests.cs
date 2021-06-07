using System.Collections.Generic;

using DotLiquid.Util;
using NUnit.Framework;
using System;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class ObjectExtensionMethodsTests
    {
        private static readonly object NIL = null;

        [Test]
        public void TestSafeTypeInsensitiveEqual()
        {
            // Not equal
            Assert.False(NIL.SafeTypeInsensitiveEqual("nil"));
            Assert.False("nil".SafeTypeInsensitiveEqual(null));
            Assert.False("a string".SafeTypeInsensitiveEqual("A STRING")); // diffreent case string equality

            // Equals
            Assert.True(NIL.SafeTypeInsensitiveEqual(null)); // null equalilty
            Assert.True("a string".SafeTypeInsensitiveEqual("a string")); // same type equality
            Assert.True(1.SafeTypeInsensitiveEqual("1")); // int to string equality
            Assert.True(Int64.Parse("99").SafeTypeInsensitiveEqual(Int32.Parse("99"))); // long to int equality
            Assert.True(2.0f.SafeTypeInsensitiveEqual("2.0"));  // float to string equality
            Assert.True(2.0d.SafeTypeInsensitiveEqual("2.0"));  // double to string equality
        }
    }
}