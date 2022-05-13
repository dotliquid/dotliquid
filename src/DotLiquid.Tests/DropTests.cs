using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
            internal class ComplexDrop : Drop
            {
                public TextDrop[] ArrayOfDrops
                {
                    get { return new[] { new TextDrop(), new TextDrop()}; }
                }

                public TextDrop SingleDrop
                {
                    get { return new TextDrop(); }
                }
            }

            internal class TextDrop : Drop
            {
                public string[] Array
                {
                    get { return new[] { "text1", "text2" }; }
                }

                public List<string> List
                {
                    get { return new List<string>(new[] { "text1", "text2" }); }
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

            public ComplexDrop Complex()
            {
                return new ComplexDrop();
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

        internal class ConflictingParentDrop : Drop
        {
            public string Name { get; set; } = nameof(ConflictingParentDrop);

            public string GetClassName() => nameof(ConflictingParentDrop);
        }

        internal class ConflictingChildDrop : ConflictingParentDrop
        {
            public new string Name { get; set; } = nameof(ConflictingChildDrop);

            public new string GetClassName() => nameof(ConflictingChildDrop);
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
        public void TestProductDrop()
        {
            Assert.DoesNotThrow(() =>
            {
                Template tpl = Template.Parse("  ");
                tpl.Render(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            });
        }

        [Test]
        public void TestDropDoesNotOutputItself()
        {
            string output = Template.Parse(" {{ product }} ")
                .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual("  ", output);
        }

        [Test]
        public void TestDropWithFilters()
        {
            string output = Template.Parse(" {{ product | product_text }} ")
                .Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    LocalVariables = Hash.FromAnonymousObject(new { product = new ProductDrop() }),
                    Filters = new[] { typeof(ProductFilter) }
                });
            Assert.AreEqual(" text1 ", output);
        }

        [Test]
        public void TestTextDrop()
        {
            string output = Template.Parse(" {{ product.texts.text }} ")
                .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual(" text1 ", output);
        }

        [Test]
        public void TestTextDrop2()
        {
            string output = Template.Parse(" {{ product.catchall.unknown }} ")
                .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual(" method: unknown ", output);
        }

        [Test]
        public void TestTextArrayDrop()
        {
            Assert.AreEqual(
                expected: "text1text2",
                actual: Template
                    .Parse("{{product.texts.array}}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));
            Assert.AreEqual(
                expected: " text1  text2 ",
                actual: Template
                    .Parse("{% for text in product.texts.array %} {{text}} {% endfor %}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));
        }

        [Test]
        public void TestTextListDrop()
        {
            Assert.AreEqual(
                expected: "text1text2",
                actual: Template
                    .Parse("{{product.texts.list}}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));
            Assert.AreEqual(
                expected: " text1  text2 ",
                actual: Template
                    .Parse("{% for text in product.texts.list %} {{text}} {% endfor %}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));
        }

        [Test]
        public void TestComplexDrop()
        {
            // Drop objects do not output themselves.
            Assert.AreEqual(
                expected: string.Empty,
                actual: Template
                    .Parse("{{ product.complex.single_drop }}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));

            // A complex drop object is still a drop object hence does not output oneself.
            Assert.AreEqual(
                expected: string.Empty,
                actual: Template
                    .Parse("{{ product.complex }}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));

            // Public properties within complex drop object do render when exactly accessed
            Assert.AreEqual(
                expected: "text1",
                actual: Template
                    .Parse("{{ product.complex.single_drop.text }}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));

            // While arrays are supported for render, when the array content is of drop object type, the rendering of each object is still empty.
            Assert.AreEqual(
                expected: string.Empty,
                actual: Template
                    .Parse("{% for text in product.complex.array_of_drops %}{{text}}{% endfor %}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));

            // We can still iterate through an array of drop objects then access the public properties of said object
            Assert.AreEqual(
                expected: "text1text1",
                actual: Template
                    .Parse("{% for text in product.complex.array_of_drops %}{{text.text}}{% endfor %}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));

            // The array of drop objects may itself contain a property of type array which can be rendered
            Assert.AreEqual(
                expected: "text1text2text1text2",
                actual: Template
                    .Parse("{% for text in product.complex.array_of_drops %}{{text.array}}{% endfor %}")
                    .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() })));
        }

        [Test]
        public void TestContextDrop()
        {
            string output = Template.Parse(" {{ context.bar }} ")
                .Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), bar = "carrot" }));
            Assert.AreEqual(" carrot ", output);
        }

        [Test]
        public void TestNestedContextDrop()
        {
            string output = Template.Parse(" {{ product.context.foo }} ")
                .Render(Hash.FromAnonymousObject(new { product = new ProductDrop(), foo = "monkey" }));
            Assert.AreEqual(" monkey ", output);
        }

        [Test]
        public void TestProtected()
        {
            string output = Template.Parse(" {{ product.call_me_not }} ")
                .Render(Hash.FromAnonymousObject(new { product = new ProductDrop() }));
            Assert.AreEqual("  ", output);
        }

        [Test]
        public void TestScope()
        {
            Assert.AreEqual("1", Template.Parse("{{ context.scopes }}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop() })));
            Assert.AreEqual("2", Template.Parse("{%for i in dummy%}{{ context.scopes }}{%endfor%}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
            Assert.AreEqual("3", Template.Parse("{%for i in dummy%}{%for i in dummy%}{{ context.scopes }}{%endfor%}{%endfor%}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
        }

        [Test]
        public void TestScopeThroughProc()
        {
            Assert.AreEqual("1", Template.Parse("{{ s }}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]) })));
            Assert.AreEqual("2", Template.Parse("{%for i in dummy%}{{ s }}{%endfor%}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]), dummy = new[] { 1 } })));
            Assert.AreEqual("3", Template.Parse("{%for i in dummy%}{%for i in dummy%}{{ s }}{%endfor%}{%endfor%}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]), dummy = new[] { 1 } })));
        }

        [Test]
        public void TestScopeWithAssigns()
        {
            Assert.AreEqual("variable", Template.Parse("{% assign a = 'variable'%}{{a}}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop() })));
            Assert.AreEqual("variable", Template.Parse("{% assign a = 'variable'%}{%for i in dummy%}{{a}}{%endfor%}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
            Assert.AreEqual("test", Template.Parse("{% assign header_gif = \"test\"%}{{header_gif}}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop() })));
            Assert.AreEqual("test", Template.Parse("{% assign header_gif = 'test'%}{{header_gif}}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop() })));
        }

        [Test]
        public void TestScopeFromTags()
        {
            Assert.AreEqual("1", Template.Parse("{% for i in context.scopes_as_array %}{{i}}{% endfor %}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
            Assert.AreEqual("12", Template.Parse("{%for a in dummy%}{% for i in context.scopes_as_array %}{{i}}{% endfor %}{% endfor %}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
            Assert.AreEqual("123", Template.Parse("{%for a in dummy%}{%for a in dummy%}{% for i in context.scopes_as_array %}{{i}}{% endfor %}{% endfor %}{% endfor %}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })));
        }

        [Test]
        public void TestAccessContextFromDrop()
        {
            Assert.AreEqual("123", Template.Parse("{% for a in dummy %}{{ context.loop_pos }}{% endfor %}").Render(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1, 2, 3 } })));
        }

        [Test]
        public void TestEnumerableDrop()
        {
            Assert.AreEqual("123", Template.Parse("{% for c in collection %}{{c}}{% endfor %}").Render(Hash.FromAnonymousObject(new { collection = new EnumerableDrop() })));
        }

        [Test]
        public void TestEnumerableDropSize()
        {
            Assert.AreEqual("3", Template.Parse("{{collection.size}}").Render(Hash.FromAnonymousObject(new { collection = new EnumerableDrop() })));
        }

        [Test]
        public void TestNullCatchAll()
        {
            Assert.AreEqual("", Template.Parse("{{ nulldrop.a_method }}").Render(Hash.FromAnonymousObject(new { nulldrop = new NullDrop() })));
        }

#if !CORE
        [Test]
        public void TestDataRowDrop()
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Column1");
            dataTable.Columns.Add("Column2");

            System.Data.DataRow dataRow = dataTable.NewRow();
            dataRow["Column1"] = "Hello";
            dataRow["Column2"] = "World";

            Template tpl = Template.Parse(" {{ row.column1 }} ");
            Assert.AreEqual(" Hello ", tpl.Render(Hash.FromAnonymousObject(new { row = new DataRowDrop(dataRow) })));
        }
#endif

        [Test]
        public void TestRubyNamingConventionPrintsHelpfulErrorIfMissingPropertyWouldMatchCSharpNamingConvention()
        {
            Helper.AssertTemplateResult(
                expected:"Missing property. Did you mean 'product_id'?",
                template: "{{ value.ProductID }}",
                anonymousObject: new { value = new CamelCaseDrop() },
                namingConvention: new RubyNamingConvention());
        }

        [Test]
        public void TestTypeResolutionDuplicateNames()
        {
            Helper.LockTemplateStaticVars(new RubyNamingConvention(), () =>
            {
                var type = typeof(ConflictingChildDrop);
                var resolver = new TypeResolution(type, mi => true);
                CollectionAssert.Contains(resolver.CachedMethods.Keys, "get_class_name");
                Assert.IsTrue(resolver.CachedMethods["get_class_name"].DeclaringType == type);
                CollectionAssert.Contains(resolver.CachedProperties.Keys, "name");
                Assert.IsTrue(resolver.CachedProperties["name"].DeclaringType == type);

                Helper.AssertTemplateResult(
                    expected: "ConflictingChildDrop|ConflictingChildDrop",
                    template: "{{ value.name }}|{{ value.get_class_name }}",
                    localVariables: Hash.FromAnonymousObject(new { value = new ConflictingChildDrop() }));
            });
        }
    }
}
