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
        [Obsolete]
        public bool RethrowErrors
        {
            get { return (ErrorsOutputMode == ErrorsOutputModeEnum.Rethrow); }
            set { ErrorsOutputMode = (value ? ErrorsOutputModeEnum.Rethrow : ErrorsOutputModeEnum.Display); }
        }

        public enum ErrorsOutputModeEnum
        {
            Rethrow,
            Suppress,
            Display
        }

        private ErrorsOutputModeEnum _erorsOutputMode = ErrorsOutputModeEnum.Display;

        public ErrorsOutputModeEnum ErrorsOutputMode
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

        public int MaxIterations
        {
            get { return _maxIterations; }
            set { _maxIterations = value; }
        }

        private int _timeout = 0;

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
                return;
            }

            List<Hash> environments = new List<Hash>();
            if (LocalVariables != null)
                environments.Add(LocalVariables);
            if (template.IsThreadSafe)
            {
                context = new Context(environments, new Hash(), new Hash(), ErrorsOutputMode, MaxIterations);
            }
            else
            {
                environments.Add(template.Assigns);
                context = new Context(environments, template.InstanceAssigns, template.Registers, ErrorsOutputMode, MaxIterations);
            }
            registers = Registers;
            filters = Filters;
        }

        public static RenderParameters FromContext(Context context)
        {
            return new RenderParameters { Context = context };
        }
    }
}
