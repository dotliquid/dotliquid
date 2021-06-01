using System.Resources;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// When a non-boolean data type is used in a boolean context (such as a conditional tag),
    /// Liquid decides whether to evaluate it as true or false.
    /// Data types that return true by default are called truthy.
    /// Data types that return false by default are called falsy.
    /// <see href="https://shopify.github.io/liquid/basics/truthy-and-falsy/"/>
    /// </summary>
    internal static class TruthyAndFalsy
    {
        /// <summary>
        /// All values in Liquid are Truthy except nil and false.
        /// </summary>
        /// <param name="any">any object</param>
        /// <returns></returns>
        public static bool IsTruthy(this object any) => !IsFalsy(any);

        /// <summary>
        /// The only values that are Falsy in Liquid are nil and false.
        /// </summary>
        /// <param name="any">any object</param>
        /// <returns></returns>
        public static bool IsFalsy(this object any)
        {
            return any == null
                || (any is bool _bool && _bool == false)
                || (any is string _string && "false".Equals(_string.ToLowerInvariant()));
        }
    }
}
