using System;

namespace DotLiquid.Exceptions
{
#if !CORE
    [Serializable]
#endif
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S3925:'ISerializable' should be implemented correctly", Justification = "ISerializable not required")]
    public abstract class RenderException :
#if CORE
        Exception
#else
        ApplicationException
#endif
    {
        protected RenderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RenderException(string message)
            : base(message)
        {
        }

        protected RenderException()
        {
        }
    }
}
