using System;
using System.Linq;
using System.Reflection;

namespace DotLiquid.Util
{
    public static class ObjectExtensionMethods
    {
        public static bool RespondTo(this object value, string member, bool ensureNoParameters = true)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            Type type = value.GetType();

            MethodInfo methodInfo = type.GetRuntimeMethod(member, Type.EmptyTypes);
            if (methodInfo != null && (!ensureNoParameters || !methodInfo.GetParameters().Any()))
                return true;

            PropertyInfo propertyInfo = type.GetRuntimeProperty(member);
            if (propertyInfo != null && propertyInfo.CanRead)
                return true;

            return false;
        }

        public static object Send(this object value, string member, object[] parameters = null)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            Type type = value.GetType();

            MethodInfo methodInfo = type.GetRuntimeMethod(member, Type.EmptyTypes);
            if (methodInfo != null)
                return methodInfo.Invoke(value, parameters);

            PropertyInfo propertyInfo = type.GetRuntimeProperty(member);
            if (propertyInfo != null)
                return propertyInfo.GetValue(value, null);

            return null;
        }
    }
}
