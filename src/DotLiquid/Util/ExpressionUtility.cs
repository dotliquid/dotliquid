using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DotLiquid.Util
{
    /// <summary>
    /// Some of this code was taken from http://www.yoda.arachsys.com/csharp/miscutil/usage/genericoperators.html.
    /// General purpose Expression utilities
    /// </summary>
    public static class ExpressionUtility
    {
        private static void Cast(Expression lhs, Expression rhs, Type leftType, Type rightType, Type resultType, out Expression castLhs, out Expression castRhs)
        {
            castLhs = leftType == resultType ? lhs : Expression.Convert(lhs, resultType);
            castRhs = rightType == resultType ? rhs : Expression.Convert(rhs, resultType);
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
                    var resultType = NumericConverter.GetBinaryResultType(leftType, rightType);
                    Cast(lhs, rhs, leftType, rightType, resultType, out var castLhs, out var castRhs);
                    return Expression.Lambda(body(castLhs, castRhs), lhs, rhs).Compile();
                }
                catch (InvalidOperationException)
                {
                    try
                    {
                        var resultType = leftType;
                        Cast(lhs, rhs, leftType, rightType, resultType, out var castLhs, out var castRhs);
                        return Expression.Lambda( body( castLhs, castRhs ), lhs, rhs ).Compile();
                    }
                    catch (InvalidOperationException)
                    {
                        var resultType = rightType;
                        Cast(lhs, rhs, leftType, rightType, resultType, out var castLhs, out var castRhs);
                        return Expression.Lambda( body( castLhs, castRhs ), lhs, rhs ).Compile();
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message; // avoid capture of ex itself
                return Expression.Lambda(Expression.Throw(Expression.Constant(new InvalidOperationException(msg))), lhs, rhs).Compile();
            }
        }
    }
}
