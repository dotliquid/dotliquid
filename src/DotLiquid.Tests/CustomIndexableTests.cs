using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class CustomIndexableTests
    {
        #region Test classes
        internal class VirtualList : IIndexable, IEnumerable
        {
            internal VirtualList(params object[] items)
            {
                this.items = items;
            }

            private readonly object[] items;

            public object this[object key]
            {
                get
                {
                    long? index = key as long?;
                    if (!index.HasValue || index.Value < 0L || index.Value >= items.Length) {
                        throw new KeyNotFoundException();
                    }
                    return items[index.Value];
                }
            }

            public bool ContainsKey(object key)
            {
                long? index = key as long?;
                return index.HasValue && index.Value >= 0L && index.Value < items.Length;
            }

            public IEnumerator GetEnumerator()
            {
                return items.GetEnumerator();
            }
        }

        internal class CustomIndexable : IIndexable, ILiquidizable
        {
            public object this[object key]
            {
                get
                {
                    if (key == null) {
                        return "null";
                    }
                    return key.GetType() + " " + key.ToString();
                }
            }

            public bool ContainsKey(object key)
            {
                return true;
            }

            public object ToLiquid()
            {
                return this;
            }
        }
        #endregion

        [Test]
        public void TestVirtualListLoop()
        {
            string output = Template.Parse("{%for item in list%}{{ item }} {%endfor%}")
                .Render(Hash.FromAnonymousObject(new {list = new VirtualList(1L, "Second", 3L)}));
            Assert.AreEqual("1 Second 3 ", output);
        }

        [Test]
        public void TestVirtualListIndex()
        {
            string output = Template.Parse("1: {{ list[0] }}, 2: {{ list[1] }}, 3: {{ list[2] }}")
                .Render(Hash.FromAnonymousObject(new {list = new VirtualList(1L, "Second", 3L)}));
            Assert.AreEqual("1: 1, 2: Second, 3: 3", output);
        }

        [Test]
        public void TestCustomIndexableIntKeys()
        {
            string output = Template.Parse("1: {{container[0]}}, 2: {{container[1]}}")
                .Render(Hash.FromAnonymousObject(new {container = new CustomIndexable()}));
            Assert.AreEqual("1: System.Int64 0, 2: System.Int64 1", output);
        }

        [Test]
        public void TestCustomIndexableStringKeys() {
            string output = Template.Parse("abc: {{container.abc}}")
                .Render(Hash.FromAnonymousObject(new {container = new CustomIndexable()}));
            Assert.AreEqual("abc: System.String abc", output);
        }
    }
}
