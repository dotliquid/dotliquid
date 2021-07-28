using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DotLiquid.FileSystems;
using DotLiquid.Util;
using DotLiquid.NamingConventions;

namespace DotLiquid
{
    /// <summary>
    /// Templates are central to liquid.
    /// Interpreting templates is a two step process. First you compile the
    /// source code you got. During compile time some extensive error checking is performed.
    /// your code should expect to get some SyntaxErrors.
    ///
    /// After you have a compiled template you can then <tt>render</tt> it.
    /// You can use a compiled template over and over again and keep it cached.
    ///
    /// Example:
    ///
    /// template = Liquid::Template.parse(source)
    /// template.render('user_name' => 'bob')
    /// </summary>
    public class Template
    {
        /// <summary>
        /// Naming convention used for template parsing
        /// </summary>
        /// <remarks>Default is Ruby</remarks>
        public static INamingConvention NamingConvention { get; set; }

        /// <summary>
        /// Filesystem used for template reading
        /// </summary>
        public static IFileSystem FileSystem { get; set; }

        /// <summary>
        /// Liquid syntax flag used for backward compatibility
        /// </summary>
        public static SyntaxCompatibility DefaultSyntaxCompatibilityLevel { get; set; } = SyntaxCompatibility.DotLiquid20;

        /// <summary>
        /// Indicates if the default is thread safe
        /// </summary>
        public static bool DefaultIsThreadSafe { get; set; }

        private static Dictionary<string, Tuple<ITagFactory, Type>> Tags { get; set; }

        /// <summary>
        /// TimeOut used for all Regex in DotLiquid
        /// </summary>
        public static TimeSpan RegexTimeOut { get; set; }

        private static readonly Dictionary<Type, Func<object, object>> SafeTypeTransformers;
        private static readonly Dictionary<Type, Func<object, object>> ValueTypeTransformers;
        private static readonly ConcurrentDictionary<Type, Func<object, object>> ValueTypeTransformerCache;

        static Template()
        {
            RegexTimeOut = TimeSpan.FromSeconds(10);
            NamingConvention = new RubyNamingConvention();
            FileSystem = new BlankFileSystem();
            Tags = new Dictionary<string, Tuple<ITagFactory, Type>>();
            SafeTypeTransformers = new Dictionary<Type, Func<object, object>>();
            ValueTypeTransformers = new Dictionary<Type, Func<object, object>>();
            ValueTypeTransformerCache = new ConcurrentDictionary<Type, Func<object, object>>();
        }

        /// <summary>
        /// Register a tag
        /// </summary>
        /// <typeparam name="T">Type of the tag</typeparam>
        /// <param name="name">Name of the tag</param>
        public static void RegisterTag<T>(string name)
            where T : Tag, new()
        {
            var tagType = typeof(T);
            Tags[name] = new Tuple<ITagFactory,Type>(new ActivatorTagFactory(tagType, name), tagType);
        }

        /// <summary>
        /// Registers a tag factory.
        /// </summary>
        /// <param name="tagFactory">The ITagFactory to be registered</param>
        public static void RegisterTagFactory(ITagFactory tagFactory)
        {
            Tags[tagFactory.TagName] = new Tuple<ITagFactory, Type>(tagFactory, null);
        }

        /// <summary>
        /// Get the tag type from it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetTagType(string name)
        {
            Tags.TryGetValue(name, out Tuple<ITagFactory, Type> result);
            return result.Item2;
        }

        /// <summary>
        /// Indicates if the tag is a block that stops Liquid syntax processing
        /// </summary>
        /// <param name="name">The name of the tag</param>
        /// <returns></returns>
        internal static bool IsRawTag(string name)
        {
            Tags.TryGetValue(name, out Tuple<ITagFactory, Type> result);
            return typeof(RawBlock)
#if NETSTANDARD1_3
                .GetTypeInfo()
#endif
                .IsAssignableFrom(result?.Item2
#if NETSTANDARD1_3
                    ?.GetTypeInfo()
#endif
                );
        }

        internal static Tag CreateTag(string name)
        {
            Tag tagInstance = null;
            Tags.TryGetValue(name, out Tuple<ITagFactory, Type> result);

            if (result != null)
            {
                tagInstance = result.Item1.Create();
            }

            return tagInstance;
        }

        /// <summary>
        /// Pass a module with filter methods which should be available
        ///  to all liquid views. Good for registering the standard library
        /// </summary>
        /// <param name="filter"></param>
        public static void RegisterFilter(Type filter)
        {
            Strainer.GlobalFilter(filter);
        }

        /// <summary>
        /// Registers a simple type. DotLiquid will wrap the object in a <see cref="DropProxy"/> object.
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
        public static void RegisterSafeType(Type type, string[] allowedMembers)
        {
            RegisterSafeType(type, x => new DropProxy(x, allowedMembers));
        }

