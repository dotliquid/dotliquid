using System;

namespace DotLiquid.Exceptions
{
    public class ArgumentException : LiquidException
    {
        public ArgumentException(string message, params string[] args)
            : base(string.Format(message, args))
        {
        }

        public ArgumentException()
        {
        }
    }
}
