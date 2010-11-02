using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class FileSystemTests
	{
		[Test]
		public void TestDefault()
		{
			Assert.Throws<FileSystemException>(() => new BlankFileSystem().ReadTemplateFile(new Context(), "dummy"));
		}

		[Test]
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
	}
}