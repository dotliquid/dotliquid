using System;
using System.Collections.Generic;

namespace DotLiquid
{
    public class RenderParameters
    {
        /// <summary>
        /// If you provide a Context object, you do not need to set any other parameters.
        /// </summary>
        public Context Context { get; set; }

        public Hash LocalVariables { get; set; }
        public IEnumerable<Type> Filters { get; set; }
        public Hash Registers { get; set; }

        /// <summary>
        /// Gets or sets a value that controls whether errors are thrown as exceptions.
        /// </summary>
        public bool RethrowErrors { get; set; }

        internal void Evaluate(Template template, out Context context)
        {
            if (Context != null)
            {
                context = Context;
                return;
            }

            List<Hash> environments = new List<Hash>();
            if (LocalVariables != null)
                environments.Add(LocalVariables);
            context = new Context(environments, Registers, RethrowErrors);
            if (Filters != null)
            {
                context.AddFilters(Filters);
            }
        }

        public static RenderParameters FromContext(Context context)
        {
            return new RenderParameters { Context = context };
        }
    }
}
