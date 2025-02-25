using System;

namespace DotLiquid.Exceptions
{
#if !CORE
    [Serializable]
#endif
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3925:'ISerializable' should be implemented correctly", Justification = "ISerializable not required")]
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
