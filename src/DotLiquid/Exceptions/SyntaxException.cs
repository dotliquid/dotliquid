using System;

namespace DotLiquid.Exceptions
{
	[Serializable]
	public class SyntaxException : LiquidException
	{
		public SyntaxException(string message, params string[] args)
			: base(string.Format(message, args))
		{
		}
	}
}