        /// <summary>
        /// Registers a simple type. DotLiquid will wrap the object in a <see cref="DropProxy"/> object.
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
        /// <param name="func">Function that converts the specified type into a Liquid Drop-compatible object (eg, implements ILiquidizable)</param>
        public static void RegisterSafeType(Type type, string[] allowedMembers, Func<object, object> func)
        {
            RegisterSafeType(type, x => new DropProxy(x, allowedMembers, func));
        }

        /// <summary>
        /// Registers a simple type using the specified transformer.
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="func">Function that converts the specified type into a Liquid Drop-compatible object (eg, implements ILiquidizable)</param>
        public static void RegisterSafeType(Type type, Func<object, object> func)
        {
            SafeTypeTransformers[type] = func;
        }

        /// <summary>
        /// Registers a simple value type transformer.  Used for rendering a variable to the output stream
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="func">Function that converts the specified type into a Liquid Drop-compatible object (eg, implements ILiquidizable)</param>
        public static void RegisterValueTypeTransformer(Type type, Func<object, object> func)
        {
            ValueTypeTransformers[type] = func;
            ValueTypeTransformerCache.Clear();
        }

        /// <summary>
        /// Gets the corresponding value type converter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object, object> GetValueTypeTransformer(Type type)
        {
            // Check for concrete types
            if (ValueTypeTransformers.TryGetValue(type, out Func<object, object> transformer))
                return transformer;

            // Check for interfaces
            return ValueTypeTransformerCache.GetOrAdd(type, (key) =>
            {
                foreach (var interfaceType in type.GetTypeInfo().ImplementedInterfaces)
                {
                    if (ValueTypeTransformers.TryGetValue(interfaceType, out transformer))
                        return transformer;
                    if (interfaceType.GetTypeInfo().IsGenericType && ValueTypeTransformers.TryGetValue(interfaceType.GetGenericTypeDefinition(), out transformer))
                        return transformer;
                }

                return null;
            });
        }

        /// <summary>
        /// Gets the corresponding safe type transformer
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object, object> GetSafeTypeTransformer(Type type)
        {
            // Check for concrete types
            if (SafeTypeTransformers.TryGetValue(type, out Func<object, object> transformer))
                return transformer;

            // Check for interfaces
            var interfaces = type.GetTypeInfo().ImplementedInterfaces;
            foreach (var interfaceType in interfaces)
            {
                if (SafeTypeTransformers.TryGetValue(interfaceType, out transformer))
                    return transformer;
                if (interfaceType.GetTypeInfo().IsGenericType && SafeTypeTransformers.TryGetValue(
                    interfaceType.GetGenericTypeDefinition(), out transformer))
                    return transformer;
            }
            return null;
        }

        /// <summary>
        /// Creates a new <tt>Template</tt> object from liquid source code
        /// </summary>
        /// <param name="source">The Liquid Template string</param>
        /// <returns></returns>
        public static Template Parse(string source)
        {
            return Parse(source, Template.DefaultSyntaxCompatibilityLevel);
        }

        /// <summary>
        /// Creates a new <tt>Template</tt> object from liquid source code
        /// </summary>
        /// <param name="source">The Liquid Template string</param>
        /// <param name="syntaxCompatibilityLevel">The Liquid syntax flag used for backward compatibility</param>
        /// <returns></returns>
        public static Template Parse(string source, SyntaxCompatibility syntaxCompatibilityLevel)
        {
            Template template = new Template();
            template.ParseInternal(source, syntaxCompatibilityLevel);
            return template;
        }

        private Hash _registers, _assigns, _instanceAssigns;
        private List<Exception> _errors;
        private bool? _isThreadSafe;

        /// <summary>
        /// Liquid document
        /// </summary>
        public Document Root { get; set; }

        /// <summary>
        /// Hash of user-defined, internally-available variables
        /// </summary>
        public Hash Registers
        {
            get { return (_registers = _registers ?? new Hash()); }
        }

        public Hash Assigns
        {
            get { return (_assigns = _assigns ?? new Hash()); }
        }

        public Hash InstanceAssigns
        {
            get { return (_instanceAssigns = _instanceAssigns ?? new Hash()); }
        }

        /// <summary>
        /// Exceptions that have been raised during template rendering
        /// </summary>
        public List<Exception> Errors
        {
            get { return (_errors = _errors ?? new List<Exception>()); }
        }

        /// <summary>
        /// Indicates if the parsed templates will be thread safe
        /// </summary>
        public bool IsThreadSafe
        {
            get { return _isThreadSafe ?? DefaultIsThreadSafe; }
        }

