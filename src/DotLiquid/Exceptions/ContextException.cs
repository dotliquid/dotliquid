using System;

namespace DotLiquid.Exceptions
{
#if !NETCore
    [Serializable]
#endif
	public class ContextException : LiquidException
	{
		public ContextException(string message, params string[] args)
			: base(string.Format(message, args))
		{
		}

		public ContextException()
		{
		}
	}
}