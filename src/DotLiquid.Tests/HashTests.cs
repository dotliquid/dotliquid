using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace DotLiquid.Tests.Ns1
{
    public class TestClass
    {
        public string TestClassProp1 { get; set; }
    }
}

namespace DotLiquid.Tests.Ns2
{
    public class TestClass
    {
        public string TestClassProp2 { get; set; }
    }
}


namespace DotLiquid.Tests
{
    [TestFixture]
    public class HashTests
    {
        public class TestBaseClass
        {
            public string TestBaseClassProp { get; set; }

            public virtual string TestOverridableProp { get; set; }
        }

        public class TestMiddleClass : TestBaseClass
        {
            public string TestMiddleClassProp { get; set; }
        }

        public class TestChildClass : TestMiddleClass
        {
            public string TestClassProp { get; set; }

            public override string TestOverridableProp { get; set; }
        }

        #region Mapper Cache Tests
        /// <summary>
        /// "mapperCache" should consider namespace.
        /// Types with same name (but different namespace) should be cached separately
        /// </summary>
        [Test]
        public void TestMapperCacheShouldCacheSeperateNamespaces()
        {
            var testClass1 = new DotLiquid.Tests.Ns1.TestClass()
            {
                TestClassProp1 = "TestClassProp1Value"
            };

            var value1 = Hash.FromAnonymousObject(testClass1);

            Assert.That(
                value1[nameof(DotLiquid.Tests.Ns1.TestClass.TestClassProp1)], Is.EqualTo(testClass1.TestClassProp1));

            //Same type name but different namespace
            var testClass2 = new DotLiquid.Tests.Ns2.TestClass()
            {
                TestClassProp2 = "TestClassProp2Value"
            };
            var value2 = Hash.FromAnonymousObject(testClass2);

            Assert.That(
                value2[nameof(DotLiquid.Tests.Ns2.TestClass.TestClassProp2)], Is.EqualTo(testClass2.TestClassProp2));
        }

        #endregion

        #region Including Base Class Properties Tests

        private void IncludeBaseClassPropertiesOrNot(bool includeBaseClassProperties)
        {
            var TestClassOverridablePropValue = "TestClassOverridablePropValue";
            var TestClassPropValue = "TestClassPropValueValue";
            var TestMiddleClassPropValue = "TestMiddleClassPropValue";
            var TestBaseClassPropValue = "TestBaseClassPropValue";

            var value = Hash.FromAnonymousObject(new TestChildClass()
            {
                TestClassProp = TestClassPropValue,
                TestMiddleClassProp = TestMiddleClassPropValue,
                TestBaseClassProp = TestBaseClassPropValue,
                TestOverridableProp = TestClassOverridablePropValue
            }, includeBaseClassProperties);

            // Properties attached directly to the type of instance being converted to Hash should always be visible
            Assert.That(
                value[nameof(TestChildClass.TestClassProp)], Is.EqualTo(TestClassPropValue));

            Assert.That(
                value[nameof(TestChildClass.TestOverridableProp)], Is.EqualTo(TestClassOverridablePropValue));

            Assert.That(
                value[nameof(TestMiddleClass.TestMiddleClassProp)], Is.EqualTo(includeBaseClassProperties ? TestMiddleClassPropValue : null));

            Assert.That(
                value[nameof(TestChildClass.TestBaseClassProp)], Is.EqualTo(includeBaseClassProperties ? TestBaseClassPropValue : null));
        }

        /// <summary>
        /// Mapping without properties from base class
        /// </summary>
        [Test]
        public void TestShouldNotMapPropertiesFromBaseClass()
        {
            IncludeBaseClassPropertiesOrNot(includeBaseClassProperties: false);
        }

        /// <summary>
        /// Mapping with properties from base class
        /// </summary>
        [Test]
        public void TestShouldMapPropertiesFromBaseClass()
        {
            IncludeBaseClassPropertiesOrNot(includeBaseClassProperties: true);
        }

        /// <summary>
        /// Mapping/Not mapping properties from base class should work for same class.
        /// "mapperCache" should consider base class property mapping option ("includeBaseClassProperties").
        /// </summary>
        [Test]
        public void TestUpperTwoScenarioWithSameClass()
        {
            //These two need to be called together to be sure same cache is being used for two
            IncludeBaseClassPropertiesOrNot(false);
            IncludeBaseClassPropertiesOrNot(true);
        }
        #endregion

