using System;
using System.Collections.Generic;
using System.Reflection;
using DotLiquid.Exceptions;

namespace DotLiquid
{
	/// <summary>
	/// Strainer is the parent class for the filters system.
  /// New filters are mixed into the strainer class which is then instanciated for each liquid template render run.
  /// 
	/// One of the strainer's responsibilities is to keep malicious method calls out
	/// </summary>
	public class Strainer
	{
		private static readonly Dictionary<string, Type> Filters = new Dictionary<string, Type>();

		public static void GlobalFilter(Type filter)
		{
			Filters[filter.AssemblyQualifiedName] = filter;
		}

		public static Strainer Create(Context context)
		{
			Strainer strainer = new Strainer(context);
			foreach (var keyValue in Filters)
				strainer.Extend(keyValue.Value);
			return strainer;
		}

		private readonly Context _context;
		private readonly Dictionary<string, MethodInfo> _methods = new Dictionary<string, MethodInfo>();

		public IEnumerable<MethodInfo> Methods
		{
			get { return _methods.Values; }
		}

		public Strainer(Context context)
		{
			_context = context;
		}

		/// <summary>
		/// In this C# implementation, we can't use mixins. So we grab all the static
		/// methods from the specified type and use them instead.
		/// </summary>
		/// <param name="type"></param>
		public void Extend(Type type)
		{
			foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
				_methods[Liquid.NamingConvention.GetMemberName(methodInfo.Name)] = methodInfo;
		}

		public bool RespondTo(string method)
		{
			return _methods.ContainsKey(method);
		}

		public object Invoke(string method, List<object> args)
		{
			// Add in any default parameters - .NET won't do this for us.
			MethodInfo methodInfo = _methods[method];
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			if (parameterInfos.Length > args.Count)
				for (int i = args.Count; i < parameterInfos.Length; ++i)
				{
					if ((parameterInfos[i].Attributes & ParameterAttributes.HasDefault) != ParameterAttributes.HasDefault)
						throw new SyntaxException("Filter '{0}' does not have a default value for '{1}' and no value was supplied");
					args.Add(parameterInfos[i].DefaultValue);
				}

			try
			{
				return methodInfo.Invoke(null, args.ToArray());
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
	}
}