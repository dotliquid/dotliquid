using System;

namespace DotLiquid.Exceptions
{
	[Serializable]
	public abstract class LiquidException : ApplicationException
	{
		protected LiquidException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected LiquidException(string message)
			: base(message)
		{
		}

		protected LiquidException()
		{
		}
	}
}