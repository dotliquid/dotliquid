using System.IO;

namespace DotLiquid.Util
{
	public class MemoryStreamWriter : StreamWriter
	{
		public MemoryStreamWriter()
			: base(new MemoryStream())
		{
			
		}

		/// <summary>
		/// Returns the text written to the stream so far.
		/// After calling this property, no more text can be written because
		/// the stream will be disposed.
		/// </summary>
		public override string ToString()
		{
			Flush();
			StreamReader reader = new StreamReader(BaseStream);
			BaseStream.Position = 0;
			return reader.ReadToEnd();
		}
	}
}