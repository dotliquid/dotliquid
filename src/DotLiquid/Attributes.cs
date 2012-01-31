using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotLiquid
{
    /// <summary>
    /// Specifies the type is safe to be rendered by DotLiquid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LiquidTypeAttribute : Attribute 
    {
        /// <summary>
        /// Specifies that only members declared at the level of the supplied type's hierarchy should be considered. Inherited members are not considered.
        /// </summary>
        public bool DeclaredOnly { get; set; }
    }
}
