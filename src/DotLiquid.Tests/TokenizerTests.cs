using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class TokenizerTests
    {
        [Test]
        public void TestGetVariableEnumerator()
        {
            Assert.False(Tokenizer.GetVariableEnumerator(null).MoveNext());
            Assert.False(Tokenizer.GetVariableEnumerator("").MoveNext());
        }

        ///TODO: TestGetVariableEnumerator_VariableNotTerminatedException

        [Test]
        public void TestGetVariableEnumerator_StripLeadingParens()
        {
            var tokens = Tokenizer.GetVariableEnumerator("(b == 'bar'");
            Assert.True(tokens.MoveNext());
            Assert.AreEqual("b == 'bar'", tokens.Current);
            Assert.False(tokens.MoveNext());
        }

        [Test]
        public void TestGetVariableEnumerator_StripTrailingParens()
        {
            var tokens = Tokenizer.GetVariableEnumerator("c == 'baz')");
            Assert.True(tokens.MoveNext());
            Assert.AreEqual("c == 'baz'", tokens.Current);
            Assert.False(tokens.MoveNext());
        }
    }
}