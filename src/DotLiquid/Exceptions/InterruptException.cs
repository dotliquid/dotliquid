namespace DotLiquid.Exceptions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3925:'ISerializable' should be implemented correctly", Justification = "ISerializable not required")]
    public class InterruptException : LiquidException
    {
        public InterruptException(string message)
            : base(message)
        {
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3925:'ISerializable' should be implemented correctly", Justification = "ISerializable not required")]
    public class BreakInterrupt : InterruptException
    {
        public BreakInterrupt()
            : base("Misplaced 'break' statement")
        {
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3925:'ISerializable' should be implemented correctly", Justification = "ISerializable not required")]
    public class ContinueInterrupt : InterruptException
    {
        public ContinueInterrupt()
            : base("Misplaced 'continue' statement")
        {
        }
    }
}
