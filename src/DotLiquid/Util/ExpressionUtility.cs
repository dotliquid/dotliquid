using System;
using System.Linq.Expressions;

namespace DotLiquid.Util
{
	/// <summary>
	/// Some of this code was taken from http://www.yoda.arachsys.com/csharp/miscutil/usage/genericoperators.html.
	/// General purpose Expression utilities
	/// </summary>
	public static class ExpressionUtility
	{
		/// <summary>
		/// Create a function delegate representing a binary operation
		/// </summary>
		/// <param name="body">Body factory</param>
		/// <param name="leftType"></param>
		/// <param name="rightType"></param>
		/// <param name="resultType"></param>
		/// <param name="castArgsToResultOnFailure">
		/// If no matching operation is possible, attempt to convert
		/// TArg1 and TArg2 to TResult for a match? For example, there is no
		/// "decimal operator /(decimal, int)", but by converting TArg2 (int) to
		/// TResult (decimal) a match is found.
		/// </param>
		/// <returns>Compiled function delegate</returns>
		public static Delegate CreateExpression(Func<Expression, Expression, BinaryExpression> body,
			Type leftType, Type rightType, Type resultType, bool castArgsToResultOnFailure)
		{
			ParameterExpression lhs = Expression.Parameter(leftType, "lhs");
			ParameterExpression rhs = Expression.Parameter(rightType, "rhs");
			try
			{
				try
				{
					return Expression.Lambda(body(lhs, rhs), lhs, rhs).Compile();
				}
				catch (InvalidOperationException)
				{
					if (castArgsToResultOnFailure && !( // if we show retry                                                        
						leftType == resultType && // and the args aren't
							rightType == resultType))
					{
						// already "TValue, TValue, TValue"...
						// convert both lhs and rhs to TResult (as appropriate)
						Expression castLhs = leftType == resultType ? lhs : (Expression) Expression.Convert(lhs, resultType);
						Expression castRhs = rightType == resultType ? rhs : (Expression) Expression.Convert(rhs, resultType);

						return Expression.Lambda(body(castLhs, castRhs), lhs, rhs).Compile();
					}
					throw;
				}
			}
			catch (Exception ex)
			{
				string msg = ex.Message; // avoid capture of ex itself
				return (Action)(delegate { throw new InvalidOperationException(msg); });
			}
		}
	}
}