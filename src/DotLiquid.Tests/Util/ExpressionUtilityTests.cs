using System;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using DotLiquid.Util;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class ExpressionUtilityTests
    {
        [Test]
        public void CreateExpression_AddsTwoIntegers()
        {
            var expression = ExpressionUtility.CreateExpression(Expression.AddChecked, typeof(int), typeof(int));
            Assert.That(expression.DynamicInvoke(2, 3), Is.EqualTo(5));
        }

        [Test]
        public void CreateExpression_AddsIntAndDouble()
        {
            var expression = ExpressionUtility.CreateExpression(Expression.AddChecked, typeof(int), typeof(double));
            Assert.That(expression.DynamicInvoke(2, 3.5), Is.EqualTo(5.5));
        }

        [Test]
        public void CreateExpression_AddsIntAndNull()
        {
            var expression = ExpressionUtility.CreateExpression(Expression.AddChecked, typeof(int), typeof(int));
            Assert.That(expression.DynamicInvoke(2, null), Is.EqualTo(2));
        }

        [Test]
        public void CreateExpression_SubtractsDecimals()
        {
            var expression = ExpressionUtility.CreateExpression(Expression.SubtractChecked, typeof(decimal), typeof(decimal));
            Assert.That(expression.DynamicInvoke(3.5m, 2m), Is.EqualTo(1.5m));
        }

        [Test]
        public void CreateExpression_ThrowsOnInvalidArgumentTypes()
        {
            var expression = ExpressionUtility.CreateExpression(Expression.AddChecked, typeof(short), typeof(short));
            Assert.Throws<ArgumentException>(() => expression.DynamicInvoke((int)2, (int)3));
        }

        [Test]
        public void CreateExpression_ThrowsOnInvalidOperation()
        {
            // byte + byte is not supported by Expression.AddChecked
            var expression = ExpressionUtility.CreateExpression(Expression.AddChecked, typeof(byte), typeof(byte));
            var ex = Assert.Throws<TargetInvocationException>(() => expression.DynamicInvoke((byte)2, (byte)3));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void CreateExpression_ThrowsOnNonNumericTypes()
        {
            var expression = ExpressionUtility.CreateExpression(Expression.AddChecked, typeof(string), typeof(string));
            var ex = Assert.Throws<TargetInvocationException>(() => expression.DynamicInvoke("a", "b"));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void CreateExpression_DivideByZero_ReturnsThrowingDelegate()
        {
            var expression = ExpressionUtility.CreateExpression(Expression.Divide, typeof(int), typeof(int));
            var ex = Assert.Throws<TargetInvocationException>(() => expression.DynamicInvoke(1, 0));
            Assert.That(ex.InnerException, Is.TypeOf<DivideByZeroException>());
        }
    }
}