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

            Assert.AreEqual(
                testClass1.TestClassProp1,
                value1[nameof(DotLiquid.Tests.Ns1.TestClass.TestClassProp1)]);

            //Same type name but different namespace
            var testClass2 = new DotLiquid.Tests.Ns2.TestClass()
            {
                TestClassProp2 = "TestClassProp2Value"
            };
            var value2 = Hash.FromAnonymousObject(testClass2);

            Assert.AreEqual(
                testClass2.TestClassProp2,
                value2[nameof(DotLiquid.Tests.Ns2.TestClass.TestClassProp2)]);
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
            Assert.AreEqual(
                TestClassPropValue,
                value[nameof(TestChildClass.TestClassProp)]);

            Assert.AreEqual(
                TestClassOverridablePropValue,
                value[nameof(TestChildClass.TestOverridableProp)]);

            Assert.AreEqual(
                includeBaseClassProperties ? TestMiddleClassPropValue : null,
                value[nameof(TestMiddleClass.TestMiddleClassProp)]);

            Assert.AreEqual(
                includeBaseClassProperties ? TestBaseClassPropValue : null,
                value[nameof(TestChildClass.TestBaseClassProp)]);
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

            Assert.True(hash.Contains("unknown-key"));
            Assert.True(hash.ContainsKey("unknown-key"));
            Assert.AreEqual(0, hash["unknown-key"]); // ensure the default value is returned

            Assert.True(hash.Contains("key"));
            Assert.True(hash.ContainsKey("key"));
            Assert.AreEqual("value", hash["key"]);

            hash.Remove("key");
            Assert.True(hash.Contains("key"));
            Assert.True(hash.ContainsKey("key"));
            Assert.AreEqual(0, hash["key"]); // ensure the default value is returned after key removed
        }

        [Test]
        public void TestLambdaConstructor()
        {
            var hash = new Hash((h, k) => { return "Lambda Value"; });
            hash["key"] = "value";

            Assert.True(hash.Contains("unknown-key"));
            Assert.True(hash.ContainsKey("unknown-key"));
            Assert.AreEqual("Lambda Value", hash["unknown-key"]);

            Assert.True(hash.Contains("key"));
            Assert.True(hash.ContainsKey("key"));
            Assert.AreEqual("value", hash["key"]);
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
    }
}
