namespace DotLiquid
{
    /// <summary>
    /// Strict Liquid Syntax Compatibility Flag
    /// </summary>
    public enum SyntaxCompatibility
    {
        /// <summary>
        /// Behavior as of DotLiquid 2.0
        /// </summary>
        DotLiquid20 = 200,

        /// <summary>
        /// Behavior as of DotLiquid 2.1
        /// </summary>
        DotLiquid21 = 210,

        /// <summary>
        /// Behavior as of DotLiquid 2.2
        /// </summary>
        DotLiquid22 = 220,

        /// <summary>
        /// Behavior as of DotLiquid 2.2a
        /// </summary>
        DotLiquid22a = 221,

        /// <summary>
        /// Behavior as of DotLiquid 2.4
        /// </summary>
        DotLiquid24 = 240,

        /// <summary>
        /// Equivalent to the latest version of DotLiquid
        /// </summary>
        DotLiquidLatest = DotLiquid24,
    }
}
