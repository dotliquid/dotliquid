using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class LazyHashTest
    {

        public class LazyHash : Hash
        {
            #region Fields

            private Lazy<Dictionary<string, PropertyInfo>> lazyProperties = null;
            private Dictionary<string, PropertyInfo> PropertyInfos => lazyProperties.Value;


            private object ObjectWithLazyProperty { get; set; }

            #endregion

            #region Constructors
            
            public LazyHash(object bo)
            {
                ObjectWithLazyProperty = bo;
                Initialize(bo);
            }
            
            private void Initialize(object bo)
            {
                lazyProperties = new Lazy<Dictionary<string, PropertyInfo>>(delegate ()
                {
                    var boProperties = new Dictionary<string, PropertyInfo>();
                    foreach (var pi in bo.GetType().GetProperties())
                    {
                        if (!boProperties.ContainsKey(pi.Name.ToLower()))
                        {
                            boProperties.Add(pi.Name.ToLower(), pi);
                        }
                    }
                    return boProperties;
                });

            }
            
            #endregion

            protected override object GetValue(string key)
            {
                if (PropertyInfos.ContainsKey(key.ToLower()))
                {
                    return PropertyInfos[key.ToLower()].GetValue(ObjectWithLazyProperty, null);
                }
                return base.GetValue(key);
            }
            
            public override bool Contains(object key)
            {
                var dicKey = key.ToString().ToLower();
                if (PropertyInfos.ContainsKey(dicKey))
                    return true;
                return base.Contains(key);
            }
        }



        public class TestLazyObject {
            public Lazy<string> _lazyProperty1 => new Lazy<string>(() =>
            {
                return "LAZY_PROPERTY_1";
            });
            public string LazyProperty1 => _lazyProperty1.Value;

            public Lazy<string> _lazyProperty2 => new Lazy<string>(() =>
            {
                return "LAZY_PROPERTY_2";
            });
            public string LazyProperty2 => _lazyProperty2.Value;

            public string StaticProperty => "STATIC_PROPERTY";
        }

        [Test]
        public void TestLazyHashProperty1WithoutAccessingProperty2()
        {
            var lazyObject = new TestLazyObject();
            Template template = Template.Parse("{{LazyProperty1}}");
            var output = template.Render(new LazyHash(lazyObject));
            Assert.AreEqual("LAZY_PROPERTY_1", output);
            Assert.IsFalse(lazyObject._lazyProperty2.IsValueCreated, "LazyObject LAZY_PROPERTY_2 has been created");
        }

        [Test]
        public void TestLazyHashProperty2WithoutAccessingProperty1()
        {
            var lazyObject = new TestLazyObject();
            Template template = Template.Parse("{{LazyProperty2}}");
            var output = template.Render(new LazyHash(lazyObject));
            Assert.AreEqual("LAZY_PROPERTY_2", output);
            Assert.IsFalse(lazyObject._lazyProperty1.IsValueCreated, "LazyObject LAZY_PROPERTY_1 has been created");
        }

        [Test]
        public void TestLazyHashWithoutAccessingAny()
        {
            var lazyObject = new TestLazyObject();
            Template template = Template.Parse("{{StaticProperty}}");
            var output = template.Render(new LazyHash(lazyObject));
            Assert.AreEqual("STATIC_PROPERTY", output);
            Assert.IsFalse(lazyObject._lazyProperty1.IsValueCreated, "LazyObject LAZY_PROPERTY_1 has been created");
            Assert.IsFalse(lazyObject._lazyProperty2.IsValueCreated, "LazyObject LAZY_PROPERTY_2 has been created");
        }

        [Test]
        public void TestLazyHashWithAccessingAllProperties()
        {
            var lazyObject = new TestLazyObject();
            Template template = Template.Parse("{{LazyProperty1}}-{{LazyProperty2}}-{{StaticProperty}}");
            var output = template.Render(new LazyHash(lazyObject));
            Assert.AreEqual($"LAZY_PROPERTY_1-LAZY_PROPERTY_2-STATIC_PROPERTY", output);
        }
    }
}
