using System;

namespace DotLiquid.Exceptions
{
    public class DisabledException : LiquidException
    {
        public DisabledException(string message, params string[] args)
            : base(string.Format(message, args))
        {
        }
    }
}
