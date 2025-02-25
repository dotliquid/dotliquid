using System;

namespace DotLiquid.Exceptions
{
#if !CORE
    [Serializable]
#endif
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3925:'ISerializable' should be implemented correctly", Justification = "ISerializable not required")]
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
