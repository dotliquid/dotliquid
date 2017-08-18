using System;

namespace DotLiquid.Exceptions
{
#if !CORE
    [Serializable]
#endif
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
