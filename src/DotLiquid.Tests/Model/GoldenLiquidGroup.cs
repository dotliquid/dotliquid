using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotLiquid.Tests.Model
{
    internal class GoldenLiquidGroup
    {
        public string Name { get; set; }

        [JsonProperty("tests")]
        public List<GoldenLiquidTest> Tests { get; set; }
    }
}
