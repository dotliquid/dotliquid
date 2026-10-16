using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotLiquid.Tests.Model
{
    internal class GoldenLiquid
    {
        public string Description { get; set; }

        [JsonProperty("tests")]
        public List<GoldenLiquidTest> Tests { get; set; }
    }
}
