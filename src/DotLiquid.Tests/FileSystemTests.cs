using System.Globalization;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    [TestFixture]
    public class FileSystemTests
    {
        private readonly string[] invalidPaths = new[]
        {
            "\root\file",
            "folder/",
            "root/.hidden",
            ".hidden",
            "../test",
            "../dir/mypartial",
            "/dir/../../dir/mypartial",
            "/etc/passwd",
            "C:\\mypartial",
            "",
            "some\r\nmultiline"
        };

        private readonly Dictionary<string, string> validPaths = new Dictionary<string, string>()
        {
            { @"root/file.txt", @"root\_file.txt.liquid" },
            { @"root\file.txt", @"root\_file.txt.liquid" },
            { @"dir/mypartial", @"dir\_mypartial.liquid" },
            { @"dir\mypartial", @"dir\_mypartial.liquid" },
            { @"dir\subdir\mypartial", @"dir\subdir\_mypartial.liquid" },
            { @"mypartial", @"_mypartial.liquid" },
            { @"a", @"_a.liquid" },
            { @"a/b", @"a\_b.liquid" },
            { @"-abc", @"_-abc.liquid" },
            { @"a-bc",  @"_a-bc.liquid" }
        };

        [Test]
        public void TestDefault()
        {
            Assert.Throws<FileSystemException>(() => new BlankFileSystem().ReadTemplateFile(new Context(CultureInfo.InvariantCulture), "dummy"));
        }
        

        [Test]
        [Category("windows")]
        public void TestLocal()
        {
            LocalFileSystem fileSystem = new LocalFileSystem(@"D:\Some\Path");
            foreach (var validPath in validPaths)
                Assert.AreEqual(
                    expected: Path.Combine(@"D:\Some\Path", validPath.Value.Replace('\\', Path.DirectorySeparatorChar)),
                    actual: fileSystem.FullPath(validPath.Key));

            foreach (var invalidPath in invalidPaths)
                Assert.Throws<FileSystemException>(() => fileSystem.FullPath(invalidPath));
        }

        [Test]
        [Category("windows")]
        public void TestLocalWithBracketsInPath()
        {
            LocalFileSystem fileSystem = new LocalFileSystem(@"D:\Some (thing)\Path");
            Assert.AreEqual(@"D:\Some (thing)\Path\_mypartial.liquid".Replace('\\', Path.DirectorySeparatorChar), fileSystem.FullPath("mypartial"));
            Assert.AreEqual(@"D:\Some (thing)\Path\dir\_mypartial.liquid".Replace('\\', Path.DirectorySeparatorChar), fileSystem.FullPath("dir/mypartial"));
        }
        

        [Test]
        public void TestEmbeddedResource()
        {
            var assembly = typeof(FileSystemTests).GetTypeInfo().Assembly;
            EmbeddedFileSystem fileSystem = new EmbeddedFileSystem(assembly, "DotLiquid.Tests.Embedded");
            foreach (var validPath in validPaths)
                Assert.AreEqual(
                    expected: "DotLiquid.Tests.Embedded." + Liquid.DirectorySeparatorsRegex.Replace(validPath.Value, "."),
                    actual: fileSystem.FullPath(validPath.Key));

            foreach (var invalidPath in invalidPaths)
                Assert.Throws<FileSystemException>(() => fileSystem.FullPath(invalidPath));
        }
    }
}
