using System;

namespace DotLiquid.Exceptions
{
    [Serializable]
    public abstract class RenderException : LiquidException
    {
        protected RenderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RenderException(string message)
            : base(message)
        {
        }

        protected RenderException()
        {
        }
    }
}
