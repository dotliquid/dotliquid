using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class NumericConverterTests
    {
        [Test]
        [TestCaseSource(nameof(GoodTestCaseSource))]
        public void TestCoerceToNumericType(string input, IFormatProvider formatProvider, object expectedValue)
        {
            object coercedValue = input.CoerceToNumericType(formatProvider, null);

            Assert.That(coercedValue, Is.Not.Null);
            Assert.That(coercedValue, Is.EqualTo(expectedValue));
            Assert.That(coercedValue, Is.TypeOf(expectedValue.GetType()));
        }

        [Test]
        [TestCaseSource(nameof(ErrorTestCaseSource))]
        public void TestCoerceToNumericTypeErrors(string input, IFormatProvider formatProvider)
        {
            object defaultValue = new object();
            object coercedValue = input.CoerceToNumericType(formatProvider, defaultValue);

            Assert.That(coercedValue, Is.EqualTo(defaultValue));
        }

        [Test]
        [TestCaseSource(nameof(GoodTestCaseSource))]
        public void TestTryParseToNumericType(string input, IFormatProvider formatProvider, object expectedValue)
        {
            bool converted = input.TryParseToNumericType(formatProvider, out object convertedValue);
            Assert.That(converted, Is.True);
            Assert.That(convertedValue, Is.EqualTo(expectedValue));
            Assert.That(convertedValue, Is.TypeOf(expectedValue.GetType()));
        }

        [Test]
        [TestCaseSource(nameof(ErrorTestCaseSource))]
        public void TestTryParseToNumericTypeErrors(string input, IFormatProvider formatProvider)
        {
            bool converted = input.TryParseToNumericType(formatProvider, out object convertedValue);
            Assert.That(converted, Is.False, $"convertedValue: {convertedValue}");
        }

        static IEnumerable GoodTestCaseSource()
        {
            IFormatProvider invariantFormatProvider = CultureInfo.InvariantCulture;
            IFormatProvider frenchFormatProvider = new CultureInfo("fr-FR");

            // Int32
            yield return new object[] { "0", null, 0 };
            yield return new object[] { "0", invariantFormatProvider, 0 };
            yield return new object[] { "123", invariantFormatProvider, 123 };
            yield return new object[] { "-123", invariantFormatProvider, -123 };

            // Int32 with thousands separator
            // Note: The French thousands separator is a thin space (U+202F) 
            yield return new object[] { "12,567", invariantFormatProvider, 12567 };
            yield return new object[] { "12 567", frenchFormatProvider, 12567 };

            // Int64
            yield return new object[] { $"{Int64.MaxValue}", null, Int64.MaxValue };
            yield return new object[] { $"{Int64.MaxValue}", invariantFormatProvider, Int64.MaxValue };
            yield return new object[] { $"{Int64.MinValue}", invariantFormatProvider, Int64.MinValue };

            // Int64 with thousands separator
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0}", Int64.MaxValue), invariantFormatProvider, Int64.MaxValue };
            yield return new object[] { String.Format(frenchFormatProvider, "{0:#,##0}", Int64.MaxValue), frenchFormatProvider, Int64.MaxValue };
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0}", Int64.MaxValue), frenchFormatProvider, Int64.MaxValue };

            // Decimal
            yield return new object[] { "0.0", null, 0m };
            yield return new object[] { "12.0", null, 12m };
            yield return new object[] { "0.0", invariantFormatProvider, 0m };
            yield return new object[] { "12.0", invariantFormatProvider, 12m };
            yield return new object[] { "12.567", invariantFormatProvider, 12.567m };
            yield return new object[] { "-12.0", invariantFormatProvider, -12m };
            yield return new object[] { "-12.567", invariantFormatProvider, -12.567m };
            yield return new object[] { $"{Decimal.MaxValue:F}", invariantFormatProvider, Decimal.MaxValue };
            yield return new object[] { $"{Decimal.MinValue:F}", invariantFormatProvider, Decimal.MinValue };
            yield return new object[] { "12345678901234567890123456.789", invariantFormatProvider, 12345678901234567890123456.789m };
            yield return new object[] { "12345678901234567890123456", invariantFormatProvider, 12345678901234567890123456m };
            yield return new object[] { "0.30000000000000004", invariantFormatProvider, 0.30000000000000004m };

            yield return new object[] { "12,0", frenchFormatProvider, 12m };
            yield return new object[] { "12,567", frenchFormatProvider, 12.567m };
            yield return new object[] { "-12,0", frenchFormatProvider, -12m };
            yield return new object[] { "-12,567", frenchFormatProvider, -12.567m };

            // Decimal with thousands separator
            // Note: The French thousands separator is a thin space (U+202F) 
            yield return new object[] { "12,567.10", invariantFormatProvider, 12567.1m };
            yield return new object[] { "12 567,10", frenchFormatProvider, 12567.1m };
            yield return new object[] { "12,567.10", frenchFormatProvider, 12567.1m };

            // Double
            double largePositiveValue = double.Parse("1e203");
            double largeNegativeValue = double.Parse("-1e203");
            yield return new object[] { $"{largePositiveValue:F}", null, largePositiveValue };
            yield return new object[] { $"{largePositiveValue:F}", invariantFormatProvider, largePositiveValue };
            yield return new object[] { $"{largeNegativeValue:F}", invariantFormatProvider, largeNegativeValue};

            // Double with thousands separator
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0.00}", largePositiveValue), invariantFormatProvider, largePositiveValue };
            yield return new object[] { String.Format(frenchFormatProvider, "{0:#,##0.00}", largePositiveValue), frenchFormatProvider, largePositiveValue };
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0.00}", largePositiveValue), frenchFormatProvider, largePositiveValue };
        }

        static IEnumerable ErrorTestCaseSource()
        {
            IFormatProvider invariantFormatProvider = CultureInfo.InvariantCulture;
            IFormatProvider frenchFormatProvider = new CultureInfo("fr-FR");

            yield return new object[] { null, null };
            yield return new object[] { null, invariantFormatProvider };
            yield return new object[] { string.Empty, invariantFormatProvider };
            yield return new object[] { "banana", invariantFormatProvider };
        }
    }
    }
