using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotLiquid.Util
{
    public static class ObjectExtensionMethods
    {
        private static HashSet<HashSet<Type>> _BackCompatComparableTypeBoundaries = new HashSet<HashSet<Type>>() {
            new HashSet<Type> { typeof(decimal), typeof(double), typeof(float), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort) },
            new HashSet<Type> { typeof(string), typeof(char) }
        };

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

        /// <summary>
        /// Test values for equality accross type boundaries except string to non-char, null-safe
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="otherValue">The second value.</param>
        /// <returns>True if the values are equal, false otherwise.</returns>
        public static bool BackCompatSafeTypeInsensitiveEqual(this object value, object otherValue)
        {
            if (value != null && otherValue != null)
            {
                // NOTE(daviburg): Historically testing for equality cross integer and string boundaries resulted in not equal.
                // This ensures we preserve the behavior.
                var comparedTypes = new HashSet<Type>() { value.GetType(), otherValue.GetType() };
                if (comparedTypes.Count > 1 && _BackCompatComparableTypeBoundaries.All(boundary => !comparedTypes.IsSubsetOf(boundary)))
                {
                    return false;
                }
            }

            return ObjectExtensionMethods.SafeTypeInsensitiveEqual(value: value, otherValue: otherValue);
        }

        /// <summary>
        /// Test values for equality accross type boundaries, null-safe
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="otherValue">The second value.</param>
        /// <returns>True if the values are equal, false otherwise.</returns>
        /// <remarks>For instance, crossing type boundaries, (long)1 and (int)1 are equal. In liquid this allows testing if the result of a given mathematical operation is present in an enumerable (list) regardless of the strict type.</remarks>
        public static bool SafeTypeInsensitiveEqual(this object value, object otherValue)
        {
            // NOTE(David Burg): null values cannot be tested for type, but they can be used for direct comparison.
            if (value == null)
            {
                return value == otherValue;
            }

            // NOTE(David Burg): a is not null so if b is null the values are not equal.
            if (otherValue == null)
            {
                return false;
            }

            // NOTE(David Burg): If both types are the same we can just do a regular comparison
            var aType = value.GetType();
            var bType = otherValue.GetType();
            if (aType == bType)
            {
                // NOTE(David Burg): Use Equals method to allow unboxing. Comparing boxed values with == operator would lead to reference comparison and unexpected results.
                return value.Equals(otherValue);
            }

            // NOTE(David Burg): When types are different we need to try if one can be converted to the other without loss or vice-versa
            // NOTE(David Burg): Order in which conversion is attempted changes the outcome for comparison between string and bool.
            // It's out of Shopify spec compliance but so for backward compatibility.
            try
            {
                return Convert.ChangeType(otherValue, aType).Equals(value);
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
            {
                try
                {
                    return Convert.ChangeType(value, bType).Equals(otherValue);
                }
                catch (Exception ex2) when (ex2 is InvalidCastException || ex2 is FormatException || ex2 is OverflowException)
                {
                    // NOTE(David Burg): Types are not the same and we can't convert so the values cannot be the same.
                    return false;
                }
            }
        }

        /// <summary>
        /// All values in Liquid are Truthy except nil and false.
        /// </summary>
        /// <param name="any">any object</param>
        /// <returns>True if the object is Truthy</returns>
        /// <see href="https://shopify.github.io/liquid/basics/truthy-and-falsy/"/>
        public static bool IsTruthy(this object any) => !IsFalsy(any);

        /// <summary>
        /// The only values that are Falsy in Liquid are nil and false.
        /// </summary>
        /// <param name="any">any object</param>
        /// <returns>True if the object is Falsy</returns>
        /// <see href="https://shopify.github.io/liquid/basics/truthy-and-falsy/"/>
        public static bool IsFalsy(this object any)
        {
            return any == null
                || (any is bool _bool && _bool == false)
                || (any is string _string && "false".Equals(_string, StringComparison.OrdinalIgnoreCase));
        }
    }
}
