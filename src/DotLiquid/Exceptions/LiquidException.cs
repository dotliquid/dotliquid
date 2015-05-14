using System;

namespace DotLiquid.Exceptions
{
#if !NETCore
    [Serializable]
#endif
    public abstract class LiquidException :
#if NETCore
        Exception
#else
        ApplicationException
#endif
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