using System;
using System.Collections.Generic;
using System.Text;

namespace DotLiquid.Exceptions
{
    class MaximumIterationsExceededException : RenderException
    {
        public MaximumIterationsExceededException(string message, params string[] args)
            : base(string.Format(message, args))
        {
        }

        public MaximumIterationsExceededException()
        {
        }
    }
}
