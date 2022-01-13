using System;
using System.Threading;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotLiquid.Util
{
    /// <summary>
    /// Utility to avoid repetitive reflection calls that are found to be slow.
    /// </summary>
    internal class ReflectionCacheValue
    {
        private const TypeAttributes AnonymousTypeAttributes = TypeAttributes.NotPublic;
        private readonly Type _type;
        private readonly Lazy<bool> _isAnonymous;

        public ReflectionCacheValue(Type type)
        {
            _type = type;
            _isAnonymous = new Lazy<bool>(() => IsAnonymousInternal(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public bool IsAnonymous => _isAnonymous.Value;

        private bool IsAnonymousInternal()
        {
#if NETSTANDARD1_3
            var typeInfo = _type.GetTypeInfo();
#endif
            return (_type.Name.StartsWith("<>") || _type.Name.StartsWith("VB$"))
                && (_type.Name.Contains("AnonymousType") || _type.Name.Contains("AnonType"))
#if NETSTANDARD1_3
                    && typeInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                        && typeInfo.IsGenericType
                            && (typeInfo.Attributes & AnonymousTypeAttributes) == AnonymousTypeAttributes;
#else
                    && _type.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                        && _type.IsGenericType
                            && (_type.Attributes & AnonymousTypeAttributes) == AnonymousTypeAttributes;
#endif
        }
    }
}
