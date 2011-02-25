using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
	[TestFixture]
	public class MemoryStreamWriterTests
	{
		[Test]
		public void CanWriteAndReadFromMemoryStreamWriter()
		{
			using (MemoryStreamWriter writer = new MemoryStreamWriter())
			{
				writer.Write("Hello world");

				Assert.AreEqual("Hello world", writer.ToString());
			}
		}
	}
}