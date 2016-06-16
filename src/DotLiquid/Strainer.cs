using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotLiquid.Exceptions;

namespace DotLiquid
{
    static class DictionaryExtensions
    {
        public static V TryAdd<K, V>(this IDictionary<K, V> dic, K key, Func<V> factory)
        {
            V found;
            if (!dic.TryGetValue(key, out found))
                return dic[key] = factory();
            return found;
        }
    }

    /// <summary>
    /// Strainer is the parent class for the filters system.
    /// New filters are mixed into the strainer class which is then instanciated for each liquid template render run.
    ///
    /// One of the strainer's responsibilities is to keep malicious method calls out
    /// </summary>
    public class Strainer
    {
        private static readonly Dictionary<string, Type> Filters = new Dictionary<string, Type>();
        private static readonly Dictionary<string, MethodInfo> FilterFuncs = new Dictionary<string, MethodInfo>();

        public static void GlobalFilter(Type filter)
        {
            Filters[filter.AssemblyQualifiedName] = filter;
        }

        public static void GlobalFilter(string rawName, MethodInfo methodInfo)
        {
            var name = Template.NamingConvention.GetMemberName(rawName);

            FilterFuncs[name] = methodInfo;
        }

        public static Strainer Create(Context context)
        {
            Strainer strainer = new Strainer(context);

            foreach (var keyValue in Filters)
                strainer.Extend(keyValue.Value);

            foreach (var keyValue in FilterFuncs)
                strainer.AddMethodInfo(keyValue.Key, keyValue.Value);
            
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
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var methodNames = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Select(m => Template.NamingConvention.GetMemberName(m.Name));

            foreach (var methodName in methodNames)
                _methods.Remove(methodName);

            foreach (MethodInfo methodInfo in methods)
            {
                AddMethodInfo(methodInfo.Name, methodInfo);
            } // foreach
        }

        public void AddFunction<TIn, TOut>(string rawName, Func<TIn, TOut> func)
        {
            AddMethodInfo(rawName, func.Method);
        }

        public void AddFunction<TIn, TIn2, TOut>(string rawName, Func<TIn, TIn2, TOut> func)
        {
            AddMethodInfo(rawName, func.Method);
        }

        public void AddMethodInfo(string rawName, MethodInfo method)
        {
            var name = Template.NamingConvention.GetMemberName(rawName);
            _methods.TryAdd(name, () => new List<MethodInfo>()).Add(method);
        }

        public bool RespondTo(string method)
        {
            return _methods.ContainsKey(method);
        }

        public object Invoke(string method, List<object> args)
        {
            // First, try to find a method with the same number of arguments.
            var methodInfo = _methods[method].FirstOrDefault(m => m.GetParameters().Length == args.Count);

            // If we failed to do so, try one with max numbers of arguments, hoping
            // that those not explicitly specified will be taken care of
            // by default values
            if (methodInfo == null)
                methodInfo = _methods[method].OrderByDescending(m => m.GetParameters().Length).First();

            if (methodInfo == null)
                throw new InvalidOperationException(string.Format("Cannot find method named {0}", method));

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
