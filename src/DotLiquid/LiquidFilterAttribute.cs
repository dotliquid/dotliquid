using System;
using System.Collections.Generic;
using System.Text;

namespace DotLiquid
{
    /// <summary>
    /// Represents an attribute that can be used to specify the name and syntax compatibility of a Liquid filter.
    /// </summary>
    public class LiquidFilterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the Liquid filter. Defaults to the method name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alternative name of the Liquid filter.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the minimum syntax compatibility version for the Liquid filter.
        /// </summary>
        public SyntaxCompatibility MinVersion { get; set; } = SyntaxCompatibility.DotLiquid20;

        /// <summary>
        /// Gets or sets the maximum syntax compatibility version for the Liquid filter.
        /// </summary>
        public SyntaxCompatibility MaxVersion { get; set; } = SyntaxCompatibility.DotLiquidLatest;
    }
}
