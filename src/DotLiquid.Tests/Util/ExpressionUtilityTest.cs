using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class ExpressionUtilityTest
    {
        private Dictionary<Type, (double, double)> typeLimits;
        [SetUp]
        public void Setup()
        {
            typeLimits = new Dictionary<Type, (double, double)>()
            {
                { typeof(decimal), (Convert.ToDouble(decimal.MaxValue), Convert.ToDouble(decimal.MinValue) ) },
                { typeof(double), (double.MaxValue, double.MinValue ) },
                { typeof(float), (Convert.ToDouble(float.MaxValue), Convert.ToDouble(float.MinValue) ) },
                { typeof(int), (Convert.ToDouble(int.MaxValue), Convert.ToDouble(int.MinValue) ) },
                { typeof(uint), (Convert.ToDouble(uint.MaxValue), Convert.ToDouble(uint.MinValue) ) },
                { typeof(long), (Convert.ToDouble(long.MaxValue), Convert.ToDouble(long.MinValue) ) },
                { typeof(ulong), (Convert.ToDouble(ulong.MaxValue), Convert.ToDouble(ulong.MinValue) ) },
                { typeof(short), (Convert.ToDouble(short.MaxValue), Convert.ToDouble(short.MinValue) ) },
                { typeof(ushort), (Convert.ToDouble(ushort.MaxValue), Convert.ToDouble(ushort.MinValue) ) },
                { typeof(byte), (Convert.ToDouble(byte.MaxValue), Convert.ToDouble(byte.MinValue) ) },
                { typeof(sbyte), (Convert.ToDouble(sbyte.MaxValue), Convert.ToDouble(sbyte.MinValue) ) }
            };
        }

        public static IEnumerable<(Type, Type)> GetNumericCombinations()
        {
            var testTypes = new HashSet<Type> { typeof(decimal), typeof(double), typeof(float), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(byte), typeof(sbyte) };
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

        [Test, TestCaseSource("GetNumericCombinations")]
        public void TestNumericCombinationsResultInUpgrade(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var result = DotLiquid.Util.ExpressionUtility.BinaryNumericResultType(t1, t2);
            Assert.That(result, Is.Not.Null);
            Assert.That(DotLiquid.Util.ExpressionUtility.BinaryNumericResultType(t2, t1), Is.EqualTo(result));
            Assert.That(typeLimits[result].Item1 >= typeLimits[t1].Item1, Is.True);
            Assert.That(typeLimits[result].Item1 >= typeLimits[t2].Item1, Is.True);
            Assert.That(typeLimits[result].Item2 <= typeLimits[t1].Item2, Is.True);
            Assert.That(typeLimits[result].Item2 <= typeLimits[t1].Item2, Is.True);
        }

        /// <summary>
        /// Repeated calls to CreateExpression with the same operation/operand types must reuse a cached
        /// compiled delegate instead of re-compiling an Expression.Lambda every time. This guards against
        /// regressions to an unbounded compile-per-call cost (e.g. when a maths filter is applied within a loop).
        /// </summary>
        [Test]
        public void TestCreateExpressionCachesCompiledDelegate()
        {
            Func<Expression, Expression, BinaryExpression> operation = Expression.AddChecked;

            var first = DotLiquid.Util.ExpressionUtility.CreateExpression(operation, typeof(int), typeof(int));
            var second = DotLiquid.Util.ExpressionUtility.CreateExpression(operation, typeof(int), typeof(int));

            Assert.That(second, Is.SameAs(first), "Expected the same compiled delegate instance to be returned for repeated calls with identical operation/operand types.");

            // A different operand type combination must still produce a working, independently cached delegate.
            var third = DotLiquid.Util.ExpressionUtility.CreateExpression(operation, typeof(long), typeof(long));
            Assert.That(third, Is.Not.SameAs(first));
        }
    }
}
