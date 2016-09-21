using System;

namespace DotLiquid.Exceptions
{
#if !CORE
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
