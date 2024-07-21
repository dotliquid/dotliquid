using System;
using System.Collections.Generic;
using DotLiquid.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.False(NIL.SafeTypeInsensitiveEqual("nil"));
            ClassicAssert.False("nil".SafeTypeInsensitiveEqual(null));
            ClassicAssert.False("a string".SafeTypeInsensitiveEqual("A STRING")); // different case string equality

            // Equals
            ClassicAssert.True(NIL.SafeTypeInsensitiveEqual(null)); // null equalilty
            ClassicAssert.True("a string".SafeTypeInsensitiveEqual("a string")); // same type equality
            ClassicAssert.True(1.SafeTypeInsensitiveEqual("1")); // int to string equality
            ClassicAssert.True(Int64.Parse("99").SafeTypeInsensitiveEqual(Int32.Parse("99"))); // long to int equality
            ClassicAssert.True(2.0f.SafeTypeInsensitiveEqual("2.0"));  // float to string equality
            ClassicAssert.True(2.0d.SafeTypeInsensitiveEqual("2.0"));  // double to string equality
        }

        [Test]
        public void TestIsTruthy()
        {
            ClassicAssert.False(ObjectExtensionMethods.IsTruthy(null));
            ClassicAssert.False(NIL.IsTruthy());
            ClassicAssert.False(false.IsTruthy());
            ClassicAssert.False("false".IsTruthy());
            ClassicAssert.False("FALSE".IsTruthy());
            ClassicAssert.False("FaLSe".IsTruthy());

            ClassicAssert.True(true.IsTruthy());
            ClassicAssert.True("testing".IsTruthy());
            ClassicAssert.True("true".IsTruthy());
            ClassicAssert.True("TRUE".IsTruthy());
            ClassicAssert.True("TrUe".IsTruthy());
            ClassicAssert.True(0.IsTruthy());
            ClassicAssert.True(1.IsTruthy());
            ClassicAssert.True(9.9f.IsTruthy());
            ClassicAssert.True(new[] { "cat", "dog" }.IsTruthy());
            ClassicAssert.True(Array.Empty<object>().IsTruthy());
        }

        [Test]
        public void TestIsFalsy()
        {
            ClassicAssert.True(ObjectExtensionMethods.IsFalsy(null));
            ClassicAssert.True(NIL.IsFalsy());
            ClassicAssert.True(false.IsFalsy());
            ClassicAssert.True("false".IsFalsy());
            ClassicAssert.True("FALSE".IsFalsy());
            ClassicAssert.True("FaLSe".IsFalsy());

            ClassicAssert.False(true.IsFalsy());
            ClassicAssert.False("testing".IsFalsy());
            ClassicAssert.False("true".IsFalsy());
            ClassicAssert.False("TRUE".IsFalsy());
            ClassicAssert.False("TrUe".IsFalsy());
            ClassicAssert.False(0.IsFalsy());
            ClassicAssert.False(1.IsFalsy());
            ClassicAssert.False(9.9f.IsFalsy());
            ClassicAssert.False(new[] { "cat", "dog" }.IsFalsy());
            ClassicAssert.False(Array.Empty<object>().IsFalsy());
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

            ClassicAssert.IsNull(ObjectExtensionMethods.GetPropertyValue(null, "any"));
            ClassicAssert.IsNull(keyValuePair.GetPropertyValue("NonExistant"));
            ClassicAssert.IsNull(new KeyValuePair<string, object>("*key*", null).GetPropertyValue("Value"));

            ClassicAssert.AreEqual("*key*", keyValuePair.GetPropertyValue("Key"));
            ClassicAssert.AreEqual("*value*", keyValuePair.GetPropertyValue("Value"));
        }
    }
}