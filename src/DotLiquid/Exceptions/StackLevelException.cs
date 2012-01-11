using System;

namespace DotLiquid.Exceptions
{
	[Serializable]
	public class StackLevelException : LiquidException
	{
		public StackLevelException(string message)
			: base(string.Format(message))
		{
		}
	}
}