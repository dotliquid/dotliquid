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
		public static INamingConvention NamingConvention;
		public static IFileSystem FileSystem { get; set; }
		private static Dictionary<string, Type> Tags { get; set; }
        private static readonly Dictionary<Type, Func<object, object>> SafeTypeTransformers;
		private static readonly Dictionary<Type, Func<object, object>> ValueTypeTransformers;

		static Template()
		{
			NamingConvention = new RubyNamingConvention();
			FileSystem = new BlankFileSystem();
			Tags = new Dictionary<string, Type>();
            SafeTypeTransformers = new Dictionary<Type, Func<object, object>>();
			ValueTypeTransformers = new Dictionary<Type, Func<object, object>>();
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
        /// Registers a simple type. DotLiquid will wrap the object in a <see cref="DropProxy"/> object.
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
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
        }

		public static Func<object, object> GetValueTypeTransformer(Type type)
		{
            // Check for concrete types
			if (ValueTypeTransformers.ContainsKey(type))
				return ValueTypeTransformers[type];

            // Check for interfaces
		    foreach (var interfaceType in ValueTypeTransformers.Where(x => x.Key.IsInterface))
		    {
                if (type.GetInterfaces().Contains(interfaceType.Key))
                    return interfaceType.Value;
		    }

			return null;
		}

        public static Func<object, object> GetSafeTypeTransformer(Type type)
		{
            // Check for concrete types
			if (SafeTypeTransformers.ContainsKey(type))
                return SafeTypeTransformers[type];

            // Check for interfaces
            foreach (var interfaceType in SafeTypeTransformers.Where(x => x.Key.IsInterface))
            {
                if (type.GetInterfaces().Contains(interfaceType.Key))
                    return interfaceType.Value;
            }

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
		private List<Exception> _errors;

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

		public List<Exception> Errors
		{
			get { return (_errors = _errors ?? new List<Exception>()); }
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
			return Render(new RenderParameters());
		}

		/// <summary>
		/// Renders the template using the specified local variables and returns a string containing the result.
		/// </summary>
		/// <param name="localVariables"></param>
		/// <returns></returns>
		public string Render(Hash localVariables)
		{
			return Render(new RenderParameters
			{
				LocalVariables = localVariables
			});
		}

		/// <summary>
		/// Renders the template using the specified parameters and returns a string containing the result.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public string Render(RenderParameters parameters)
		{
			using (TextWriter writer = new StringWriter())
			{
				Render(writer, parameters);
				return writer.ToString();
			}
		}

		/// <summary>
		/// Renders the template into the specified StreamWriter.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="parameters"></param>
		public void Render(TextWriter result, RenderParameters parameters)
		{
			RenderInternal(result, parameters);
		}

		/// <summary>
		/// Renders the template into the specified Stream.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="parameters"></param>
		public void Render(Stream stream, RenderParameters parameters)
		{
			// Can't dispose this new StreamWriter, because it would close the
			// passed-in stream, which isn't up to us.
			StreamWriter streamWriter = new StreamWriter(stream);
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

			Context context;
			Hash registers;
			IEnumerable<Type> filters;
			parameters.Evaluate(this, out context, out registers, out filters);

			if (registers != null)
				Registers.Merge(registers);

			if (filters != null)
				context.AddFilters(filters);

			try
			{
				// Render the nodelist.
				Root.Render(context, result);
			}
			finally
			{
				_errors = context.Errors;
			}
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

			// Trim leading whitespace.
			source = Regex.Replace(source, string.Format(@"([ \t]+)?({0}|{1})-", Liquid.VariableStart, Liquid.TagStart), "$2");

			// Trim trailing whitespace.
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