using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using DotLiquid.Exceptions;
using DotLiquid.NamingConventions;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// Strainer is the parent class for the filters system.
    /// New filters are mixed into the strainer class which is then instantiated for each liquid template render run.
    ///
    /// One of the strainer's responsibilities is to keep malicious method calls out
    /// </summary>
    public class Strainer
    {
        private static readonly Dictionary<string, Type> Filters = new Dictionary<string, Type>();
        private static readonly Dictionary<string, Tuple<object, MethodInfo>> FilterFuncs = new Dictionary<string, Tuple<object, MethodInfo>>();

        public static void GlobalFilter(Type filter)
        {
            Filters[filter.AssemblyQualifiedName] = filter;
        }

        public static void GlobalFilter(string rawName, object target, MethodInfo methodInfo, INamingConvention namingConvention)
        {
            var name = namingConvention.GetMemberName(rawName);

            FilterFuncs[name] = Tuple.Create(target, methodInfo);
        }

        public static Strainer Create(Context context)
        {
            Strainer strainer = new Strainer(context);

            foreach (var keyValue in Filters)
                strainer.Extend(keyValue.Value, context.NamingConvention);

            foreach (var keyValue in FilterFuncs)
                strainer.AddMethodInfo(keyValue.Key, keyValue.Value.Item1, keyValue.Value.Item2, context.NamingConvention);
            
            return strainer;
        }

        private readonly Context _context;
        private readonly Dictionary<string, IList<Tuple<object, MethodInfo>>> _methods = new Dictionary<string, IList<Tuple<object, MethodInfo>>>();

        public IEnumerable<MethodInfo> Methods
        {
            get { return _methods.Values.SelectMany(m => m.Select(x => x.Item2)); }
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
        /// <param name="namingConvention">Naming convention used for template parsing</param>
        public void Extend(Type type, INamingConvention namingConvention)
        {
            // Calls to Extend replace existing filters with the same number of params.
            var methods = type.GetRuntimeMethods().Where(m => m.IsPublic && m.IsStatic);
            foreach (var method in methods)
            {
                string methodName = namingConvention.GetMemberName(method.Name);
                if  (_methods.Any(m => method.MatchesMethod(m, namingConvention)))
                {
                    _methods.Remove(methodName);
                }
            }

            foreach (MethodInfo methodInfo in methods)
            {
                AddMethodInfo(methodInfo.Name, null, methodInfo, namingConvention);
            } // foreach
        }

        public void AddFunction<TIn, TOut>(string rawName, Func<TIn, TOut> func, INamingConvention namingConvention)
        {
            AddMethodInfo(rawName, func.Target, func.GetMethodInfo(), namingConvention);
        }

        public void AddFunction<TIn, TIn2, TOut>(string rawName, Func<TIn, TIn2, TOut> func, INamingConvention namingConvention)
        {
            AddMethodInfo(rawName, func.Target, func.GetMethodInfo(), namingConvention);
        }

        public void AddMethodInfo(string rawName, object target, MethodInfo method, INamingConvention namingConvention)
        {
            var name = namingConvention.GetMemberName(rawName);
            _methods.TryAdd(name, () => new List<Tuple<object, MethodInfo>>()).Add(Tuple.Create(target, method));
        }

        public bool RespondTo(string method)
        {
            return _methods.ContainsKey(method);
        }

        /// <summary>
        /// Invoke specified method with provided arguments
        /// </summary>
        /// <param name="method">The method token.</param>
        /// <param name="args">The arguments for invoking the method</param>
        /// <returns>The method's return.</returns>
        public object Invoke(string method, List<object> args)
        {
            // First, try to find a method with the same number of arguments minus context which we set automatically further down.
            var methodInfo = _methods[method].FirstOrDefault(m => 
                m.Item2.GetNonContextParameterCount() == args.Count);

            // If we failed to do so, try one with max numbers of arguments, hoping
            // that those not explicitly specified will be taken care of
            // by default values
            if (methodInfo == null)
                methodInfo = _methods[method].OrderByDescending(m => m.Item2.GetParameters().Length).First();

            ParameterInfo[] parameterInfos = methodInfo.Item2.GetParameters();

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

            // Attempt conversions where required by type mismatch and possible by value range.
            // These may be narrowing conversions (e.g. Int64 to Int32) when the actual range doesn't cause an overflow.
            for (var argumentIndex = 0; argumentIndex < parameterInfos.Length; argumentIndex++)
            {
                if (args[argumentIndex] is IConvertible convertibleArg)
                {
                    var parameterType = parameterInfos[argumentIndex].ParameterType;
                    if (convertibleArg.GetType() != parameterType
                        && !parameterType
#if NETSTANDARD1_3
                            .GetTypeInfo()
#endif
                            .IsAssignableFrom(
                                convertibleArg
                                    .GetType()
#if NETSTANDARD1_3
                                    .GetTypeInfo()
#endif
                                    )
                        )
                    {
                        args[argumentIndex] = Convert.ChangeType(convertibleArg, parameterType);
                    }
                }
            }

            try
            {
                return methodInfo.Item2.Invoke(methodInfo.Item1, args.ToArray());
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
