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
			return Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute), false)
				&& t.IsGenericType
					&& (t.Name.Contains("AnonymousType") || t.Name.Contains("AnonType"))
						&& (t.Name.StartsWith("<>") || t.Name.StartsWith("VB$"))
							&& (t.Attributes & AnonymousTypeAttributes) == AnonymousTypeAttributes;
		}
	}
}