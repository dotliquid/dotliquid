using System;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using DotLiquid.Tests.Util;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class IncludeTagTests
    {
        private class OtherFileSystem : IFileSystem
        {
            public string ReadTemplateFile(Context context, string templateName)
            {
                return "from OtherFileSystem";
            }
        }

        private class InfiniteFileSystem : IFileSystem
        {
            public string ReadTemplateFile(Context context, string templateName)
            {
                return "-{% include 'loop' %}";
            }
        }

        private class ReflectFileSystem : IFileSystem
        {
            public string ReadTemplateFile(Context context, string templateName) => templateName;
        }

        [SetUp]
        public void SetUp()
        {
            var testTemplates = new Dictionary<string, string>()
            {
                { "product", "Product: {{ product.title }} " },
                { "locale_variables", "Locale: {{echo1}} {{echo2}}" },
                { "variant", "Variant: {{ variant.title }}" },
                { "nested_template", "{% include 'header' %} {% include 'body' %} {% include 'footer' %}" },
                { "body", "body {% include 'body_detail' %}" },
                { "nested_product_template", "Product: {{ nested_product_template.title }} {%include 'details'%} " },
                { "recursively_nested_template", "-{% include 'recursively_nested_template' %}" },
                { "pick_a_source", "from TestFileSystem" }
            };
            Template.FileSystem = new DictionaryFileSystem(testTemplates);
        }

        [Test]
        public void TestIncludeTagMustNotBeConsideredError()
        {
            Assert.That(Template.Parse("{% include 'product_template' %}").Errors.Count, Is.EqualTo(0));
            Assert.DoesNotThrow(() => Template.Parse("{% include 'product_template' %}").Render(new RenderParameters(CultureInfo.InvariantCulture) { RethrowErrors = true }));
        }

        [Test]
        public void TestIncludeTagLooksForFileSystemInRegistersFirst()
        {
            Assert.That(Template.Parse("{% include 'pick_a_source' %}").Render(new RenderParameters(CultureInfo.InvariantCulture) { Registers = Hash.FromAnonymousObject(new { file_system = new OtherFileSystem() }) }), Is.EqualTo("from OtherFileSystem"));
        }

        [Test]
        public void TestIncludeTagWith()
        {
            Assert.That(Template.Parse("{% include 'product' with products[0] %}").Render(Hash.FromAnonymousObject(new { products = new[] { Hash.FromAnonymousObject(new { title = "Draft 151cm" }), Hash.FromAnonymousObject(new { title = "Element 155cm" }) } })), Is.EqualTo("Product: Draft 151cm "));
        }

        [Test]
        public void TestIncludeTagWithDefaultName()
        {
            Assert.That(Template.Parse("{% include 'product' %}").Render(Hash.FromAnonymousObject(new { product = Hash.FromAnonymousObject(new { title = "Draft 151cm" }) })), Is.EqualTo("Product: Draft 151cm "));
        }

        [Test]
        public void TestIncludeTagFor()
        {
            Assert.That(Template.Parse("{% include 'product' for products %}").Render(Hash.FromAnonymousObject(new { products = new[] { Hash.FromAnonymousObject(new { title = "Draft 151cm" }), Hash.FromAnonymousObject(new { title = "Element 155cm" }) } })), Is.EqualTo("Product: Draft 151cm Product: Element 155cm "));
        }

        [Test]
        public void TestIncludeTagWithLocalVariables()
        {
            Assert.That(Template.Parse("{% include 'locale_variables' echo1: 'test123' %}").Render(), Is.EqualTo("Locale: test123 "));
        }

        [Test]
        public void TestIncludeTagWithStringVariable()
        {
            Helper.WithDictionaryFileSystem(new Dictionary<string, string>
                {
                    { "product", "Product: {{ product }}" }
            }, () =>
            {
                Assert.That(Template.Parse("{% include 'product' with 'foo' %}").Render(), Is.EqualTo("Product: foo"));
                Assert.That(Template.Parse("{% include 'product' for 'foo' %}").Render(), Is.EqualTo("Product: foo"));
            });
        }

        [Test]
        public void TestIncludeTagWithMultipleLocalVariables()
        {
            Assert.That(Template.Parse("{% include 'locale_variables' echo1: 'test123', echo2: 'test321' %}").Render(), Is.EqualTo("Locale: test123 test321"));
        }

        [Test]
        public void TestIncludeTagWithMultipleLocalVariablesFromContext()
        {
            Assert.That(Template.Parse("{% include 'locale_variables' echo1: echo1, echo2: more_echos.echo2 %}").Render(Hash.FromAnonymousObject(new { echo1 = "test123", more_echos = Hash.FromAnonymousObject(new { echo2 = "test321" }) })), Is.EqualTo("Locale: test123 test321"));
        }

        [Test]
        public void TestNestedIncludeTag()
        {
            Assert.That(Template.Parse("{% include 'body' %}").Render(), Is.EqualTo("body body_detail"));

            Assert.That(Template.Parse("{% include 'nested_template' %}").Render(), Is.EqualTo("header body body_detail footer"));
        }

        [Test]
        public void TestNestedIncludeTagWithVariable()
        {
            Assert.That(Template.Parse("{% include 'nested_product_template' with product %}").Render(Hash.FromAnonymousObject(new { product = Hash.FromAnonymousObject(new { title = "Draft 151cm" }) })), Is.EqualTo("Product: Draft 151cm details "));

            Assert.That(Template.Parse("{% include 'nested_product_template' for products %}").Render(Hash.FromAnonymousObject(new { products = new[] { Hash.FromAnonymousObject(new { title = "Draft 151cm" }), Hash.FromAnonymousObject(new { title = "Element 155cm" }) } })), Is.EqualTo("Product: Draft 151cm details Product: Element 155cm details "));
        }

        [Test]
        public void TestRecursivelyIncludedTemplateDoesNotProductEndlessLoop()
        {
            Template.FileSystem = new InfiniteFileSystem();

            Assert.Throws<StackLevelException>(() => Template.Parse("{% include 'loop' %}").Render(new RenderParameters(CultureInfo.InvariantCulture) { RethrowErrors = true }));
        }

        [Test]
        public void TestDynamicallyChosenTemplate()
        {
            Assert.That(Template.Parse("{% include template %}").Render(Hash.FromAnonymousObject(new { template = "Test123" })), Is.EqualTo("Test123"));
            Assert.That(Template.Parse("{% include template %}").Render(Hash.FromAnonymousObject(new { template = "Test321" })), Is.EqualTo("Test321"));

            Assert.That(Template.Parse("{% include template for product %}").Render(Hash.FromAnonymousObject(new { template = "product", product = Hash.FromAnonymousObject(new { title = "Draft 151cm" }) })), Is.EqualTo("Product: Draft 151cm "));
        }

        [Test]
        public void TestUndefinedTemplateVariableWithTestFileSystem()
        {
            Assert.That(Template.Parse(" hello {% include notthere %} world ").Render(), Is.EqualTo(" hello  world "));
        }

        [Test]
        public void TestUndefinedTemplateVariableWithLocalFileSystem()
        {
            Template.FileSystem = new LocalFileSystem(string.Empty);
            Assert.Throws<FileSystemException>(() => Template.Parse(" hello {% include notthere %} world ").Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                RethrowErrors = true
            }));
        }

        [Test]
        public void TestMissingTemplateWithLocalFileSystem()
        {
            Template.FileSystem = new LocalFileSystem(string.Empty);
            Assert.Throws<FileSystemException>(() => Template.Parse(" hello {% include 'doesnotexist' %} world ").Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                RethrowErrors = true
            }));
        }

        [Test]
        public void TestIncludeFromTemplateFileSystem()
        {
            TemplateFileSystem fileSystem = new TemplateFileSystem(Template.FileSystem);
            Template.FileSystem = fileSystem;
            for (int i = 0; i < 2; ++i)
            {
                Assert.That(Template.Parse("{% include 'product' with products[0] %}").Render(Hash.FromAnonymousObject(new { products = new[] { Hash.FromAnonymousObject(new { title = "Draft 151cm" }), Hash.FromAnonymousObject(new { title = "Element 155cm" }) } })), Is.EqualTo("Product: Draft 151cm "));
            }
            Assert.That(fileSystem.CacheHitTimes, Is.EqualTo(1));
        }

        [Test]
        public void TestDotLiquid24UndefinedTemplateVariableShouldError()
        {
            Assert.That(Template.Parse("{% include undefined_variable %}").Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid24
            }),
                Is.EqualTo("Liquid error: Argument error in tag 'include' - Illegal template name"));
        }

        [Test]
        public void TestDotLiquid24NotStringTemplateVariableShouldError()
        {
            Assert.That(Template.Parse("{% include 123 %}").Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid24
            }),
                Is.EqualTo("Liquid error: Argument error in tag 'include' - Illegal template name"));
        }

        [Test]
        public void TestDotLiquid24TemplateFileSystemReceiveTemplateNameInsteadOfRaw()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                Template.FileSystem = new ReflectFileSystem();
                Assert.That(Template.Parse("{% include 'product' %}").Render(), Is.EqualTo("'product'"));
                Assert.That(Template.Parse("{% include 'product' %}").Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid24
                }),
                    Is.EqualTo("product"));
            });
        }

        [Test]
        public void TestDotLiquid24CacheSecondReadSamePartial()
        {
            CountingFileSystem fileSystem = new CountingFileSystem();
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                Template.FileSystem = fileSystem;
                Assert.That(Template.Parse("{% include 'pick_a_source' %}{% include 'pick_a_source' %}").Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid24
                }),
                    Is.EqualTo("from CountingFileSystemfrom CountingFileSystem"));
                Assert.That(fileSystem.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestDotLiquid24DoesntCachePartialsAcrossRenders()
        {
            CountingFileSystem fileSystem = new CountingFileSystem();
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                Template.FileSystem = fileSystem;
                Assert.That(Template.Parse("{% include 'pick_a_source' %}").Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid24
                }),
                    Is.EqualTo("from CountingFileSystem"));
                Assert.That(fileSystem.Count, Is.EqualTo(1));
                Assert.That(Template.Parse("{% include 'pick_a_source' %}").Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid24
                }),
                    Is.EqualTo("from CountingFileSystem"));
                Assert.That(fileSystem.Count, Is.EqualTo(2));
            });
        }
    }
}
