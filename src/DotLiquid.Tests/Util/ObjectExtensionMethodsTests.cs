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
            Assert.That(NIL.SafeTypeInsensitiveEqual("nil"), Is.False);  // null to string equality
            Assert.That("nil".SafeTypeInsensitiveEqual(null), Is.False); // string to null equality
            Assert.That("a string".SafeTypeInsensitiveEqual("A STRING"), Is.False); // different case string equality

            Assert.That(true.SafeTypeInsensitiveEqual(false), Is.False); // bool to bool equality
            Assert.That(true.SafeTypeInsensitiveEqual("false"), Is.False); // bool to Falsy string equality
            Assert.That(true.SafeTypeInsensitiveEqual("true"), Is.False); // bool to Truthy string equality
            Assert.That("true".SafeTypeInsensitiveEqual(false), Is.False); // Truthy string to bool equality
            Assert.That("true".SafeTypeInsensitiveEqual(true), Is.False); // Truthy string to bool equality
            Assert.That("true".SafeTypeInsensitiveEqual("FALSE"), Is.False); // Truthy string to Falsy string equality

            Assert.That(false.SafeTypeInsensitiveEqual("true"), Is.False); // bool to Truthy string equality
            Assert.That("false".SafeTypeInsensitiveEqual(true), Is.False); // Falsy string to bool equality
            Assert.That("false".SafeTypeInsensitiveEqual("TRUE"), Is.False); // Falsy string to Truthy string equality

            Assert.That(false.SafeTypeInsensitiveEqual(0), Is.False); // bool to int(0) equality
            Assert.That("false".SafeTypeInsensitiveEqual(0), Is.False); // Falsy string to int(0) equality
            Assert.That(true.SafeTypeInsensitiveEqual(1), Is.False); // bool to int(1) equality
            Assert.That("true".SafeTypeInsensitiveEqual(1), Is.False); // Falsy string to int(1) equality

            Assert.That(1.SafeTypeInsensitiveEqual("1"), Is.False); // int to string equality
            Assert.That(2.0f.SafeTypeInsensitiveEqual("2.0"), Is.False);  // float to string equality
            Assert.That(2.0d.SafeTypeInsensitiveEqual("2.0"), Is.False);  // double to string equality

            Assert.That(true.SafeTypeInsensitiveEqual("true"), Is.False); // bool to Truthy string equality
            Assert.That("true".SafeTypeInsensitiveEqual(true), Is.False); // Truthy string to bool equality
            Assert.That("true".SafeTypeInsensitiveEqual("TRUE"), Is.False); // Truthy string to Truthy string equality

            Assert.That(false.SafeTypeInsensitiveEqual("false"), Is.False); // bool to Falsy string equality
            Assert.That("false".SafeTypeInsensitiveEqual(false), Is.False); // Falsy string to bool equality
            Assert.That("false".SafeTypeInsensitiveEqual(false), Is.False); // Falsy string to bool equality
            Assert.That("false".SafeTypeInsensitiveEqual("FALSE"), Is.False); // Falsy string to Falsy string equality

            // Equals
            Assert.That(NIL.SafeTypeInsensitiveEqual(null), Is.True); // null equalilty
            Assert.That("a string".SafeTypeInsensitiveEqual("a string"), Is.True); // same type equality
            Assert.That(Int64.Parse("99").SafeTypeInsensitiveEqual(Int32.Parse("99")), Is.True); // long to int equality
            Assert.That(true.SafeTypeInsensitiveEqual(true), Is.True); // bool to bool equality
            Assert.That(false.SafeTypeInsensitiveEqual(false), Is.True); // bool to bool equality
            Assert.That("A".SafeTypeInsensitiveEqual('A'), Is.True); // string to char equality
            Assert.That('A'.SafeTypeInsensitiveEqual("A"), Is.True); // char to string equality
            Assert.That(StringComparison.Ordinal.SafeTypeInsensitiveEqual("Ordinal"), Is.True); // enum to string equality
            Assert.That("Ordinal".SafeTypeInsensitiveEqual(StringComparison.Ordinal), Is.True); // string to enum equality
        }

        [Test]
        public void TestSafeTypeInsensitiveEqual_NumericTypes()
        {
            byte valueU8 = 12;
            ushort valueU16 = 12;
            uint valueU32 = 12;
            ulong valueU64 = 12;
            sbyte value8 = 12;
            short value16 = 12;
            int value32 = 12;
            long value64 = 12;
            decimal valueDecimal = 12;
            float valueFloat = 12;
            double valueDouble = 12;
            object[] equalValues = {
                valueU8,
                valueU16,
                valueU32,
                valueU64,
                value8,
                value16,
                value32,
                value64,
                valueDecimal,
                valueFloat,
                valueDouble
            };
            for (int i = 0; i < equalValues.Length; i++)
            {
                for (int j = 0; j < equalValues.Length; j++)
                {
                    Assert.That(equalValues[i].SafeTypeInsensitiveEqual(equalValues[j]), Is.True,
                        $"{equalValues[i].GetType()} != {equalValues[j].GetType()}");
                }
            }

            // Only include a single unsigned MinValue, as they are all zero
            object[] inequalValues = {
                // byte.MinValue,
                byte.MaxValue,
                // ushort.MinValue,
                ushort.MaxValue,
                // uint.MinValue,
                uint.MaxValue,
                ulong.MinValue,
                ulong.MaxValue,
                sbyte.MinValue,
                sbyte.MaxValue,
                short.MinValue,
                short.MaxValue,
                int.MinValue,
                int.MaxValue,
                long.MinValue,
                long.MaxValue,
                decimal.MinValue,
                decimal.MaxValue,
                float.MinValue,
                float.MaxValue,
                double.MinValue,
                double.MaxValue
            };

            for (int i = 0; i < inequalValues.Length; i++)
            {
                for (int j = 0; j < inequalValues.Length; j++)
                {
                    if (i != j)
                    {
                        Assert.That(inequalValues[i].SafeTypeInsensitiveEqual(inequalValues[j]), Is.False,
                            $"{inequalValues[i]}({inequalValues[i].GetType()}) == {inequalValues[j]}({inequalValues[j].GetType()})");
                    }
                }
            }
        }

        [Test]
        public void TestIsTruthy()
        {
            Assert.That(ObjectExtensionMethods.IsTruthy(null), Is.False);
            Assert.That(NIL.IsTruthy(), Is.False);
            Assert.That(false.IsTruthy(), Is.False);

            Assert.That("false".IsTruthy(), Is.True);
            Assert.That("FALSE".IsTruthy(), Is.True);
            Assert.That("FaLSe".IsTruthy(), Is.True);
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

            Assert.That("false".IsFalsy(), Is.False);
            Assert.That("FALSE".IsFalsy(), Is.False);
            Assert.That("FaLSe".IsFalsy(), Is.False);
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