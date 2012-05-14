using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.FileSystems;
using DotLiquid.Util;
using DotLiquid.NamingConventions;

namespace DotLiquid
{
	/// <summary>
	/// Templates are central to liquid.
	/// Interpretating templates is a two step process. First you compile the
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
	public class Template : ICopyable
	{
		private static Dictionary<string, Type> Tags { get; set; }
		private static readonly Dictionary<Type, Func<object, object>> SafeTypeTransformers;

        // Make sure we never set a null IFileSystem or INamingConvention (since users can change them)

	    private static IFileSystem _fileSystem = new BlankFileSystem();
	    public static IFileSystem FileSystem
	    {
	        get { return _fileSystem; }
            set { if (value != null) _fileSystem = value; }
	    }

        private static INamingConvention _namingConvention = new RubyNamingConvention();
	    public static  INamingConvention NamingConvention
	    {
            get { return _namingConvention; }
            set { if (value != null) _namingConvention = value; }
	    }

		static Template()
		{
			Tags = new Dictionary<string, Type>();
			SafeTypeTransformers = new Dictionary<Type, Func<object, object>>();
		}

		public static void RegisterTag<T>(string name)
			where T : Tag, new()
		{
			Tags[name] = typeof(T);
		}

		public static Type GetTagType(string name)
		{
			Type result;
			Tags.TryGetValue(name, out result);
			return result;
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
        /// Registers a simple type using the specified transformer.
        /// </summary>
		/// <param name="type">The type to register</param>
        /// <param name="func">Function that converts the specified type into a Liquid Drop-compatible object (eg, implements ILiquidizable)</param>
		public static void RegisterSafeType(Type type, Func<object, object> func)
        {
			SafeTypeTransformers[type] = func;
        }

		public static Func<object, object> GetSafeTypeTransformer(Type type)
		{
			if (SafeTypeTransformers.ContainsKey(type))
				return SafeTypeTransformers[type];
			return null;
		}

		/// <summary>
		/// Creates a new <tt>Template</tt> object from liquid source code
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static Template Parse(string source)
		{
			Template template = new Template();
			template.ParseInternal(source);
			return template;
		}

		private Hash _registers, _assigns, _instanceAssigns;

		public Document Root { get; set; }

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

        public object Copy()
        {
            Template template = new Template();
            if (_registers != null) template._registers = _registers.Copy();
            if (_assigns != null) template._assigns = _assigns.Copy();
            if (_instanceAssigns != null) template._instanceAssigns = _instanceAssigns.Copy();
            if (Root != null) template.Root = (Document)Root.Copy();
            return template;
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
		/// <param name="source"></param>
		/// <returns></returns>
		internal Template ParseInternal(string source)
		{
			source = DotLiquid.Tags.Literal.FromShortHand(source);
			source = DotLiquid.Tags.Comment.FromShortHand(source);

			Root = new Document();
			Root.Initialize(null, null, Tokenize(source));
			return this;
		}

		/// <summary>
		/// Renders the template using default parameters and returns a string containing the result.
		/// </summary>
		/// <returns></returns>
		public string Render()
        {
            List<Exception> errors;
            return Render(out errors);
		}

        /// <summary>
        /// Renders the template using default parameters and returns a string containing the result.
        /// </summary>
        /// <returns></returns>
        public string Render(out List<Exception> errors)
        {
            return Render(new RenderParameters(), out errors);
        }

        /// <summary>
        /// Renders the template using the specified local variables and returns a string containing the result.
        /// </summary>
        /// <param name="localVariables"></param>
        /// <returns></returns>
        public string Render(Hash localVariables)
        {
            List<Exception> errors;
            return Render(localVariables, out errors);
        }

	    /// <summary>
	    /// Renders the template using the specified local variables and returns a string containing the result.
	    /// </summary>
	    /// <param name="localVariables"></param>
	    /// <param name="errors"></param>
	    /// <returns></returns>
	    public string Render(Hash localVariables, out List<Exception> errors)
		{
			return Render(new RenderParameters
			{
				LocalVariables = localVariables
			}, out errors);
		}

        /// <summary>
        /// Renders the template using the specified parameters and returns a string containing the result.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string Render(RenderParameters parameters)
        {
            List<Exception> errors;
            return Render(parameters, out errors);
        }

	    /// <summary>
	    /// Renders the template using the specified parameters and returns a string containing the result.
	    /// </summary>
	    /// <param name="parameters"></param>
	    /// <param name="errors"></param>
	    /// <returns></returns>
	    public string Render(RenderParameters parameters, out List<Exception> errors)
		{
			using (TextWriter writer = new StringWriter())
			{
				errors = Render(writer, parameters);
				return writer.ToString();
			}
		}

		/// <summary>
		/// Renders the template into the specified StreamWriter.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="parameters"></param>
        public List<Exception> Render(TextWriter result, RenderParameters parameters)
		{
            return RenderInternal(result, parameters);
		}

		/// <summary>
		/// Renders the template into the specified Stream.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="parameters"></param>
        public List<Exception> Render(Stream stream, RenderParameters parameters)
		{
			// Can't dispose this new StreamWriter, because it would close the
			// passed-in stream, which isn't up to us.
			StreamWriter streamWriter = new StreamWriter(stream);
            List<Exception> errors = RenderInternal(streamWriter, parameters);
			streamWriter.Flush();
		    return errors;
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
		private List<Exception> RenderInternal(TextWriter result, RenderParameters parameters)
		{
            List<Exception> errors = new List<Exception>();
			if (Root == null) return errors;

			Context context;
			Hash registers;
			IEnumerable<Type> filters;
			parameters.Evaluate(this, out context, out registers, out filters);

			if (registers != null)
				context.Registers.Merge(registers);

			if (filters != null)
				context.AddFilters(filters);

			try
			{
				// Render the nodelist.
                Root.Render(context, result);
			}
			finally
			{
				errors = context.Errors;
			}
		    return errors;
		}

		/// <summary>
		/// Uses the <tt>Liquid::TemplateParser</tt> regexp to tokenize the passed source
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		internal static List<string> Tokenize(string source)
		{
			if (string.IsNullOrEmpty(source))
				return new List<string>();

			source = Regex.Replace(source, string.Format(@"-({0}|{1})(\n|\r\n|[ \t]+)?", Liquid.VariableEnd, Liquid.TagEnd), "$1");

			List<string> tokens = Regex.Split(source, Liquid.TemplateParser).ToList();

			// Trim any whitespace elements from the end of the array.
			for (int i = tokens.Count - 1; i > 0; --i)
				if (tokens[i] == string.Empty)
					tokens.RemoveAt(i);

			// Removes the rogue empty element at the beginning of the array
			if (tokens[0] != null && tokens[0] == string.Empty)
				tokens.Shift();

			return tokens;
		}
	}
}