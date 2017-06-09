using System;

namespace DotLiquid.Exceptions
{
#if !CORE
    [Serializable]
#endif
    public class VariableNotFoundException : LiquidException
    {
        public VariableNotFoundException(string message, params string[] args)
            : base(string.Format(message, args))
        {
        }

        public VariableNotFoundException(string message)
            : base(message)
        {
        }
    }
}
