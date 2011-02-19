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
			NamingConvention = new CSharpNamingConvention(); // we live in a .net world baby!
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
		private bool _rethrowErrors;

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

		public string Render(Context context)
		{
			MemoryStreamWriter streamWriter = new MemoryStreamWriter();
			RenderInternal(streamWriter, context, null, null);
			return streamWriter.ToString();
		}

		public void Render(StreamWriter streamWriter, Context context)
		{
			RenderInternal(streamWriter, context, null, null);
		}

#if NET35
        public string Render()
        {
            return Render((Hash) null, null, null);
        }

        public string Render(IEnumerable<Type> filters)
        {
            return Render(null, filters, null);
        }

        public string Render(Hash hash)
        {
            return Render(hash, null, null);
        }

        public string Render(Hash hash, IEnumerable<Type> filters)
        {
            return Render(hash, filters, null);
        }

        public string Render(Hash localVariables, IEnumerable<Type> filters, Hash registers)
#else
		public string Render(Hash localVariables = null, IEnumerable<Type> filters = null, Hash registers = null)
#endif
		{
			MemoryStreamWriter streamWriter = new MemoryStreamWriter();
			Render(streamWriter, localVariables, filters, registers);
			return streamWriter.ToString();
		}

#if NET35
        public void Render(StreamWriter streamWriter)
        {
			Render(streamWriter, null, null, null);
        }

		public void Render(StreamWriter streamWriter, IEnumerable<Type> filters)
        {
            Render(streamWriter, null, filters, null);
        }

		public void Render(StreamWriter streamWriter, Hash hash)
        {
			Render(streamWriter, hash, null, null);
        }

		public void Render(StreamWriter streamWriter, Hash hash, IEnumerable<Type> filters)
        {
			Render(streamWriter, hash, filters, null);
        }

		public void Render(StreamWriter streamWriter, Hash localVariables, IEnumerable<Type> filters, Hash registers)
#else
		public void Render(StreamWriter streamWriter, Hash localVariables = null, IEnumerable<Type> filters = null, Hash registers = null)
#endif
		{
			List<Hash> environments = new List<Hash>();
			if (localVariables != null)
				environments.Add(localVariables);
			environments.Add(Assigns);
			Context context = new Context(environments, InstanceAssigns, Registers, _rethrowErrors);

			RenderInternal(streamWriter, context, registers, filters);
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
		private void RenderInternal(StreamWriter streamWriter, Context context, Hash registers, IEnumerable<Type> filters)
		{
			if (Root == null)
				return;

			if (registers != null)
				Registers.Merge(registers);

			if (filters != null)
				context.AddFilters(filters);

			try
			{
				// Render the nodelist.
				Root.Render(context, streamWriter);
			}
			finally
			{
				_errors = context.Errors;
			}
		}

#if NET35
        public string RenderAndRethrowErrors()
        {
            return RenderAndRethrowErrors((Hash)null);
        }

        public string RenderAndRethrowErrors(Hash hash)
#else
		public string RenderAndRethrowErrors(Hash hash = null)
#endif
		{
			_rethrowErrors = true;
			return Render(hash, null, null);
		}

#if NET35
        public void RenderAndRethrowErrors(StreamWriter streamWriter)
        {
			RenderAndRethrowErrors(streamWriter, (Hash)null);
        }

        public void RenderAndRethrowErrors(StreamWriter streamWriter, Hash hash)
#else
		public void RenderAndRethrowErrors(StreamWriter streamWriter, Hash hash = null)
#endif
		{
			_rethrowErrors = true;
			Render(streamWriter, hash, null, null);
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

			source = Regex.Replace(source, string.Format(@"-({0}|{1})\n", Liquid.VariableEnd, Liquid.TagEnd), "$1");

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
