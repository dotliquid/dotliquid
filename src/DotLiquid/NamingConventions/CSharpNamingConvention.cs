using System;

namespace DotLiquid.NamingConventions
{
    public class CSharpNamingConvention : INamingConvention
    {
        /// <inheritdoc />
        public StringComparer StringComparer
        {
            get { return StringComparer.Ordinal; }
        }

        /// <inheritdoc />
        public string GetMemberName(string name)
        {
            return name;
        }

        /// <inheritdoc />
        public bool OperatorEquals(string testedOperator, string referenceOperator)
        {
            return UpperFirstLetter(testedOperator).Equals(referenceOperator)
                    || LowerFirstLetter(testedOperator).Equals(referenceOperator);
        }

        private static string UpperFirstLetter(string word)
        {
            return char.ToUpperInvariant(word[0]) + word.Substring(1);
        }

        private static string LowerFirstLetter(string word)
        {
            return char.ToLowerInvariant(word[0]) + word.Substring(1);
        }
    }
}
