using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace DotLiquid.Tests.Model
{
    public class GoldenLiquidTest
    {
        [JsonIgnore]
        public string GroupName { get; set; }

        [JsonIgnore]
        public string UniqueName => $"{GroupName} - {Name}";

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("want")]
        public string Want { get; set; }

        [JsonProperty("context")]
        public ExpandoObject Context { get; set; }

        [JsonProperty("partials")]
        public Dictionary<string, string> Partials { get; set; }

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("strict")]
        public bool Strict { get; set; }

        public override string ToString()
        {
            return UniqueName;
        }
    }
}
