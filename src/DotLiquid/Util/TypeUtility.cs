using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotLiquid.Util
{
    internal static class TypeUtility
    {
        private const TypeAttributes AnonymousTypeAttributes = TypeAttributes.NotPublic;

        public static bool IsAnonymousType(Type t)
        {
            return t.GetTypeInfo().GetCustomAttribute(typeof(CompilerGeneratedAttribute), false) != null
                && t.GetTypeInfo().IsGenericType
                    && (t.Name.Contains("AnonymousType") || t.Name.Contains("AnonType"))
                        && (t.Name.StartsWith("<>", StringComparison.Ordinal) || t.Name.StartsWith("VB$", StringComparison.Ordinal))
                            && (t.GetTypeInfo().Attributes & AnonymousTypeAttributes) == AnonymousTypeAttributes;
        }
    }
}
