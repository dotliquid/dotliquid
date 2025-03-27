using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Tests.Helpers
{
    public class DataObjectRegistered
    {
        static DataObjectRegistered()
        {
            Template.RegisterSafeType(typeof(DataObjectRegistered), new[] { "PropAllowed" });
        }
        public string PropAllowed { get; set; }
        public string PropDisallowed { get; set; }
    }
}
