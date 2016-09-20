using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    using System.Reflection;

    [TestFixture]
    public class FileSystemTests
    {
        [Test]
        public void TestDefault()
        {
            Assert.Throws<FileSystemException>(() => new BlankFileSystem().ReadTemplateFile(new Context(), "dummy"));
        }
        

        [Test]
        [Platform(Exclude = "Linux,Mono", Reason = "Path format specific to windows")]
        public void TestLocal()
        {
            LocalFileSystem fileSystem = new LocalFileSystem(@"D:\Some\Path");
            Assert.AreEqual(@"D:\Some\Path\_mypartial.liquid", fileSystem.FullPath("mypartial"));
            Assert.AreEqual(@"D:\Some\Path\dir\_mypartial.liquid", fileSystem.FullPath("dir/mypartial"));

            Assert.Throws<FileSystemException>(() => fileSystem.FullPath("../dir/mypartial"));
            Assert.Throws<FileSystemException>(() => fileSystem.FullPath("/dir/../../dir/mypartial"));
            Assert.Throws<FileSystemException>(() => fileSystem.FullPath("/etc/passwd"));
            Assert.Throws<FileSystemException>(() => fileSystem.FullPath(@"C:\mypartial"));
        }

        [Test]
        [Platform(Exclude = "Linux,Mono", Reason = "Path format specific to windows")]
        public void TestLocalWithBracketsInPath()
        {
            LocalFileSystem fileSystem = new LocalFileSystem(@"D:\Some (thing)\Path");
            Assert.AreEqual(@"D:\Some (thing)\Path\_mypartial.liquid", fileSystem.FullPath("mypartial"));
            Assert.AreEqual(@"D:\Some (thing)\Path\dir\_mypartial.liquid", fileSystem.FullPath("dir/mypartial"));
        }
        

        [Test]
        public void TestEmbeddedResource()
        {
            var assembly = typeof(FileSystemTests).GetTypeInfo().Assembly;
            EmbeddedFileSystem fileSystem = new EmbeddedFileSystem(assembly, "DotLiquid.Tests.Embedded");
            Assert.AreEqual(@"DotLiquid.Tests.Embedded._mypartial.liquid", fileSystem.FullPath("mypartial"));
            Assert.AreEqual(@"DotLiquid.Tests.Embedded.dir._mypartial.liquid", fileSystem.FullPath("dir/mypartial"));

            Assert.Throws<FileSystemException>(() => fileSystem.FullPath("../dir/mypartial"));
            Assert.Throws<FileSystemException>(() => fileSystem.FullPath("/dir/../../dir/mypartial"));
            Assert.Throws<FileSystemException>(() => fileSystem.FullPath("/etc/passwd"));
            Assert.Throws<FileSystemException>(() => fileSystem.FullPath(@"C:\mypartial"));
        }
    }
}
