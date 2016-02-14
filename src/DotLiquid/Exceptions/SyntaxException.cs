using System;

namespace DotLiquid.Exceptions
{
#if !NETCore
	[Serializable]
#endif
	public class SyntaxException : LiquidException
	{
		public SyntaxException(string message, params string[] args)
			: base(string.Format(message, args))
		{
		}
	}
}