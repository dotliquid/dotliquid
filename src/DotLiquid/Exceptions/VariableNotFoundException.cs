using System;

namespace DotLiquid.Exceptions
{
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
