using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotLiquid.Tests.Helpers;
using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
    [TestFixture]
    public class NumericConverterTests
    {
        private static Dictionary<Type, (double, double)> TypeLimits = new Dictionary<Type, (double, double)>()
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

        [Test]
        public void TestCoerceToReal()
        {
            Assert.That(15.CoerceToReal(CultureInfo.InvariantCulture, 0), Is.EqualTo(15m).And.TypeOf(typeof(decimal)));
            Assert.That(15m.CoerceToReal(CultureInfo.InvariantCulture, 0), Is.EqualTo(15m).And.TypeOf(typeof(decimal)));
            Assert.That(15.0f.CoerceToReal(CultureInfo.InvariantCulture, 0), Is.EqualTo(15).And.TypeOf(typeof(double)));
            Assert.That(15.0.CoerceToReal(CultureInfo.InvariantCulture, 0), Is.EqualTo(15).And.TypeOf(typeof(double)));
            Assert.That("15".CoerceToReal(CultureInfo.InvariantCulture, 0), Is.EqualTo(15m).And.TypeOf(typeof(decimal)));
            Assert.That("-15".CoerceToReal(CultureInfo.InvariantCulture, 0), Is.EqualTo(-15m).And.TypeOf(typeof(decimal)));
            Assert.That("15.0".CoerceToReal(CultureInfo.InvariantCulture, 0), Is.EqualTo(15m).And.TypeOf(typeof(decimal)));
        }

        [Test]
        public void TestCoerceToRealOverflow()
        {
            double largeValue = double.Parse("1e203");
            string largePositiveValue = $"{largeValue:F}";
            Assert.That(largePositiveValue.CoerceToReal(CultureInfo.InvariantCulture, 15m), Is.EqualTo(largeValue).And.TypeOf(typeof(double)));
        }

        [Test]
        [TestCaseSource(nameof(GoodTestCaseSource))]
        public void TestCoerceToNumericType(object input, IFormatProvider formatProvider, object expectedValue)
        {
            object coercedValue = input.CoerceToNumericType(formatProvider, null);

            Assert.That(coercedValue, Is.Not.Null);
            Assert.That(coercedValue, Is.EqualTo(expectedValue));
            Assert.That(coercedValue, Is.TypeOf(expectedValue.GetType()));
        }

        [Test]
        [TestCaseSource(nameof(ErrorTestCaseSource))]
        public void TestCoerceToNumericTypeErrors(object input, IFormatProvider formatProvider)
        {
            object defaultValue = new object();
            object coercedValue = input.CoerceToNumericType(formatProvider, defaultValue);

            Assert.That(coercedValue, Is.EqualTo(defaultValue));
        }

        [Test]
        [TestCaseSource(nameof(GoodTestCaseSource))]
        public void TestTryCoerceToNumericType(object input, IFormatProvider formatProvider, object expectedValue)
        {
            bool converted = input.TryCoerceToNumericType(formatProvider, out object convertedValue);
            Assert.That(converted, Is.True);
            Assert.That(convertedValue, Is.EqualTo(expectedValue));
            Assert.That(convertedValue, Is.TypeOf(expectedValue.GetType()));
        }

        [Test]
        [TestCaseSource(nameof(ErrorTestCaseSource))]
        public void TestTryCoerceToNumericTypeErrors(object input, IFormatProvider formatProvider)
        {
            bool converted = input.TryCoerceToNumericType(formatProvider, out object convertedValue);
            Assert.That(converted, Is.False, $"convertedValue: {convertedValue}");
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

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestNumericCombinationsResultInUpgrade(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var result = NumericConverter.GetBinaryResultType(t1, t2);
            Assert.That(result, Is.Not.Null);
            Assert.That(NumericConverter.GetBinaryResultType(t2, t1), Is.EqualTo(result));
            Assert.That(TypeLimits[result].Item1 >= TypeLimits[t1].Item1, Is.True);
            Assert.That(TypeLimits[result].Item1 >= TypeLimits[t2].Item1, Is.True);
            Assert.That(TypeLimits[result].Item2 <= TypeLimits[t1].Item2, Is.True);
            Assert.That(TypeLimits[result].Item2 <= TypeLimits[t1].Item2, Is.True);
        }

        private static IEnumerable GoodTestCaseSource()
        {
            IFormatProvider invariantFormatProvider = CultureInfo.InvariantCulture;
            IFormatProvider frenchFormatProvider = new CultureInfo("fr-FR");

            // Int32
            yield return new object[] { "0", null, 0 };
            yield return new object[] { "0", invariantFormatProvider, 0 };
            yield return new object[] { "123", invariantFormatProvider, 123 };
            yield return new object[] { "-123", invariantFormatProvider, -123 };

            // Int32 with thousands separator
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0}", 12567),
                invariantFormatProvider, 12567 };
            yield return new object[] { String.Format(frenchFormatProvider, "{0:#,##0}", 12567),
                frenchFormatProvider, 12567 };
            // Note: For fallback to happen, the number must be big enough to contain 2 separators,
            // otherwise it will be interprested as a French floating point number.
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0}", 12567890),
                frenchFormatProvider, 12567890 };

            // Int64
            yield return new object[] { $"{Int64.MaxValue}", null, Int64.MaxValue };
            yield return new object[] { $"{Int64.MaxValue}", invariantFormatProvider, Int64.MaxValue };
            yield return new object[] { $"{Int64.MinValue}", invariantFormatProvider, Int64.MinValue };

            // Int64 with thousands separator
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0}", Int64.MaxValue),
                invariantFormatProvider, Int64.MaxValue };
            yield return new object[] { String.Format(frenchFormatProvider, "{0:#,##0}", Int64.MaxValue),
                frenchFormatProvider, Int64.MaxValue };
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0}", Int64.MaxValue),
                frenchFormatProvider, Int64.MaxValue };

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
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0.00}", 12567.1m),
                invariantFormatProvider, 12567.1m };
            yield return new object[] { String.Format(frenchFormatProvider, "{0:#,##0.00}", 12567.1m),
                frenchFormatProvider, 12567.1m };
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0.00}", 12567.1m),
                frenchFormatProvider, 12567.1m };

            // Double
            double largePositiveValue = double.Parse("1e203");
            double largeNegativeValue = double.Parse("-1e203");
            yield return new object[] { $"{largePositiveValue:F}", null, largePositiveValue };
            yield return new object[] { $"{largePositiveValue:F}", invariantFormatProvider, largePositiveValue };
            yield return new object[] { $"{largeNegativeValue:F}", invariantFormatProvider, largeNegativeValue };

            // Double with thousands separator
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0.00}", largePositiveValue),
                invariantFormatProvider, largePositiveValue };
            yield return new object[] { String.Format(frenchFormatProvider, "{0:#,##0.00}", largePositiveValue),
                frenchFormatProvider, largePositiveValue };
            yield return new object[] { String.Format(invariantFormatProvider, "{0:#,##0.00}", largePositiveValue),
                frenchFormatProvider, largePositiveValue };
        }

        private static IEnumerable ErrorTestCaseSource()
        {
            IFormatProvider invariantFormatProvider = CultureInfo.InvariantCulture;

            yield return new object[] { null, null };
            yield return new object[] { null, invariantFormatProvider };
            yield return new object[] { string.Empty, invariantFormatProvider };
            yield return new object[] { "banana", invariantFormatProvider };
        }
    }
}
