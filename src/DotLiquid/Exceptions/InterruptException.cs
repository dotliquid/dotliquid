namespace DotLiquid.Exceptions
{
    public class InterruptException : LiquidException
    {
        public InterruptException(string message)
            : base(message)
        {
        }
    }

    public class BreakInterrupt : InterruptException
    {
        public static readonly BreakInterrupt Instance = new BreakInterrupt();
        public BreakInterrupt()
            : base("Misplaced 'break' statement")
        {
        }
    }

    public class ContinueInterrupt : InterruptException
    {
        public static readonly ContinueInterrupt Instance = new ContinueInterrupt();
        public ContinueInterrupt()
            : base("Misplaced 'continue' statement")
        {
        }
    }
}
