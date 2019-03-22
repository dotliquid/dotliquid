using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DotLiquid.Util
{
    /// <summary>
    /// Some of this code was taken from http://www.yoda.arachsys.com/csharp/miscutil/usage/genericoperators.html.
    /// General purpose Expression utilities
    /// </summary>
    public static class ExpressionUtility
    {
        private static readonly Dictionary<Type, HashSet<Type>> NumericTypePromotions = new Dictionary<Type, HashSet<Type>> {
            {typeof(Byte), new HashSet<Type> {typeof(UInt16), typeof(Int16), typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Single), typeof(Double), typeof(Decimal)}},
            {typeof(SByte), new HashSet<Type> {typeof(Int16), typeof(Int32), typeof(Int64), typeof(Single), typeof(Double), typeof(Decimal)}},
            {typeof(Int16), new HashSet<Type> {typeof(Int32), typeof(Int64), typeof(Single), typeof(Double), typeof(Decimal)}},
            {typeof(UInt16), new HashSet<Type> {typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Single), typeof(Double), typeof(Decimal)}},
            {typeof(Char), new HashSet<Type> {typeof(UInt16), typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Single), typeof(Double), typeof(Decimal)}},
            {typeof(Int32), new HashSet<Type> {typeof(Int64), typeof(Double), typeof(Decimal), typeof(Single)}},
            {typeof(UInt32), new HashSet<Type> {typeof(Int64), typeof(UInt64), typeof(Double), typeof(Decimal), typeof(Single)}},
            {typeof(Int64), new HashSet<Type> {typeof(Decimal), typeof(Single), typeof(double)}},
            {typeof(UInt64), new HashSet<Type> {typeof(Decimal), typeof(Single), typeof(double)}},
            {typeof(Single), new HashSet<Type> {typeof(Double)}},
            {typeof(Decimal), new HashSet<Type> {typeof(Single), typeof(Double)}},
            {typeof(Double), new HashSet<Type>()}
        };

        /// <summary>
        /// Perform the implicit conversions as set out in the C# spec docs at
        /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/conversion-tables
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        static Type BinaryNumericResultType(Type left, Type right)
        {
            if (left == right)
                return left;

            if (!NumericTypePromotions.ContainsKey(left))
                throw new System.ArgumentException("Argument is not numeric", nameof(left));
            if (!NumericTypePromotions.ContainsKey(right))
                throw new System.ArgumentException("Argument is not numeric", nameof(right));

            // Test left to right promotion
            return NumericTypePromotions[right].Contains(left) ? right : left;
        }

        private static (Expression left, Expression right) Cast(Expression lhs, Expression rhs,Type leftType, Type rightType, Type resultType)
        {
            var castLhs = leftType == resultType ? lhs : Expression.Convert(lhs, resultType);
            var castRhs = rightType == resultType ? rhs : Expression.Convert(rhs, resultType);
            return (castLhs, castRhs);
        }

        /// <summary>
        /// Create a function delegate representing a binary operation
        /// </summary>
        /// <param name="body">Body factory</param>
        /// <param name="leftType"></param>
        /// <param name="rightType"></param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <returns>Compiled function delegate</returns>
        public static Delegate CreateExpression
            (Func<Expression, Expression, BinaryExpression> body
             , Type leftType
             , Type rightType)
        {
            var lhs = Expression.Parameter(leftType, "lhs");
            var rhs = Expression.Parameter(rightType, "rhs");
            try
            {
                try
                {
                    var resultType = BinaryNumericResultType( leftType, rightType );
                    var (castLhs, castRhs) = Cast( lhs, rhs, leftType, rightType, resultType );
                    return Expression.Lambda(body(castLhs, castRhs), lhs, rhs).Compile();
                }
                catch (InvalidOperationException)
                {
                    try
                    {
                        var resultType = leftType;
                        var (castLhs, castRhs) = Cast( lhs, rhs, leftType, rightType, resultType );
                        return Expression.Lambda( body( castLhs, castRhs ), lhs, rhs ).Compile();
                    }
                    catch (InvalidOperationException)
                    {
                        var resultType = rightType;
                        var (castLhs, castRhs) = Cast( lhs, rhs, leftType, rightType, resultType );
                        return Expression.Lambda( body( castLhs, castRhs ), lhs, rhs ).Compile();
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message; // avoid capture of ex itself
                return (Action)(() => throw new InvalidOperationException(msg));
            }
        }
    }
}
