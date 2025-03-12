using System;
using System.Collections.Generic;
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
            Assert.That(NIL.SafeTypeInsensitiveEqual("nil"), Is.False);
            Assert.That("nil".SafeTypeInsensitiveEqual(null), Is.False);
            Assert.That("a string".SafeTypeInsensitiveEqual("A STRING"), Is.False); // different case string equality

            // Equals
            Assert.That(NIL.SafeTypeInsensitiveEqual(null), Is.True); // null equalilty
            Assert.That("a string".SafeTypeInsensitiveEqual("a string"), Is.True); // same type equality
            Assert.That(1.SafeTypeInsensitiveEqual("1"), Is.True); // int to string equality
            Assert.That(Int64.Parse("99").SafeTypeInsensitiveEqual(Int32.Parse("99")), Is.True); // long to int equality
            Assert.That(2.0f.SafeTypeInsensitiveEqual("2.0"), Is.True);  // float to string equality
            Assert.That(2.0d.SafeTypeInsensitiveEqual("2.0"), Is.True);  // double to string equality
        }

        [Test]
        public void TestSafeTypeInsensitiveEqualArrays()
        {
            string[] array1 = { "a", "b", "c" };
            string[] array2 = { "a", "b", "c" };
            string[] array3 = { "a", "b", "d" };
            string[] array4 = { "A", "B", "C" };
            string[] array5 = { "b", "c", "a" };
            string[] array6 = { "a", "b", "c", "d" };
            string[] array7 = { };
            string[] array8 = { "1", "2", "3" };
            List<string> list1 = new List<string> { "a", "b", "c" };
            int[] intArray1 = { 1, 2, 3 };
            int[] intArray2 = { 1, 2, 3 };
            int[] intArray3 = { 1, 2, 3, 4, 5 };

            // Not equal
            Assert.That(array1.SafeTypeInsensitiveEqual(array3), Is.False);
            Assert.That(array1.SafeTypeInsensitiveEqual(array4), Is.False);
            Assert.That(array1.SafeTypeInsensitiveEqual(array5), Is.False);
            Assert.That(array1.SafeTypeInsensitiveEqual(array6), Is.False);
            Assert.That(array1.SafeTypeInsensitiveEqual(array7), Is.False);
            Assert.That(array1.SafeTypeInsensitiveEqual(intArray1), Is.False);
            Assert.That(array1.SafeTypeInsensitiveEqual("a,b,c"), Is.False);
            Assert.That(array1.SafeTypeInsensitiveEqual(null), Is.False);
            Assert.That(array3.SafeTypeInsensitiveEqual(array1), Is.False);
            Assert.That("abc".SafeTypeInsensitiveEqual(array1), Is.False);
            Assert.That(intArray1.SafeTypeInsensitiveEqual(intArray3), Is.False);
            Assert.That(intArray1.SafeTypeInsensitiveEqual(array1), Is.False);

            // Equals
            Assert.That(array1.SafeTypeInsensitiveEqual(array1), Is.True);
            Assert.That(array1.SafeTypeInsensitiveEqual(array2), Is.True);
            Assert.That(array2.SafeTypeInsensitiveEqual(array1), Is.True);
            Assert.That(array1.SafeTypeInsensitiveEqual(list1), Is.True);
            Assert.That(list1.SafeTypeInsensitiveEqual(array1), Is.True);
            Assert.That(intArray1.SafeTypeInsensitiveEqual(intArray1), Is.True);
            Assert.That(intArray1.SafeTypeInsensitiveEqual(intArray2), Is.True);
            Assert.That(intArray1.SafeTypeInsensitiveEqual(array8), Is.True);
        }

        [Test]
        public void TestIsTruthy()
        {
            Assert.That(ObjectExtensionMethods.IsTruthy(null), Is.False);
            Assert.That(NIL.IsTruthy(), Is.False);
            Assert.That(false.IsTruthy(), Is.False);
            Assert.That("false".IsTruthy(), Is.False);
            Assert.That("FALSE".IsTruthy(), Is.False);
            Assert.That("FaLSe".IsTruthy(), Is.False);

            Assert.That(true.IsTruthy(), Is.True);
            Assert.That("testing".IsTruthy(), Is.True);
            Assert.That("true".IsTruthy(), Is.True);
            Assert.That("TRUE".IsTruthy(), Is.True);
            Assert.That("TrUe".IsTruthy(), Is.True);
            Assert.That(0.IsTruthy(), Is.True);
            Assert.That(1.IsTruthy(), Is.True);
            Assert.That(9.9f.IsTruthy(), Is.True);
            Assert.That(new[] { "cat", "dog" }.IsTruthy(), Is.True);
            Assert.That(Array.Empty<object>().IsTruthy(), Is.True);
        }

        [Test]
        public void TestIsFalsy()
        {
            Assert.That(ObjectExtensionMethods.IsFalsy(null), Is.True);
            Assert.That(NIL.IsFalsy(), Is.True);
            Assert.That(false.IsFalsy(), Is.True);
            Assert.That("false".IsFalsy(), Is.True);
            Assert.That("FALSE".IsFalsy(), Is.True);
            Assert.That("FaLSe".IsFalsy(), Is.True);

            Assert.That(true.IsFalsy(), Is.False);
            Assert.That("testing".IsFalsy(), Is.False);
            Assert.That("true".IsFalsy(), Is.False);
            Assert.That("TRUE".IsFalsy(), Is.False);
            Assert.That("TrUe".IsFalsy(), Is.False);
            Assert.That(0.IsFalsy(), Is.False);
            Assert.That(1.IsFalsy(), Is.False);
            Assert.That(9.9f.IsFalsy(), Is.False);
            Assert.That(new[] { "cat", "dog" }.IsFalsy(), Is.False);
            Assert.That(Array.Empty<object>().IsFalsy(), Is.False);
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

            Assert.That(ObjectExtensionMethods.GetPropertyValue(null, "any"), Is.Null);
            Assert.That(keyValuePair.GetPropertyValue("NonExistant"), Is.Null);
            Assert.That(new KeyValuePair<string, object>("*key*", null).GetPropertyValue("Value"), Is.Null);

            Assert.That(keyValuePair.GetPropertyValue("Key"), Is.EqualTo("*key*"));
            Assert.That(keyValuePair.GetPropertyValue("Value"), Is.EqualTo("*value*"));
        }
    }
}