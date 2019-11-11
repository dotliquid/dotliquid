using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class DropTests
    {
        #region Classes used in tests

        internal class NullDrop : Drop
        {
            public override object BeforeMethod(string method)
            {
                return null;
            }
        }

        internal class ContextDrop : Drop
        {
            public int Scopes
            {
                get { return Context.Scopes.Count; }
            }

            public IEnumerable<int> ScopesAsArray
            {
                get { return Enumerable.Range(1, Context.Scopes.Count); }
            }

            public int LoopPos
            {
                get { return (int)Context["forloop.index"]; }
            }

            public void Break()
            {
                Debugger.Break();
            }

            public override object BeforeMethod(string method)
            {
                return Context[method];
            }
        }

        internal class ProductDrop : Drop
        {
            internal class TextDrop : Drop
            {
                public string[] Array
                {
                    get { return new[] { "text1", "text2" }; }
                }

                public string Text
                {
                    get { return "text1"; }
                }
            }

            internal class CatchallDrop : Drop
            {
                public override object BeforeMethod(string method)
                {
                    return "method: " + method;
                }
            }

            public TextDrop Texts()
            {
                return new TextDrop();
            }

            public CatchallDrop Catchall()
            {
                return new CatchallDrop();
            }

            public new ContextDrop Context
            {
                get { return new ContextDrop(); }
            }

            protected string CallMeNot()
            {
                return "protected";
            }
        }

        internal class EnumerableDrop : Drop, IEnumerable
        {
            public int Size
            {
                get { return 3; }
            }

            public IEnumerator GetEnumerator()
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }
        }

#if !CORE
        internal class DataRowDrop : Drop
        {
            private readonly System.Data.DataRow _dataRow;

            public DataRowDrop(System.Data.DataRow dataRow)
            {
                _dataRow = dataRow;
            }

            public override object BeforeMethod(string method)
            {
                if (_dataRow.Table.Columns.Contains(method))
                    return _dataRow[method];
                return null;
            }
        }
