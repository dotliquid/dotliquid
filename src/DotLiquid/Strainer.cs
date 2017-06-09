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
        private static readonly Dictionary<string, Tuple<object, MethodInfo>> FilterFuncs = new Dictionary<string, Tuple<object, MethodInfo>>();

        public static void GlobalFilter(Type filter)
        {
            Filters[filter.AssemblyQualifiedName] = filter;
        }

        public static void GlobalFilter(string rawName, object target, MethodInfo methodInfo)
        {
            var name = Template.NamingConvention.GetMemberName(rawName);

            FilterFuncs[name] = Tuple.Create(target, methodInfo);
        }

        public static Strainer Create(Context context)
        {
            Strainer strainer = new Strainer(context);

            foreach (var keyValue in Filters)
                strainer.Extend(keyValue.Value);

            foreach (var keyValue in FilterFuncs)
                strainer.AddMethodInfo(keyValue.Key, keyValue.Value.Item1, keyValue.Value.Item2);
            
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
        public void Extend(Type type)
        {
            // From what I can tell, calls to Extend should replace existing filters. So be it.
            var methods = type.GetRuntimeMethods().Where(m => m.IsPublic && m.IsStatic);
            var methodNames = methods.Select(m => Template.NamingConvention.GetMemberName(m.Name));

            foreach (var methodName in methodNames)
                _methods.Remove(methodName);

            foreach (MethodInfo methodInfo in methods)
            {
                AddMethodInfo(methodInfo.Name, null, methodInfo);
            } // foreach
        }

        public void AddFunction<TIn, TOut>(string rawName, Func<TIn, TOut> func)
        {
            AddMethodInfo(rawName, func.Target, func.GetMethodInfo());
        }

        public void AddFunction<TIn, TIn2, TOut>(string rawName, Func<TIn, TIn2, TOut> func)
        {
            AddMethodInfo(rawName, func.Target, func.GetMethodInfo());
        }

        public void AddMethodInfo(string rawName, object target, MethodInfo method)
        {
            var name = Template.NamingConvention.GetMemberName(rawName);
            _methods.TryAdd(name, () => new List<Tuple<object, MethodInfo>>()).Add(Tuple.Create(target, method));
        }

        public bool RespondTo(string method)
        {
            return _methods.ContainsKey(method);
        }

        public object Invoke(string method, List<object> args)
        {
            // First, try to find a method with the same number of arguments minus context which we set automatically further down.
            var methodInfo = _methods[method].FirstOrDefault(m => 
                m.Item2.GetParameters().Where(p => p.ParameterType != typeof(Context)).Count() == args.Count);

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

            try
            {
                return methodInfo.Item2.Invoke(methodInfo.Item1, args.ToArray());
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
