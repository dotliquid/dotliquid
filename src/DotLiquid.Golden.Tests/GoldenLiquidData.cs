using Newtonsoft.Json;
using System.Collections.Generic;

namespace DotLiquid.Golden.Tests
{
    public class GoldenLiquidData
    {
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("test_groups")]
        public List<TestGroup> TestGroups { get; set; }
    }

    public class TestGroup
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("tests")]
        public List<TestData> Tests { get; set; }

        override public string ToString()
        {
            return Name;
        }
    }

    public class TestData
    {
        [JsonIgnore]
        public string GroupName { get; set; }
        [JsonIgnore]
        public Hash Context { get; set; }
        [JsonIgnore]
        public Hash Partials { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("template")]
        public string Template { get; set; }
        [JsonProperty("want")]
        public string Want { get; set; }
        [JsonProperty("context")]
        public Dictionary<string, object> RawContext { get; set; }
        [JsonProperty("partials")]
        public Dictionary<string, object> RawPartials { get; set; }
        [JsonProperty("error")]
        public bool Error { get; set; }
        [JsonProperty("strict")]
        public bool Strict { get; set; }

        override public string ToString()
        {
            return GroupName + " > " + Name;
        }
    }
}