        [Test]
        public void TestDefaultValueConstructor()
        {
            var hash = new Hash(0); // default value of zero
            hash["key"] = "value";

            Assert.That(hash.Contains("unknown-key"), Is.True);
            Assert.That(hash.ContainsKey("unknown-key"), Is.True);
            Assert.That(hash["unknown-key"], Is.EqualTo(0)); // ensure the default value is returned

            Assert.That(hash.Contains("key"), Is.True);
            Assert.That(hash.ContainsKey("key"), Is.True);
            Assert.That(hash["key"], Is.EqualTo("value"));

            hash.Remove("key");
            Assert.That(hash.Contains("key"), Is.True);
            Assert.That(hash.ContainsKey("key"), Is.True);
            Assert.That(hash["key"], Is.EqualTo(0)); // ensure the default value is returned after key removed
        }

        [Test]
        public void TestLambdaConstructor()
        {
            var hash = new Hash((h, k) => { return "Lambda Value"; });
            hash["key"] = "value";

            Assert.That(hash.Contains("unknown-key"), Is.True);
            Assert.That(hash.ContainsKey("unknown-key"), Is.True);
            Assert.That(hash["unknown-key"], Is.EqualTo("Lambda Value"));

            Assert.That(hash.Contains("key"), Is.True);
            Assert.That(hash.ContainsKey("key"), Is.True);
            Assert.That(hash["key"], Is.EqualTo("value"));
        }

        [Test]
        public void TestUnsupportedKeyType()
        {
            IDictionary hash = new Hash();
            Assert.Throws<System.NotSupportedException>(() =>
            {
                _ = hash[0]; // Only a string key is permitted.
            });
        }

        [Test]
        public void TestMergeNestedDictionaries()
        {
            var hash = Hash.FromDictionary(new Dictionary<string, object> {{
                    "People",
                    new Dictionary<string, object> {
                            { "ID1", new Dictionary<string, object>{ { "First", "Jane" }, { "Last", "Green" } } },
                            { "ID2", new Dictionary<string, object>{ { "First", "Mike" }, { "Last", "Doe" } } }
                        }
                    }});

            // Test using a for loop
            Helper.AssertTemplateResult(
                expected: "JaneMike",
                template: "{% for item in People %}{{ item.First }}{%endfor%}",
                localVariables: hash);

            // Test using direct variable access
            Helper.AssertTemplateResult(
                expected: "Jane Doe",
                template: "{{ People.ID1.First }} {{ People.ID2.Last }}",
                localVariables: hash);
        }

        [Test]
        public void TestFromAnonymousObjectAcceptsNull()
        {
            var hash = Hash.FromAnonymousObject(null);
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash.Count, Is.EqualTo(0));

            Assert.That(hash.Contains("unknown-key"), Is.False);
            Assert.That(hash.ContainsKey("unknown-key"), Is.False);
            Assert.That(hash["unknown-key"], Is.Null);
        }

        [Test]
        public void TestHashIDictionaryGenericsInterfaceAccess()
        {
            var zeroPair = new KeyValuePair<string, object>("Zero", "0");
            IDictionary<string, object> hash = Hash.FromDictionary(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { zeroPair.Key, zeroPair.Value } });
            var upperKey = zeroPair.Key;
            var lowerKey = upperKey.ToLower();

