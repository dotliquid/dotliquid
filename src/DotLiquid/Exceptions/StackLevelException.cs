using System;

namespace DotLiquid.Exceptions
{
    public class StackLevelException : LiquidException
    {
        public StackLevelException(string message)
            : base(string.Format(message))
        {
        }
    }
}
