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
        public void TestConstructor()
        {
            var hash = new Hash();

            Assert.That(hash.Contains("unknown-key"), Is.False);
            Assert.That(hash.ContainsKey("unknown-key"), Is.False);
            Assert.That(hash["unknown-key"], Is.Null);
        }

        [Test]
        public void TestDefaultValueConstructor()
        {
            int defaultValue = 12;
            var hash = new Hash(defaultValue);
            hash["key"] = "value";

            // ensure the default value is returned for unknown keys
            Assert.That(hash.Contains("unknown-key"), Is.True);
            Assert.That(hash.ContainsKey("unknown-key"), Is.True);
            Assert.That(hash["unknown-key"], Is.EqualTo(defaultValue));

            Assert.That(hash.Contains("key"), Is.True);
            Assert.That(hash.ContainsKey("key"), Is.True);
            Assert.That(hash["key"], Is.EqualTo("value"));

            // ensure the default value is returned after key removed
            hash.Remove("key");
            Assert.That(hash.Contains("key"), Is.True);
            Assert.That(hash.ContainsKey("key"), Is.True);
            Assert.That(hash["key"], Is.EqualTo(defaultValue));
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
                var value = hash[0]; // Only a string key is permitted.
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
        public void TestFromAnonymousObject_Null()
        {
            var hash = Hash.FromAnonymousObject(null);
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash.Count, Is.EqualTo(0));

            Assert.That(hash.Contains("unknown-key"), Is.False);
            Assert.That(hash.ContainsKey("unknown-key"), Is.False);
            Assert.That(hash["unknown-key"], Is.Null);
        }

        #region IDictionary<string, object> Tests

        [Test]
        public void TestIDictionaryStringObject_Add()
        {
            IDictionary<string, object> hash = new Hash();
            hash.Add("Key1", "Value1");

            Assert.That(hash.ContainsKey("Key1"), Is.True);
            Assert.That(hash["Key1"], Is.EqualTo("Value1"));
        }

        [Test]
        public void TestIDictionaryStringObject_ContainsKey()
        {
            IDictionary<string, object> hash = new Hash();
            hash.Add("Key1", "Value1");

            Assert.That(hash.ContainsKey("Key1"), Is.True);
            Assert.That(hash.ContainsKey("Key2"), Is.False);
        }

        [Test]
        public void TestIDictionaryStringObject_Remove()
        {
            IDictionary<string, object> hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Remove("Key1");

            Assert.That(hash.ContainsKey("Key1"), Is.False);
        }

        [Test]
        public void TestIDictionaryStringObject_TryGetValue()
        {
            IDictionary<string, object> hash = new Hash();
            hash.Add("Key1", "Value1");

            object value;
            bool result = hash.TryGetValue("Key1", out value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo("Value1"));

            result = hash.TryGetValue("Key2", out value);

            Assert.That(result, Is.False);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void TestIDictionaryStringObject_Keys()
        {
            IDictionary<string, object> hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Add("Key2", "Value2");

            var keys = hash.Keys;

            Assert.That(keys, Contains.Item("Key1"));
            Assert.That(keys, Contains.Item("Key2"));
        }

        [Test]
        public void TestIDictionaryStringObject_Values()
        {
            IDictionary<string, object> hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Add("Key2", "Value2");

            var values = hash.Values;

            Assert.That(values, Contains.Item("Value1"));
            Assert.That(values, Contains.Item("Value2"));
        }

        [Test]
        public void TestIDictionaryStringObject_Clear()
        {
            IDictionary<string, object> hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Add("Key2", "Value2");

            hash.Clear();

            Assert.That(hash.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestIDictionaryStringObject_Enumerator()
        {
            IDictionary<string, object> hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Add("Key2", "Value2");

            var enumerator = hash.GetEnumerator();
            var keys = new List<string>();
            var values = new List<object>();

            while (enumerator.MoveNext())
            {
                keys.Add(enumerator.Current.Key);
                values.Add(enumerator.Current.Value);
            }

            Assert.That(keys, Contains.Item("Key1"));
            Assert.That(keys, Contains.Item("Key2"));
            Assert.That(values, Contains.Item("Value1"));
            Assert.That(values, Contains.Item("Value2"));
        }

        #endregion

        #region IEnumerable<KeyValuePair<string, object>> Tests

        [Test]
        public void TestIEnumerableKeyValuePairStringObject_GetEnumerator()
        {
            IEnumerable<KeyValuePair<string, object>> hash = new Hash();
            ((IDictionary<string, object>)hash).Add("Key1", "Value1");
            ((IDictionary<string, object>)hash).Add("Key2", "Value2");

            var enumerator = hash.GetEnumerator();
            var keys = new List<string>();
            var values = new List<object>();

            while (enumerator.MoveNext())
            {
                keys.Add(enumerator.Current.Key);
                values.Add(enumerator.Current.Value);
            }

            Assert.That(keys, Contains.Item("Key1"));
            Assert.That(keys, Contains.Item("Key2"));
            Assert.That(values, Contains.Item("Value1"));
            Assert.That(values, Contains.Item("Value2"));
        }

        #endregion

        #region IEnumerable Tests

        [Test]
        public void TestIEnumerable_GetEnumerator()
        {
            IEnumerable hash = new Hash();
            ((IDictionary<string, object>)hash).Add("Key1", "Value1");
            ((IDictionary<string, object>)hash).Add("Key2", "Value2");

            var enumerator = hash.GetEnumerator();
            var keys = new List<string>();
            var values = new List<object>();

            while (enumerator.MoveNext())
            {
                var entry = (KeyValuePair<string, object>)enumerator.Current;
                keys.Add(entry.Key);
                values.Add(entry.Value);
            }

            Assert.That(keys, Contains.Item("Key1"));
            Assert.That(keys, Contains.Item("Key2"));
            Assert.That(values, Contains.Item("Value1"));
            Assert.That(values, Contains.Item("Value2"));
        }

        #endregion

        #region ICollection<KeyValuePair<string, object>> Tests

        [Test]
        public void TestICollectionKeyValuePairStringObject_Add()
        {
            ICollection<KeyValuePair<string, object>> hash = new Hash();
            hash.Add(new KeyValuePair<string, object>("Key1", "Value1"));

            Assert.That(((IDictionary<string, object>)hash).ContainsKey("Key1"), Is.True);
            Assert.That(((IDictionary<string, object>)hash)["Key1"], Is.EqualTo("Value1"));
        }

        [Test]
        public void TestICollectionKeyValuePairStringObject_Contains()
        {
            ICollection<KeyValuePair<string, object>> hash = new Hash();
            var kvp = new KeyValuePair<string, object>("Key1", "Value1");
            hash.Add(kvp);

            Assert.That(hash.Contains(kvp), Is.True);
        }

        [Test]
        public void TestICollectionKeyValuePairStringObject_CopyTo()
        {
            ICollection<KeyValuePair<string, object>> hash = new Hash();
            hash.Add(new KeyValuePair<string, object>("Key1", "Value1"));
            hash.Add(new KeyValuePair<string, object>("Key2", "Value2"));

            var array = new KeyValuePair<string, object>[2];
            hash.CopyTo(array, 0);

            Assert.That(array[0].Key, Is.EqualTo("Key1"));
            Assert.That(array[0].Value, Is.EqualTo("Value1"));
            Assert.That(array[1].Key, Is.EqualTo("Key2"));
            Assert.That(array[1].Value, Is.EqualTo("Value2"));
        }

        [Test]
        public void TestICollectionKeyValuePairStringObject_Remove()
        {
            ICollection<KeyValuePair<string, object>> hash = new Hash();
            var kvp = new KeyValuePair<string, object>("Key1", "Value1");
            hash.Add(kvp);
            hash.Remove(kvp);

            Assert.That(((IDictionary<string, object>)hash).ContainsKey("Key1"), Is.False);
        }

        [Test]
        public void TestICollectionKeyValuePairStringObject_Count()
        {
            ICollection<KeyValuePair<string, object>> hash = new Hash();
            hash.Add(new KeyValuePair<string, object>("Key1", "Value1"));
            hash.Add(new KeyValuePair<string, object>("Key2", "Value2"));

            Assert.That(hash.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestICollectionKeyValuePairStringObject_IsReadOnly()
        {
            ICollection<KeyValuePair<string, object>> hash = new Hash();
            Assert.That(hash.IsReadOnly, Is.False);
        }

        #endregion

        #region ICollection Tests

        [Test]
        public void TestICollection_CopyTo()
        {
            ICollection hash = new Hash();
            ((IDictionary<string, object>)hash).Add("Key1", "Value1");
            ((IDictionary<string, object>)hash).Add("Key2", "Value2");

            var array = new KeyValuePair<string, object>[2];
            hash.CopyTo(array, 0);

            Assert.That(array[0].Key, Is.EqualTo("Key1"));
            Assert.That(array[0].Value, Is.EqualTo("Value1"));
            Assert.That(array[1].Key, Is.EqualTo("Key2"));
            Assert.That(array[1].Value, Is.EqualTo("Value2"));
        }

        [Test]
        public void TestICollection_Count()
        {
            ICollection hash = new Hash();
            ((IDictionary<string, object>)hash).Add("Key1", "Value1");
            ((IDictionary<string, object>)hash).Add("Key2", "Value2");

            Assert.That(hash.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestICollection_IsSynchronized()
        {
            ICollection hash = new Hash();
            Assert.That(hash.IsSynchronized, Is.False);
        }

        [Test]
        public void TestICollection_SyncRoot()
        {
            ICollection hash = new Hash();
            Assert.That(hash.SyncRoot, Is.Not.Null);
        }

        #endregion

        #region IDictionary Tests

        [Test]
        public void TestIDictionary_Add()
        {
            IDictionary hash = new Hash();
            hash.Add("Key1", "Value1");

            Assert.That(hash.Contains("Key1"), Is.True);
            Assert.That(hash["Key1"], Is.EqualTo("Value1"));
        }

        [Test]
        public void TestIDictionary_Contains()
        {
            IDictionary hash = new Hash();
            hash.Add("Key1", "Value1");

            Assert.That(hash.Contains("Key1"), Is.True);
            Assert.That(hash.Contains("Key2"), Is.False);
        }

        [Test]
        public void TestIDictionary_Remove()
        {
            IDictionary hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Remove("Key1");

            Assert.That(hash.Contains("Key1"), Is.False);
        }

        [Test]
        public void TestIDictionary_Keys()
        {
            IDictionary hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Add("Key2", "Value2");

            var keys = hash.Keys;

            Assert.That(keys, Contains.Item("Key1"));
            Assert.That(keys, Contains.Item("Key2"));
        }

        [Test]
        public void TestIDictionary_Values()
        {
            IDictionary hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Add("Key2", "Value2");

            var values = hash.Values;

            Assert.That(values, Contains.Item("Value1"));
            Assert.That(values, Contains.Item("Value2"));
        }

        [Test]
        public void TestIDictionary_Clear()
        {
            IDictionary hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Add("Key2", "Value2");

            hash.Clear();

            Assert.That(hash.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestIDictionary_Enumerator()
        {
            IDictionary hash = new Hash();
            hash.Add("Key1", "Value1");
            hash.Add("Key2", "Value2");

            IDictionaryEnumerator enumerator = hash.GetEnumerator();
            var keys = new List<object>();
            var values = new List<object>();

            while (enumerator.MoveNext())
            {
                keys.Add(enumerator.Key);
                values.Add(enumerator.Value);
            }

            Assert.That(keys, Contains.Item("Key1"));
            Assert.That(keys, Contains.Item("Key2"));
            Assert.That(values, Contains.Item("Value1"));
            Assert.That(values, Contains.Item("Value2"));
        }

        #endregion
    }
}