            Assert.Multiple(() =>
            {
                Assert.That(hash.Count, Is.EqualTo(1));
                Assert.That(hash.Keys, Is.EqualTo(new[] { upperKey }).AsCollection);
                Assert.That(hash.Values, Is.EqualTo(new[] { "0" }).AsCollection);
                Assert.That(hash[upperKey], Is.EqualTo("0"));
                Assert.That(hash[lowerKey], Is.EqualTo("0"));
                Assert.That(hash.Contains(zeroPair), Is.True);
                Assert.That(hash.Contains(new KeyValuePair<string, object>("One", "1")), Is.False);

                var array = new KeyValuePair<string, object>[1];
                hash.CopyTo(array, 0);
                Assert.That(array[0].Key, Is.EqualTo(zeroPair.Key));
                Assert.That(array[0].Value, Is.EqualTo(zeroPair.Value));
            });
        }

        [Test]
        public void TestHashIDictionaryGenericsInterfaceEnumerator()
        {
            var dictionary = new Dictionary<string, object>() { { "Zero", "0" }, { "One", 1 } };
            IDictionary<string, object> hash = Hash.FromDictionary(dictionary);
            var enumerator = hash.GetEnumerator();
            var actualKeys = new List<string>();
            var actualValues = new List<object>();

            while (enumerator.MoveNext())
            {
                actualKeys.Add(enumerator.Current.Key);
                actualValues.Add(enumerator.Current.Value);
            }

            Assert.That(actualKeys, Is.EquivalentTo(dictionary.Keys));
            Assert.That(actualValues, Is.EquivalentTo(dictionary.Values));
        }

        [Test]
        public void TestHashIDictionaryGenericsInterfaceManipulation()
        {
            var zeroPair = new KeyValuePair<string, object>("Zero", "0");
            var onePair = new KeyValuePair<string, object>("One", 1);
            IDictionary<string, object> hash = Hash.FromDictionary(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { zeroPair.Key, zeroPair.Value } });

            Assert.Multiple(() =>
            {
                hash.Add(onePair);
                Assert.That(hash.Count, Is.EqualTo(2));
                Assert.That(hash.Contains(onePair), Is.True);
                Assert.That(hash[onePair.Key], Is.EqualTo(1));
                hash.Remove(onePair);
                Assert.That(hash.Count, Is.EqualTo(1));

                hash.Clear();
                Assert.That(hash.Count, Is.EqualTo(0));
                Assert.That(hash, Is.Empty);
            });
        }

        [Test]
        public void TestHashIDictionaryInterfaceAccess()
        {
            IDictionary hash = Hash.FromDictionary(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Zero", "0" } });
            var upperKey = "Zero";
            object lowerKey = upperKey.ToLower();

            Assert.Multiple(() =>
            {
                Assert.That(hash.Count, Is.EqualTo(1));
                Assert.That(hash.Keys, Is.EqualTo(new[] { upperKey }).AsCollection);
                Assert.That(hash.Values, Is.EqualTo(new[] { "0" }).AsCollection);
                Assert.That(hash[upperKey], Is.EqualTo("0"));
                Assert.That(hash[lowerKey], Is.EqualTo("0"));
                Assert.That(hash.Contains(upperKey), Is.True);
                Assert.That(hash.Contains(lowerKey), Is.True);
                Assert.That(hash.Contains("One"), Is.False);

                var array = new KeyValuePair<string, object>[1];
                hash.CopyTo(array, 0);
                Assert.That(array[0].Key, Is.EqualTo(upperKey));
                Assert.That(array[0].Value, Is.EqualTo("0"));
            });
        }

        [Test]
        public void TestHashIDictionaryInterfaceEnumerator()
        {
            var dictionary = new Dictionary<string, object>() { { "Zero", "0" }, { "One", 1 } };
            IDictionary hash = Hash.FromDictionary(dictionary);
            var enumerator = hash.GetEnumerator();
            var actualKeys = new List<object>();
            var actualValues = new List<object>();

            while (enumerator.MoveNext())
            {
                actualKeys.Add(enumerator.Key);
                actualValues.Add(enumerator.Value);
            }

            Assert.That(actualKeys, Is.EquivalentTo(dictionary.Keys));
            Assert.That(actualValues, Is.EquivalentTo(dictionary.Values));
        }

        [Test]
        public void TestHashIDictionaryInterfaceManipulation()
        {
            IDictionary hash = Hash.FromDictionary(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Zero", "0" } });
            var oneKey = "One";

            Assert.Multiple(() =>
            {
                hash.Add(oneKey, 1);
                Assert.That(hash.Count, Is.EqualTo(2));
                Assert.That(hash.Contains(oneKey), Is.True);
                Assert.That(hash[oneKey], Is.EqualTo(1));
                hash.Remove(oneKey);
                Assert.That(hash.Count, Is.EqualTo(1));

                hash.Clear();
                Assert.That(hash.Count, Is.EqualTo(0));
                Assert.That(hash, Is.Empty);
            });
        }

        [Test]
        public void TestHashIDictionaryInterfaceOther()
        {
            IDictionary hash = new Hash();

            Assert.Multiple(() =>
            {
                Assert.That(hash.IsFixedSize, Is.False);
                Assert.That(hash.IsReadOnly, Is.False);
                Assert.That(hash.IsSynchronized, Is.False);
                Assert.That(hash.SyncRoot, Is.Not.Null);
            });
        }

        [Test]
        public void TestHashIIndexableInterface()
        {
            IIndexable hash = Hash.FromDictionary(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "Zero", "0" } });
            var upperKey = "Zero";
            object lowerKey = upperKey.ToLower();

            Assert.Multiple(() =>
            {
                Assert.That(hash[upperKey], Is.EqualTo("0"));
                Assert.That(hash[lowerKey], Is.EqualTo("0"));
                Assert.That(hash.ContainsKey(upperKey), Is.True);
                Assert.That(hash.ContainsKey(lowerKey), Is.True);
                Assert.That(hash.ContainsKey("one"), Is.False);
            });
        }
    }
}
