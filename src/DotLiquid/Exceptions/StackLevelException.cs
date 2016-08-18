using System;

namespace DotLiquid.Exceptions
{
#if !CORE
    [Serializable]
#endif
    public class StackLevelException : LiquidException
    {
        public StackLevelException(string message)
            : base(string.Format(message))
        {
        }
    }
}
