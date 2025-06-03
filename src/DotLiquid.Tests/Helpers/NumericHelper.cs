using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Tests.Helpers
{
    public static class NumericHelper
    {
        /// <summary>
        /// Gets all combinations of numeric types.
        /// </summary>
        /// <returns>An enumeration of Tuple&lt;Type, Type&gt; containing pairs of different types.</returns>
        /// <remarks>Pairs are unorderd - that is, if (a, b) is included then (b, a) will not be included.</remarks>
        public static IEnumerable<(Type, Type)> GetNumericTypeCombinations()
        {
            var testTypes = new HashSet<Type> {
                typeof(decimal), typeof(double), typeof(float),
                typeof(long), typeof(ulong),
                typeof(int), typeof(uint),
                typeof(short), typeof(ushort),
                typeof(sbyte), typeof(byte), 
            };
            var testAgainst = new HashSet<Type>(testTypes.ToArray());

            foreach (var t1 in testTypes)
            {
                foreach (var t2 in testAgainst)
                {
                    yield return (t1, t2);
                }
                testAgainst.Remove(t1); // All combinations are tested, no need to test other objects against it.
            }
        }
    }
}
