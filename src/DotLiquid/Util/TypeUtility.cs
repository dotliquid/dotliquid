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
			return t.GetCustomAttributes(typeof (CompilerGeneratedAttribute), false).Length == 1
				&& t.IsGenericType
				&& t.Name.Contains("AnonymousType")
				&& (t.Name.StartsWith("<>") || t.Name.StartsWith("VB$"))
				&& (t.Attributes & AnonymousTypeAttributes) == AnonymousTypeAttributes;
		}
	}
}