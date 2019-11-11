using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                    int? index = key as int?;
                    if (!index.HasValue || index.Value < 0 || index.Value >= items.Length) {
                        throw new KeyNotFoundException();
                    }
                    return items[index.Value];
                }
            }

            public bool ContainsKey(object key)
            {
                int? index = key as int?;
                return index.HasValue && index.Value >= 0 && index.Value < items.Length;
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
        public async Task TestVirtualListLoop()
        {
            string output = await Template.Parse("{%for item in list%}{{ item }} {%endfor%}")
                .RenderAsync(Hash.FromAnonymousObject(new {list = new VirtualList(1, "Second", 3)}));
            Assert.AreEqual("1 Second 3 ", output);
        }

        [Test]
        public async Task TestVirtualListIndex()
        {
            string output = await Template.Parse("1: {{ list[0] }}, 2: {{ list[1] }}, 3: {{ list[2] }}")
                .RenderAsync(Hash.FromAnonymousObject(new {list = new VirtualList(1, "Second", 3)}));
            Assert.AreEqual("1: 1, 2: Second, 3: 3", output);
        }

        [Test]
        public async Task TestCustomIndexableIntKeys()
        {
            string output = await Template.Parse("1: {{container[0]}}, 2: {{container[1]}}")
                .RenderAsync(Hash.FromAnonymousObject(new {container = new CustomIndexable()}));
            Assert.AreEqual("1: System.Int32 0, 2: System.Int32 1", output);
        }

        [Test]
        public async Task TestCustomIndexableStringKeys() {
            string output = await Template.Parse("abc: {{container.abc}}")
                .RenderAsync(Hash.FromAnonymousObject(new {container = new CustomIndexable()}));
            Assert.AreEqual("abc: System.String abc", output);
        }
    }
}
