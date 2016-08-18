using System;

namespace DotLiquid.Exceptions
{
#if !CORE
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
