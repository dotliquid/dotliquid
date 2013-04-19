using System.Reflection;
using DotLiquid.FileSystems;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class EmbeddedFileSystemTests
    {
        [Test]
        public void Resolve_simple_template_path()
        {
            // Arrange
            var fileSystem = new EmbeddedFileSystem(Assembly.GetExecutingAssembly(), "My.Files");

            // Act
            var path = fileSystem.FullPath("layout");

            // Assert
            Assert.That(path, Is.EqualTo("My.Files._layout.liquid"));
        }

        [Test]
        public void Resolve_template_path_with_folder()
        {
            // Arrange
            var fileSystem = new EmbeddedFileSystem(Assembly.GetExecutingAssembly(), "My.Files");

            // Act
            var path = fileSystem.FullPath("templates/layout");

            // Assert
            Assert.That(path, Is.EqualTo("My.Files.templates._layout.liquid"));
        }

        [Test]
        public void Read_a_template_from_test_embedded_resources()
        {
            // Arrange
            var fileSystem = new EmbeddedFileSystem(Assembly.GetExecutingAssembly(),
                "DotLiquid.Tests.Embedded");

            // Act
            var template = fileSystem.ReadTemplateFile(new Context(), "'layout'");

            // Assert
            Assert.That(template, Is.EqualTo("This is an embedded template"));
        }
    }
}