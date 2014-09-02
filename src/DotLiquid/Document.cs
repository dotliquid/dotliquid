using System.Collections.Generic;
using System.IO;
using DotLiquid.Exceptions;

namespace DotLiquid
{
	public class Document : Block
	{
		/// <summary>
		/// We don't need markup to open this block
		/// </summary>
		/// <param name="tagName"></param>
		/// <param name="markup"></param>
		/// <param name="tokens"></param>
		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Parse(tokens);
		}

		/// <summary>
		/// There isn't a real delimiter
		/// </summary>
		protected override string BlockDelimiter
		{
			get { return string.Empty; }
		}

		/// <summary>
		/// Document blocks don't need to be terminated since they are not actually opened
		/// </summary>
		protected override void AssertMissingDelimitation()
		{
		}

	    public override void Render(Context context, TextWriter result)
	    {
	        try
	        {
	            base.Render(context, result);
	        }
	        catch (BreakInterrupt)
	        {
	        }
	        catch (ContinueInterrupt)
	        {
	        }
	    }
	}
}