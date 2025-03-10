namespace DotLiquid.NamingConventions
{
    /// <summary>
    /// The <see cref="DotLiquid.NamingConventions.INamingConvention">INamingConvention</see> interface
    /// provides a way to customize how names are compared, modified, and evaluated in Liquid.
    /// </summary>
    public interface INamingConvention
    {
        /// <summary>
        /// Gets the string comparer used for case sensitivity and culture information
        /// </summary>
        System.StringComparer StringComparer { get; }

        /// <summary>
        /// Returns a modified version of the input name according to the naming convention.
        /// </summary>
        /// <param name="name">The original name to be modified.</param>
        /// <returns>A string representing the modified name.</returns>
        string GetMemberName(string name);

        /// <summary>
        /// Compares two operator strings to determine if they are equal according to the naming convention.
        /// </summary>
        /// <param name="testedOperator">The operator string to be compared.</param>
        /// <param name="referenceOperator">The reference operator string.</param>
        /// <returns></returns>
        bool OperatorEquals(string testedOperator, string referenceOperator);
    }
}
