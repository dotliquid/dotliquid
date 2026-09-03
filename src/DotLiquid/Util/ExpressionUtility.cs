using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotLiquid.Util
{
    /// <summary>
    /// Some of this code was taken from http://www.yoda.arachsys.com/csharp/miscutil/usage/genericoperators.html.
    /// General purpose Expression utilities
    /// </summary>
    public static class ExpressionUtility
    {
        private static readonly Dictionary<Type, Type[]> NumericTypePromotions;

        /// <summary>
        /// Cache of compiled binary operation delegates, keyed by operation method, left operand type and right operand type,
        /// avoiding recompiling an Expression.Lambda on every filter call. Uses nested ConditionalWeakTables keyed on the
        /// operand Types (rather than a flat dictionary) so entries don't pin arbitrary/dynamic operand Types in memory forever.
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, ConditionalWeakTable<Type, ConditionalWeakTable<Type, Delegate>>> CompiledExpressionCache
            = new ConcurrentDictionary<MethodInfo, ConditionalWeakTable<Type, ConditionalWeakTable<Type, Delegate>>>();

        static ExpressionUtility()
        {
            NumericTypePromotions = new Dictionary<Type, Type[]>();

            void Add(Type key, params Type[] types) => NumericTypePromotions[key] = types;
            // Using the promotion table at
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/conversion-tables

            Add(typeof(Byte), typeof(UInt16), typeof(Int16), typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(SByte), typeof(Int16), typeof(Int32), typeof(Int64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(Int16), typeof(Int32), typeof(Int64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(UInt16), typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(Char), typeof(UInt16), typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(Int32), typeof(Int64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(UInt32), typeof(Int64), typeof(UInt64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(Int64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(UInt64), typeof(Decimal), typeof(Single), typeof(double));
            Add(typeof(Single), typeof(Double));
            Add(typeof(Decimal), typeof(Single), typeof(Double));
            Add(typeof(Double));

        }

        /// <summary>
        /// Perform the implicit conversions as set out in the C# spec docs at
        /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/conversion-tables
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        internal static Type BinaryNumericResultType(Type left, Type right)
        {
            if (left == right)
                return left;

            if (!NumericTypePromotions.ContainsKey(left))
                throw new System.ArgumentException("Argument is not numeric", nameof(left));
            if (!NumericTypePromotions.ContainsKey(right))
                throw new System.ArgumentException("Argument is not numeric", nameof(right));

            // Test left to right promotion
            if (NumericTypePromotions[right].Contains(left))
                return left;
            if (NumericTypePromotions[left].Contains(right))
                return right;
            return NumericTypePromotions[right].First(p => NumericTypePromotions[left].Contains(p));
        }

        private static void Cast(Expression lhs, Expression rhs, Type leftType, Type rightType, Type resultType, out Expression castLhs, out Expression castRhs)
        {
            castLhs = leftType == resultType ? lhs : (Expression)Expression.Convert(lhs, resultType);
            castRhs = rightType == resultType ? rhs : (Expression)Expression.Convert(rhs, resultType);
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
            // Level 1: left-type cache for this operation (e.g. Expression.AddChecked, a static framework method).
            var delegatesByLeftType = CompiledExpressionCache.GetOrAdd(
                body.GetMethodInfo(),
                _ => new ConditionalWeakTable<Type, ConditionalWeakTable<Type, Delegate>>());

            // Level 2: right-type cache for this left operand type.
            var delegatesByRightType = delegatesByLeftType.GetValue(
                leftType,
                _ => new ConditionalWeakTable<Type, Delegate>());

            // Level 3: compiled delegate for this (operation, leftType, rightType), compiling on a miss.
            return delegatesByRightType.GetValue(
                rightType,
                _ => CompileExpression(body, leftType, rightType));
        }

        private static Delegate CompileExpression
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
