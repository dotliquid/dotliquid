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
        private static readonly Dictionary<string, Func<object, object>> FilterFuncs = new Dictionary<string, Func<object, object>>();

        public static void GlobalFilter(Type filter)
        {
            Filters[filter.AssemblyQualifiedName] = filter;
        }

        public static void GlobalFunction<TIn, TOut>(string rawName, Func<TIn, TOut> func)
        {
            var name = Template.NamingConvention.GetMemberName(rawName);

            FilterFuncs[name] = i => (object)func((TIn)i);
        }

        public static Strainer Create(Context context)
        {
            Strainer strainer = new Strainer(context);
            foreach (var keyValue in Filters)
                strainer.Extend(keyValue.Value);

            foreach (var keyValue in FilterFuncs)
                strainer._funcs[keyValue.Key] = keyValue.Value;
            
            return strainer;
        }

        private readonly Context _context;
        private readonly Dictionary<string, IList<MethodInfo>> _methods = new Dictionary<string, IList<MethodInfo>>();
        private readonly Dictionary<string, Func<object, object>> _funcs = new Dictionary<string, Func<object, object>>();

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
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var methodNames = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Select(m => Template.NamingConvention.GetMemberName(m.Name));

            foreach (var methodName in methodNames)
            {
                _methods.Remove(methodName);
                _funcs.Remove(methodName);
            }

            foreach (MethodInfo methodInfo in methods)
            {
                var name = Template.NamingConvention.GetMemberName(methodInfo.Name);

                if (!_methods.ContainsKey(name))
                    _methods[name] = new List<MethodInfo>();

                _methods[name].Add(methodInfo);
            } // foreach
        }

        public void AddFunction<TIn, TOut>(string rawName, Func<TIn, TOut> func)
        {
            var name = Template.NamingConvention.GetMemberName(rawName);

            _methods.Remove(name);
            _funcs[name] = o => (object)func((TIn)o);
        }

        public bool RespondTo(string method)
        {
            return _methods.ContainsKey(method) || _funcs.ContainsKey(method);
        }

        public object Invoke(string method, List<object> args)
        {
            Func<object, object> func;
            if (args.Count == 1 && _funcs.TryGetValue(method, out func))
                return func(args[0]);

            // First, try to find a method with the same number of arguments.
            var methodInfo = _methods[method].FirstOrDefault(m => m.GetParameters().Length == args.Count);

            // If we failed to do so, try one with max numbers of arguments, hoping
            // that those not explicitly specified will be taken care of
            // by default values
            if (methodInfo == null)
                methodInfo = _methods[method].OrderByDescending(m => m.GetParameters().Length).First();

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

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
                return methodInfo.Invoke(null, args.ToArray());
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
