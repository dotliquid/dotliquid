using System;

namespace DotLiquid.Exceptions
{
#if !CORE
    [Serializable]
#endif
    public abstract class LiquidException :
#if CORE
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
