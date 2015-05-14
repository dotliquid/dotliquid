using System;

namespace DotLiquid.Exceptions
{
#if !NETCore
    [Serializable]
#endif
    public class StackLevelException : LiquidException
	{
		public StackLevelException(string message)
			: base(string.Format(message))
		{
		}
	}
}