#endif

        internal class CamelCaseDrop : Drop
        {
            public int ProductID
            {
                get { return 1; }
            }
        }

        internal static class ProductFilter
        {
            public static string ProductText(object input)
            {
                return ((ProductDrop)input).Texts().Text;
            }
        }

        #endregion

        [Test]
        public async Task TestProductDrop()
        {
            Assert.DoesNotThrow(() =>
            {
                Template tpl = Template.Parse("  ");
                tpl.RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() })).GetAwaiter().GetResult();
            });
        }

        [Test]
        public async Task TestDropDoesNotOutputItself()
        {
            string output = await Template.Parse(" {{ product }} ")
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual("  ", output);
        }

        [Test]
        public async Task TestDropWithFilters()
        {
            string output = await Template.Parse(" {{ product | product_text }} ")
                .RenderAsync(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    LocalVariables = Hash.FromAnonymousObject(new { product = new ProductDrop() }),
                    Filters = new[] { typeof(ProductFilter) }
                });
            Assert.AreEqual(" text1 ", output);
        }

        [Test]
        public async Task TestTextDrop()
        {
            string output = await Template.Parse(" {{ product.texts.text }} ")
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual(" text1 ", output);
        }

        [Test]
        public async Task TestTextDrop2()
        {
            string output = await Template.Parse(" {{ product.catchall.unknown }} ")
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual(" method: unknown ", output);
        }

        [Test]
        public async Task TestTextArrayDrop()
        {
            string output = await Template.Parse("{% for text in product.texts.array %} {{text}} {% endfor %}")
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual(" text1  text2 ", output);
        }

        [Test]
        public async Task TestContextDrop()
        {
            string output = await Template.Parse(" {{ context.bar }} ")
                .RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), bar = "carrot" }));
            Assert.AreEqual(" carrot ", output);
        }

        [Test]
        public async Task TestNestedContextDrop()
        {
            string output = await Template.Parse(" {{ product.context.foo }} ")
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop(), foo = "monkey" }));
            Assert.AreEqual(" monkey ", output);
        }

        [Test]
        public async Task TestProtected()
        {
            string output = await Template.Parse(" {{ product.call_me_not }} ")
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual("  ", output);
        }

        [Test]
        public async Task TestScope()
        {
            Assert.AreEqual("1", await Template.Parse("{{ context.scopes }}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop() })));
            Assert.AreEqual("2", await Template.Parse("{%for i in dummy%}{{ context.scopes }}{%endfor%}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
            Assert.AreEqual("3", await Template.Parse("{%for i in dummy%}{%for i in dummy%}{{ context.scopes }}{%endfor%}{%endfor%}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
        }

        [Test]
        public async Task TestScopeThroughProc()
        {
            Assert.AreEqual("1", await Template.Parse("{{ s }}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]) })));
            Assert.AreEqual("2", await Template.Parse("{%for i in dummy%}{{ s }}{%endfor%}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]), dummy = new[] { 1 } })));
            Assert.AreEqual("3", await Template.Parse("{%for i in dummy%}{%for i in dummy%}{{ s }}{%endfor%}{%endfor%}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]), dummy = new[] { 1 } })));
        }

        [Test]
        public async Task TestScopeWithAssigns()
        {
            Assert.AreEqual("variable", await Template.Parse("{% assign a = 'variable'%}{{a}}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop() })));
            Assert.AreEqual("variable", await Template.Parse("{% assign a = 'variable'%}{%for i in dummy%}{{a}}{%endfor%}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
            Assert.AreEqual("test", await Template.Parse("{% assign header_gif = \"test\"%}{{header_gif}}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop() })));
            Assert.AreEqual("test", await Template.Parse("{% assign header_gif = 'test'%}{{header_gif}}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop() })));
        }

        [Test]
        public async Task TestScopeFromTags()
        {
            Assert.AreEqual("1", await Template.Parse("{% for i in context.scopes_as_array %}{{i}}{% endfor %}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
            Assert.AreEqual("12", await Template.Parse("{%for a in dummy%}{% for i in context.scopes_as_array %}{{i}}{% endfor %}{% endfor %}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
            Assert.AreEqual("123", await Template.Parse("{%for a in dummy%}{%for a in dummy%}{% for i in context.scopes_as_array %}{{i}}{% endfor %}{% endfor %}{% endfor %}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
        }

        [Test]
        public async Task TestAccessContextFromDrop()
        {
            Assert.AreEqual("123", await Template.Parse("{% for a in dummy %}{{ context.loop_pos }}{% endfor %}").RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1, 2, 3 } })));
        }

        [Test]
        public async Task TestEnumerableDrop()
        {
            Assert.AreEqual("123", await Template.Parse("{% for c in collection %}{{c}}{% endfor %}").RenderAsync(Hash.FromAnonymousObject(new { collection = new EnumerableDrop() })));
        }

        [Test]
        public async Task TestEnumerableDropSize()
        {
            Assert.AreEqual("3", await Template.Parse("{{collection.size}}").RenderAsync(Hash.FromAnonymousObject(new { collection = new EnumerableDrop() })));
        }

        [Test]
        public async Task TestNullCatchAll()
        {
            Assert.AreEqual("", await Template.Parse("{{ nulldrop.a_method }}").RenderAsync(Hash.FromAnonymousObject(new { nulldrop = new NullDrop() })));
        }

#if !CORE
        [Test]
        public async Task TestDataRowDrop()
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Column1");
            dataTable.Columns.Add("Column2");

            System.Data.DataRow dataRow = dataTable.NewRow();
            dataRow["Column1"] = "Hello";
            dataRow["Column2"] = "World";

            Template tpl = Template.Parse(" {{ row.column1 }} ");
            Assert.AreEqual(" Hello ", await tpl.RenderAsync(Hash.FromAnonymousObject(new { row = new DataRowDrop(dataRow) })));
        }
#endif

        [Test]
        public async Task TestRubyNamingConventionPrintsHelpfulErrorIfMissingPropertyWouldMatchCSharpNamingConvention()
        {
            INamingConvention savedNamingConvention = Template.NamingConvention;
            Template.NamingConvention = new RubyNamingConvention();
            Template template = Template.Parse("{{ value.ProductID }}");
            Assert.AreEqual("Missing property. Did you mean 'product_id'?", await template.RenderAsync(Hash.FromAnonymousObject(new
            {
                value = new CamelCaseDrop()
            })));
            Template.NamingConvention = savedNamingConvention;
        }
    }
}
