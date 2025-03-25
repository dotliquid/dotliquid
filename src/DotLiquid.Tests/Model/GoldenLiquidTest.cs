using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace DotLiquid.Tests.Model
{
    public class GoldenLiquidTest
    {
        [JsonIgnore]
        public string UniqueName => Name;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("data")]
        public ExpandoObject Data { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("results")]
        public List<string> Results { get; set; }

        [JsonProperty("templates")]
        public Dictionary<string, string> Templates { get; set; }

        [JsonProperty("invalid")]
        public bool IsInvalid { get; set; }

        [JsonProperty("tags")]
        public List<GoldenLiquidTag> Tags { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
