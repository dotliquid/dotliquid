using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotLiquid
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LiquidTypeAttribute : Attribute 
    {
        public bool DeclaredOnly { get; set; }
    }
}
