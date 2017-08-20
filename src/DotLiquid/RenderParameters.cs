using System;
using System.Collections.Generic;

namespace DotLiquid
{
    /// <summary>
    /// Rendering parameters
    /// </summary>
    public class RenderParameters
    {
        /// <summary>
        /// If you provide a Context object, you do not need to set any other parameters.
        /// </summary>
        public Context Context { get; set; }

        /// <summary>
        /// Hash of local variables used during rendering
        /// </summary>
        public Hash LocalVariables { get; set; }

        /// <summary>
        /// Filters used during rendering
        /// </summary>
        public IEnumerable<Type> Filters { get; set; }

        /// <summary>
        /// Hash of user-defined, internally-available variables
        /// </summary>
        public Hash Registers { get; set; }

        /// <summary>
        /// Gets or sets a value that controls whether errors are thrown as exceptions.
        /// </summary>
        [Obsolete("Use ErrorsOutputMode instead")]
        public bool RethrowErrors
        {
            get { return (ErrorsOutputMode == ErrorsOutputMode.Rethrow); }
            set { ErrorsOutputMode = (value ? ErrorsOutputMode.Rethrow : ErrorsOutputMode.Display); }
        }
        
        private ErrorsOutputMode _erorsOutputMode = ErrorsOutputMode.Display;

        /// <summary>
        /// Errors output mode
        /// </summary>
        public ErrorsOutputMode ErrorsOutputMode
        {
            get
            {
                return _erorsOutputMode;
            }

            set
            {
                _erorsOutputMode = value;
            }
        }

        private int _maxIterations = 0;

        /// <summary>
        /// Maximum number of iterations for the For tag
        /// </summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
            set { _maxIterations = value; }
        }

        private int _timeout = 0;

        /// <summary>
        /// Rendering timeout in ms
        /// </summary>
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        internal void Evaluate(Template template, out Context context, out Hash registers, out IEnumerable<Type> filters)
        {
            if (Context != null)
            {
                context = Context;
                registers = null;
                filters = null;
                context.RestartTimeout();
                return;
            }

            List<Hash> environments = new List<Hash>();
            if (LocalVariables != null)
                environments.Add(LocalVariables);
            if (template.IsThreadSafe)
            {
                context = new Context(environments, new Hash(), new Hash(), ErrorsOutputMode, MaxIterations, Timeout);
            }
            else
            {
                environments.Add(template.Assigns);
                context = new Context(environments, template.InstanceAssigns, template.Registers, ErrorsOutputMode, MaxIterations, Timeout);
            }
            registers = Registers;
            filters = Filters;
        }

        /// <summary>
        /// Creates a RenderParameters from a context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static RenderParameters FromContext(Context context)
        {
            return new RenderParameters { Context = context };
        }
    }
}
