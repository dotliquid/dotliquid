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
		/// </summary>
		public override string ToString()
		{
			Flush();
			StreamReader reader = new StreamReader(BaseStream);
			long savedPosition = BaseStream.Position;
			BaseStream.Position = 0;
			string result = reader.ReadToEnd();
			BaseStream.Position = savedPosition;
			return result;
		}
	}
}