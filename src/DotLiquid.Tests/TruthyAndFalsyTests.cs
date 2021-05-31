using System;
using NUnit.Framework;

namespace DotLiquid.Tests
{

    [TestFixture]
    public class TruthyAndFalsyTests
    {
        [Test]
        public void TestNullObject()
        {
            object nil = null;
            Assert.False(nil.IsTruthy());
            Assert.True(nil.IsFalsy());
        }

        [Test]
        public void TestIsTruthy()
        {
            Assert.False(TruthyAndFalsy.IsTruthy(false));
            Assert.False(TruthyAndFalsy.IsTruthy(null));
            Assert.False(TruthyAndFalsy.IsTruthy("false"));
            Assert.False(TruthyAndFalsy.IsTruthy("FALSE"));
            Assert.False(TruthyAndFalsy.IsTruthy("FaLSe"));

            Assert.True(TruthyAndFalsy.IsTruthy(true));
            Assert.True(TruthyAndFalsy.IsTruthy("testing"));
            Assert.True(TruthyAndFalsy.IsTruthy("true"));
            Assert.True(TruthyAndFalsy.IsTruthy("TRUE"));
            Assert.True(TruthyAndFalsy.IsTruthy("TrUe"));
            Assert.True(TruthyAndFalsy.IsTruthy(0));
            Assert.True(TruthyAndFalsy.IsTruthy(1));
            Assert.True(TruthyAndFalsy.IsTruthy(9.9f));
            Assert.True(TruthyAndFalsy.IsTruthy(new[] { "cat", "dog" }));
            Assert.True(TruthyAndFalsy.IsTruthy(Array.Empty<object>()));
        }

        [Test]
        public void TestIsFalsy()
        {
            Assert.True(TruthyAndFalsy.IsFalsy(false));
            Assert.True(TruthyAndFalsy.IsFalsy(null));
            Assert.True(TruthyAndFalsy.IsFalsy("false"));
            Assert.True(TruthyAndFalsy.IsFalsy("FALSE"));
            Assert.True(TruthyAndFalsy.IsFalsy("FaLSe"));

            Assert.False(TruthyAndFalsy.IsFalsy(true));
            Assert.False(TruthyAndFalsy.IsFalsy("testing"));
            Assert.False(TruthyAndFalsy.IsFalsy("true"));
            Assert.False(TruthyAndFalsy.IsFalsy("TRUE"));
            Assert.False(TruthyAndFalsy.IsFalsy("TrUe"));
            Assert.False(TruthyAndFalsy.IsFalsy(0));
            Assert.False(TruthyAndFalsy.IsFalsy(1));
            Assert.False(TruthyAndFalsy.IsFalsy(9.9f));
            Assert.False(TruthyAndFalsy.IsFalsy(new[] { "cat", "dog" }));
            Assert.False(TruthyAndFalsy.IsFalsy(Array.Empty<object>()));
        }
    }
}
