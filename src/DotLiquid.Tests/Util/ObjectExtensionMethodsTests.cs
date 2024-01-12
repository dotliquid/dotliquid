using System;
using System.Collections.Generic;
using DotLiquid.NamingConventions;
using DotLiquid.Util;
using NUnit.Framework;

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
            Assert.False("a string".SafeTypeInsensitiveEqual("A STRING")); // different case string equality

            // Equals
            Assert.True(NIL.SafeTypeInsensitiveEqual(null)); // null equalilty
            Assert.True("a string".SafeTypeInsensitiveEqual("a string")); // same type equality
            Assert.True(1.SafeTypeInsensitiveEqual("1")); // int to string equality
            Assert.True(Int64.Parse("99").SafeTypeInsensitiveEqual(Int32.Parse("99"))); // long to int equality
            Assert.True(2.0f.SafeTypeInsensitiveEqual("2.0"));  // float to string equality
            Assert.True(2.0d.SafeTypeInsensitiveEqual("2.0"));  // double to string equality
        }

        [Test]
        public void TestIsTruthy()
        {
            Assert.False(ObjectExtensionMethods.IsTruthy(null));
            Assert.False(NIL.IsTruthy());
            Assert.False(false.IsTruthy());
            Assert.False("false".IsTruthy());
            Assert.False("FALSE".IsTruthy());
            Assert.False("FaLSe".IsTruthy());

            Assert.True(true.IsTruthy());
            Assert.True("testing".IsTruthy());
            Assert.True("true".IsTruthy());
            Assert.True("TRUE".IsTruthy());
            Assert.True("TrUe".IsTruthy());
            Assert.True(0.IsTruthy());
            Assert.True(1.IsTruthy());
            Assert.True(9.9f.IsTruthy());
            Assert.True(new[] { "cat", "dog" }.IsTruthy());
            Assert.True(Array.Empty<object>().IsTruthy());
        }

        [Test]
        public void TestIsFalsy()
        {
            Assert.True(ObjectExtensionMethods.IsFalsy(null));
            Assert.True(NIL.IsFalsy());
            Assert.True(false.IsFalsy());
            Assert.True("false".IsFalsy());
            Assert.True("FALSE".IsFalsy());
            Assert.True("FaLSe".IsFalsy());

            Assert.False(true.IsFalsy());
            Assert.False("testing".IsFalsy());
            Assert.False("true".IsFalsy());
            Assert.False("TRUE".IsFalsy());
            Assert.False("TrUe".IsFalsy());
            Assert.False(0.IsFalsy());
            Assert.False(1.IsFalsy());
            Assert.False(9.9f.IsFalsy());
            Assert.False(new[] { "cat", "dog" }.IsFalsy());
            Assert.False(Array.Empty<object>().IsFalsy());
        }

        [Test]
        public void TestIsTruthy_ShopifySample1()
        {
            Helper.AssertTemplateResult(
                expected: @"


  This text will always appear since ""name"" is defined.
",
                template: @"{% assign name = ""Tobi"" %}

{% if name %}
  This text will always appear since ""name"" is defined.
{% endif %}",
                localVariables: null);
        }

        [Test]
        public void TestIsTruthy_ShopifySample2()
        {
            var page = new
            {
                category = "" // exists, but empty
            };

            Helper.AssertTemplateResult(
            expected: @"
  <h1></h1>
",
            template: @"{% if page.category %}
  <h1>{{ page.category }}</h1>
{% endif %}",
            localVariables: Hash.FromAnonymousObject(new { page }));
        }

        [Test]
        public void TestGetPropertyValue()
        {
            var keyValuePair = new KeyValuePair<string, object>("*key*", "*value*");

            Assert.IsNull(ObjectExtensionMethods.GetPropertyValue(null, "any"));
            Assert.IsNull(keyValuePair.GetPropertyValue("NonExistant"));
            Assert.IsNull(new KeyValuePair<string, object>("*key*", null).GetPropertyValue("Value"));

            Assert.AreEqual("*key*", keyValuePair.GetPropertyValue("Key"));
            Assert.AreEqual("*value*", keyValuePair.GetPropertyValue("Value"));
        }
    }
}