using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class FileSystemTests
    {
        private readonly string[] invalidPaths = new[]
        {
            Path.DirectorySeparatorChar + "root" + Path.DirectorySeparatorChar + "file",
            "folder/",
            "root/.hidden",
            ".hidden",
            "../test",
            "../dir/mypartial",
            "/dir/../../dir/mypartial",
            "/etc/passwd",
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\mypartial" : "/home/mypartial",
            "",
            "some\r\nmultiline"
        };

        private readonly Dictionary<string, string> validPaths = new Dictionary<string, string>()
        {
            { @"root/file.txt", Path.Combine("root", "_file.txt.liquid") },
            { @"root\file.txt", Path.Combine("root", "_file.txt.liquid") },
            { @"dir/mypartial", Path.Combine("dir", "_mypartial.liquid") },
            { @"dir\mypartial", Path.Combine("dir", "_mypartial.liquid") },
            { @"dir\subdir\mypartial", Path.Combine("dir", "subdir", "_mypartial.liquid") },
            { @"mypartial", "_mypartial.liquid" },
            { @"a", "_a.liquid" },
            { @"a/b", Path.Combine("a", "_b.liquid") },
            { @"-abc", "_-abc.liquid" },
            { @"a-bc", "_a-bc.liquid" }
        };

        [Test]
        public void TestDefault()
        {
            Assert.Throws<FileSystemException>(() => new BlankFileSystem().ReadTemplateFile(new Context(CultureInfo.InvariantCulture), "dummy"));
        }

        [Test]
        public void TestLocalCrossPlatform()
        {
            // Use a cross-platform base path
            string basePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? @"C:\Some\Path" 
                : "/home/some/path";
            
            LocalFileSystem fileSystem = new LocalFileSystem(basePath);
            
            foreach (var validPath in validPaths)
            {
                string expectedPath = Path.Combine(basePath, validPath.Value);
                Assert.AreEqual(
                    expected: expectedPath,
                    actual: fileSystem.FullPath(validPath.Key));
            }

            foreach (var invalidPath in invalidPaths)
                Assert.Throws<FileSystemException>(() => fileSystem.FullPath(invalidPath));
        }

        [Test]
        public void TestLocalWithBracketsInPathCrossPlatform()
        {
            // Use a cross-platform base path with brackets
            string basePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? @"C:\Some (thing)\Path" 
                : "/home/some (thing)/path";
            
            LocalFileSystem fileSystem = new LocalFileSystem(basePath);
            
            Assert.AreEqual(
                Path.Combine(basePath, "_mypartial.liquid"), 
                fileSystem.FullPath("mypartial"));
            
            Assert.AreEqual(
                Path.Combine(basePath, "dir", "_mypartial.liquid"), 
                fileSystem.FullPath("dir/mypartial"));
        }

        [Test]
        public void TestEmbeddedResource()
        {
            var assembly = typeof(FileSystemTests).GetTypeInfo().Assembly;
            EmbeddedFileSystem fileSystem = new EmbeddedFileSystem(assembly, "DotLiquid.Tests.Embedded");
            
            foreach (var validPath in validPaths)
            {
                // For embedded resources, path separators become dots
                string expectedResourceName = "DotLiquid.Tests.Embedded." + 
                    validPath.Value.Replace(Path.DirectorySeparatorChar, '.').Replace('/', '.').Replace('\\', '.');
                
                Assert.AreEqual(
                    expected: expectedResourceName,
                    actual: fileSystem.FullPath(validPath.Key));
            }

            foreach (var invalidPath in invalidPaths)
                Assert.Throws<FileSystemException>(() => fileSystem.FullPath(invalidPath));
        }
    }
}