        /// <summary>
        /// Creates a new <tt>Template</tt> from an array of tokens. Use <tt>Template.parse</tt> instead
        /// </summary>
        internal Template()
        {
        }

        /// <summary>
        /// Parse source code.
        /// Returns self for easy chaining
        /// </summary>
        /// <param name="source">The source code.</param>
        /// <param name="syntaxCompatibilityLevel">The Liquid syntax flag used for backward compatibility</param>
        /// <returns>The template.</returns>
        internal Template ParseInternal(string source, SyntaxCompatibility syntaxCompatibilityLevel)
        {
            this.Root = new Document();
            this.Root.Initialize(tagName: null, markup: null, tokens: Tokenizer.Tokenize(source, syntaxCompatibilityLevel));
            return this;
        }

        /// <summary>
        /// Make this template instance thread safe.
        /// After this call, you can't use template owned variables anymore.
        /// </summary>
        public void MakeThreadSafe()
        {
            _isThreadSafe = true;
        }

        /// <summary>
        /// Renders the template using default parameters and the current culture and returns a string containing the result.
        /// </summary>
        /// <returns>The rendering result as string.</returns>
        public string Render(IFormatProvider formatProvider = null)
        {
            formatProvider = formatProvider ?? CultureInfo.CurrentCulture;
            return Render(new RenderParameters(formatProvider));
        }

        /// <summary>
        /// Renders the template using the specified local variables and returns a string containing the result.
        /// </summary>
        /// <param name="localVariables">Local variables.</param>
        /// <param name="formatProvider">String formatting provider.</param>
        /// <returns>The rendering result as string.</returns>
        public string Render(Hash localVariables, IFormatProvider formatProvider=null)
        {
            using (var writer = new StringWriter(formatProvider ?? CultureInfo.CurrentCulture))
            {
                return this.Render(
                    writer: writer,
                    parameters: new RenderParameters(writer.FormatProvider)
                    {
                        LocalVariables = localVariables
                    });
            }
        }


        /// <summary>
        /// Renders the template using the specified parameters and returns a string containing the result.
        /// </summary>
        /// <param name="parameters">Render parameters.</param>
        /// <returns>The rendering result as string.</returns>
        public string Render(RenderParameters parameters)
        {
            using (var writer = new StringWriter(parameters.FormatProvider))
            {
                return this.Render(writer, parameters );
            }
        }

        /// <summary>
        /// Renders the template using the specified parameters and returns a string containing the result.
        /// </summary>
        /// <param name="writer">Render parameters.</param>
        /// <param name="parameters"></param>
        /// <returns>The rendering result as string.</returns>
        public string Render(TextWriter writer, RenderParameters parameters)
        {
            if (writer == null)
                throw new ArgumentNullException(paramName: nameof(writer));
            if (parameters == null)
                throw new ArgumentNullException(paramName: nameof(parameters));

            this.RenderInternal(writer, parameters);
            return writer.ToString();
        }

        /// <inheritdoc />
        private class StreamWriterWithFormatProvider : StreamWriter
        {
            public StreamWriterWithFormatProvider(Stream stream, IFormatProvider formatProvider) : base( stream ) => FormatProvider = formatProvider;

            public override IFormatProvider FormatProvider { get; }
        }

        /// <summary>
        /// Renders the template into the specified Stream.
        /// </summary>
        /// <param name="stream">The stream to render into.</param>
        /// <param name="parameters">The render parameters.</param>
        public void Render(Stream stream, RenderParameters parameters)
        {
            // Can't dispose this new StreamWriter, because it would close the
            // passed-in stream, which isn't up to us.
            StreamWriter streamWriter = new StreamWriterWithFormatProvider( stream, parameters.FormatProvider );
            RenderInternal(streamWriter, parameters);
            streamWriter.Flush();
        }

        /// <summary>
        /// Render takes a hash with local variables.
        ///
        /// if you use the same filters over and over again consider registering them globally
        /// with <tt>Template.register_filter</tt>
        ///
        /// Following options can be passed:
        ///
        /// * <tt>filters</tt> : array with local filters
        /// * <tt>registers</tt> : hash with register variables. Those can be accessed from
        /// filters and tags and might be useful to integrate liquid more with its host application
        /// </summary>
        private void RenderInternal(TextWriter result, RenderParameters parameters)
        {
            if (Root == null)
                return;

            parameters.Evaluate(this, out Context context, out Hash registers, out IEnumerable<Type> filters);

            if (!IsThreadSafe)
            {
                if (registers != null)
                    Registers.Merge(registers);

                if (filters != null)
                    context.AddFilters(filters);
            }

            try
            {
                // Render the nodelist.
                Root.Render(context, result);
            }
            finally
            {
                if (!IsThreadSafe)
                    _errors = context.Errors;
            }
        }
    }
}
