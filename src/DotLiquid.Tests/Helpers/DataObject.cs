using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Tests.Helpers
{
    [LiquidType("PropAllowed")]
    public class DataObject
    {
        public string PropAllowed { get; set; }
        public string PropDisallowed { get; set; }
    }
}
