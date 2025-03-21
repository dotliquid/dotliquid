using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotLiquid.Tests.Model
{
    public class GoldenLiquid
    {
        public string Version { get; set; }

        [JsonProperty("test_groups")]
        public List<GoldenLiquidGroup> TestGroups { get; set; }
    }
}
