using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class StringExtensionMethodsTests
    {
        [Test]
        [TestCaseSource(nameof(GoodTestCaseSource))]
        public void TestTryParseToNumericType(string input, IFormatProvider formatProvider, object expectedValue)
        {
            bool converted = input.TryParseToNumericType(formatProvider, out object convertedValue);
            Assert.That(converted, Is.True);
            Assert.That(convertedValue, Is.TypeOf(expectedValue.GetType()));
            Assert.That(convertedValue, Is.EqualTo(expectedValue));
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

            // Int64
            yield return new object[] { $"{Int64.MaxValue}", null, Int64.MaxValue };
            yield return new object[] { $"{Int64.MaxValue}", invariantFormatProvider, Int64.MaxValue };
            yield return new object[] { $"{Int64.MinValue}", invariantFormatProvider, Int64.MinValue };

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

            yield return new object[] { "12,0", frenchFormatProvider, 12m };
            yield return new object[] { "12,567", frenchFormatProvider, 12.567m };
            yield return new object[] { "-12,0", frenchFormatProvider, -12m };
            yield return new object[] { "-12,567", frenchFormatProvider, -12.567m };
            yield return new object[] { "12.567", frenchFormatProvider, 12.567m };

            // Double
            double largePositiveValue = double.Parse("1e203");
            double largeNegativeValue = double.Parse("-1e203");
            yield return new object[] { $"{largePositiveValue:F}", null, largePositiveValue };
            yield return new object[] { $"{largePositiveValue:F}", invariantFormatProvider, largePositiveValue };
            yield return new object[] { $"{largeNegativeValue:F}", invariantFormatProvider, largeNegativeValue};
        }

        static IEnumerable ErrorTestCaseSource()
        {
            IFormatProvider invariantFormatProvider = CultureInfo.InvariantCulture;

            yield return new object[] { null, null };
            yield return new object[] { null, invariantFormatProvider };
            yield return new object[] { string.Empty, invariantFormatProvider };
            yield return new object[] { "banana", invariantFormatProvider };
            // yield return new object[] { "-12,567", invariantFormatProvider }; // This returns -12567m
        }
    }
    }
