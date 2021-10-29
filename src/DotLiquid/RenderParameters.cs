using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotLiquid
{
    /// <summary>
    /// Rendering parameters
    /// </summary>
    public class RenderParameters
    {
        /// <summary>
        /// A list of errors that occurred during render
        /// </summary>
        public IList<Exception> Errors => _errors;

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

        /// <summary>
        /// Liquid syntax flag used for backward compatibility
        /// </summary>
        public SyntaxCompatibility SyntaxCompatibilityLevel { get; set; }

        private int _maxIterations = 0;

        /// <summary>
        /// Maximum number of iterations for the For tag
        /// </summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
            set { _maxIterations = value; }
        }

        private readonly List<Exception> _errors = new List<Exception>();
        private int _timeout = 0;
        public IFormatProvider FormatProvider { get; }

        public RenderParameters(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? throw new ArgumentNullException( nameof(formatProvider) );
            SyntaxCompatibilityLevel = Template.DefaultSyntaxCompatibilityLevel;
        }

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
            Hash instanceAssigns, contextRegisters;
            if (template.IsThreadSafe)
            {
                instanceAssigns = new Hash();
                contextRegisters = new Hash();
                
            }
            else
            {
                instanceAssigns = template.InstanceAssigns;
                contextRegisters = template.Registers;
                environments.Add(template.Assigns);
            }

            context = new Context(environments, instanceAssigns, contextRegisters, ErrorsOutputMode, MaxIterations, Timeout, FormatProvider, _errors, default)
            {
                SyntaxCompatibilityLevel = this.SyntaxCompatibilityLevel
            };
            registers = Registers;
            filters = Filters;
        }

        /// <summary>
        /// Creates a RenderParameters from a context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static RenderParameters FromContext(Context context, IFormatProvider formatProvider)
        {
            if (context == null)
                throw new ArgumentNullException( nameof(context) );
            return new RenderParameters(formatProvider) { Context = context };
        }
    }
}
