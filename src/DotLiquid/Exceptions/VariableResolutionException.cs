using System;

namespace DotLiquid.Exceptions
{
	[Serializable]
	public class VariableResolutionException : LiquidException
	{
        public VariableResolutionException(string message, params string[] args)
			: base(string.Format(message, args))
		{
		}
	}
}