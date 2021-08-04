using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotLiquid.Util
{
    internal class ReflectionCache
    {
        private const TypeAttributes AnonymousTypeAttributes = TypeAttributes.NotPublic;
        private bool? _isAnonymous;
        private readonly Type _type;

        public ReflectionCache(Type type)
        {
            _type = type;
        }

        public bool IsAnonymous
        {
            get
            {
                if (!_isAnonymous.HasValue)
                {
#if NETSTANDARD1_3
                    var typeInfo = _type.GetTypeInfo();
#endif
                    _isAnonymous = (_type.Name.StartsWith("<>") || _type.Name.StartsWith("VB$"))
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

                return _isAnonymous.Value;
            }
        }
    }
}
