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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3376:Make this class name end with 'Exception'", Justification = "That would be a breaking change")]
    public class BreakInterrupt : InterruptException
    {
        public BreakInterrupt()
            : base("Misplaced 'break' statement")
        {
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3925:'ISerializable' should be implemented correctly", Justification = "ISerializable not required")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3376:Make this class name end with 'Exception'", Justification = "That would be a breaking change")]
    public class ContinueInterrupt : InterruptException
    {
        public ContinueInterrupt()
            : base("Misplaced 'continue' statement")
        {
        }
    }
}
