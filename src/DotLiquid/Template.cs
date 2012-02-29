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
	public class Template
	{
		public static INamingConvention NamingConvention;
		public static IFileSystem FileSystem { get; set; }
		private static Dictionary<string, Type> Tags { get; set; }

		static Template()
		{
			NamingConvention = new RubyNamingConvention();
			FileSystem = new BlankFileSystem();
			Tags = new Dictionary<string, Type>();
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

        internal static Dictionary<Type, Func<object, object>> simpleTypeTransformers = new Dictionary<Type, Func<object, object>>();

        /// <summary>
        /// Registers a simple type. DotLiquid will wrap the object in a <see cref="DropProxy"/> object.
        /// </summary>
        /// <param name="t">The type to register</param>
        public static void RegisterSimpleType(Type t)
        {
            RegisterSimpleType(t, (x) => new DropProxy(x));
        }
        
        /// <summary>
        /// Registers a simple type using the specified transformer.
        /// </summary>
        /// <param name="t">The type to register</param>
        /// <param name="func">Function that converts the specified type into a Liquid Drop-compatible object (eg, implements ILiquidizable)</param>
        public static void RegisterSimpleType(Type t, Func<object, object> func)
        {
            simpleTypeTransformers.Add(t, func);
        }


        /// <summary>
        /// Dictionary that holds namespace=>declaredOnly 
        /// </summary>
        internal static IDictionary<string, bool> simpleTypeNamespaces = new Dictionary<string, bool>();
        /// <summary>
        /// Registers all types in the specified namespace (exact match) as renderable (equivalent to marking with [LiquidType] attribute).
        /// DotLiquid will wrap in a <see cref="DropProxy"/> object.
        /// </summary>
        /// <param name="namespace">The namespace to add (case sensitive, exact match)</param>        
        /// <param name="declaredOnly">Specifies that only members declared at the level of the supplied type's hierarchy should be considered. Inherited members are not considered.</param>
        public static void RegisterSimpleNamespace(string @namespace, bool declaredOnly)
        {
            simpleTypeNamespaces.Add(@namespace, declaredOnly);
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

			source = Regex.Replace(source, string.Format(@"-({0}|{1})(\n|\r\n)", Liquid.VariableEnd, Liquid.TagEnd), "$1");

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