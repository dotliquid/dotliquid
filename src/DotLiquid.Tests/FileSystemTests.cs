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

        // Helper method to get expected path based on input
        private string GetExpectedPath(string basePath, string input)
        {
            // Normalize the input path to use forward slashes
            string normalizedInput = input.Replace('\\', '/');
            
            if (normalizedInput.Contains('/'))
            {
                var parts = normalizedInput.Split('/');
                var dirParts = new string[parts.Length - 1];
                Array.Copy(parts, dirParts, parts.Length - 1);
                var fileName = "_" + parts[parts.Length - 1] + ".liquid";
                
                if (dirParts.Length > 0)
                {
                    var fullDirParts = new string[dirParts.Length + 1];
                    fullDirParts[0] = basePath;
                    Array.Copy(dirParts, 0, fullDirParts, 1, dirParts.Length);
                    return Path.Combine(Path.Combine(fullDirParts), fileName);
                }
                else
                {
                    return Path.Combine(basePath, fileName);
                }
            }
            else
            {
                return Path.Combine(basePath, "_" + input + ".liquid");
            }
        }

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

            // Test various path formats
            var testPaths = new[]
            {
                "mypartial",
                "dir/mypartial",
                @"dir\mypartial",
                "dir/subdir/mypartial",
                @"dir\subdir\mypartial",
                "root/file.txt",
                @"root\file.txt",
                "a",
                "a/b",
                "-abc",
                "a-bc"
            };

            foreach (var testPath in testPaths)
            {
                var actual = fileSystem.FullPath(testPath);
                var expected = GetExpectedPath(basePath, testPath);
                
                // Both should produce the same normalized path
                Assert.AreEqual(
                    Path.GetFullPath(expected),
                    Path.GetFullPath(actual),
                    $"Failed for input: {testPath}");
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
                Path.GetFullPath(Path.Combine(basePath, "_mypartial.liquid")), 
                Path.GetFullPath(fileSystem.FullPath("mypartial")));
            
            Assert.AreEqual(
                Path.GetFullPath(Path.Combine(basePath, "dir", "_mypartial.liquid")), 
                Path.GetFullPath(fileSystem.FullPath("dir/mypartial")));
        }

        [Test]
        public void TestEmbeddedResource()
        {
            var assembly = typeof(FileSystemTests).GetTypeInfo().Assembly;
            EmbeddedFileSystem fileSystem = new EmbeddedFileSystem(assembly, "DotLiquid.Tests.Embedded");
            
            var testPaths = new Dictionary<string, string>
            {
                { "mypartial", "DotLiquid.Tests.Embedded._mypartial.liquid" },
                { "dir/mypartial", "DotLiquid.Tests.Embedded.dir._mypartial.liquid" },
                { @"dir\mypartial", "DotLiquid.Tests.Embedded.dir._mypartial.liquid" },
                { "dir/subdir/mypartial", "DotLiquid.Tests.Embedded.dir.subdir._mypartial.liquid" },
                { @"dir\subdir\mypartial", "DotLiquid.Tests.Embedded.dir.subdir._mypartial.liquid" },
                { "root/file.txt", "DotLiquid.Tests.Embedded.root._file.txt.liquid" },
                { @"root\file.txt", "DotLiquid.Tests.Embedded.root._file.txt.liquid" },
                { "a", "DotLiquid.Tests.Embedded._a.liquid" },
                { "a/b", "DotLiquid.Tests.Embedded.a._b.liquid" },
                { "-abc", "DotLiquid.Tests.Embedded._-abc.liquid" },
                { "a-bc", "DotLiquid.Tests.Embedded._a-bc.liquid" }
            };
            
            foreach (var testPath in testPaths)
            {
                Assert.AreEqual(
                    expected: testPath.Value,
                    actual: fileSystem.FullPath(testPath.Key),
                    message: $"Failed for input: {testPath.Key}");
            }

            foreach (var invalidPath in invalidPaths)
                Assert.Throws<FileSystemException>(() => fileSystem.FullPath(invalidPath));
        }
    }
}