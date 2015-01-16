using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly Dictionary<string, IList<MethodInfo>> _methods = new Dictionary<string, IList<MethodInfo>>();

		public IEnumerable<MethodInfo> Methods
		{
			get { return _methods.Values.SelectMany(m => m); }
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
			// From what I can tell, calls to Extend should replace existing filters. So be it.
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
							.Select(m => new
							{
								Method = m,
								MethodName = Template.NamingConvention.GetMemberName(m.Name)
							})
							.ToList();

			foreach (var method in methods)
				_methods.Remove(method.MethodName);

			foreach (var method in methods)
			{
				IList<MethodInfo> methodList;
				if (!_methods.TryGetValue(method.MethodName, out methodList))
				{
					methodList = new List<MethodInfo>();
					_methods[method.MethodName] = methodList;
				}

				methodList.Add(method.Method);
			}
		}

		public bool RespondTo(string method)
		{
			return _methods.ContainsKey(method);
		}

		public object Invoke(string method, List<object> args)
		{
			var methodInfos = _methods[method].Select(s =>
				new
				{
					Method = s,
					Parameters = s.GetParameters()
				})
				.ToList();

			// First, try to find a method with the same number of arguments.
			var methodInfo = methodInfos.FirstOrDefault(m => m.Parameters.Length == args.Count);

			// If we failed to do so, try one with max numbers of arguments, hoping
			// that those not explicitly specified will be taken care of
			// by default values
			if (methodInfo == null)
				methodInfo = methodInfos.OrderByDescending(m => m.Parameters.Length).First();

			var parameterInfos = methodInfo.Parameters;

			// If first parameter is Context, send in actual context.
			if (parameterInfos.Length > 0 && parameterInfos[0].ParameterType == typeof(Context))
				args.Insert(0, _context);

			// Add in any default parameters - .NET won't do this for us.
			if (parameterInfos.Length > args.Count)
				for (int i = args.Count; i < parameterInfos.Length; ++i)
				{
					if ((parameterInfos[i].Attributes & ParameterAttributes.HasDefault) != ParameterAttributes.HasDefault)
						throw new SyntaxException(Liquid.ResourceManager.GetString("StrainerFilterHasNoValueException"), method, parameterInfos[i].Name);
					args.Add(parameterInfos[i].DefaultValue);
				}

			try
			{
				return methodInfo.Method.Invoke(null, args.ToArray());
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
	}
}