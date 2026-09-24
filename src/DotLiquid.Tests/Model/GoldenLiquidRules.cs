using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotLiquid.Tests.Model
{
    internal class GoldenLiquidRules
    {
        [JsonProperty("skipped_groups")]
        public List<string> SkippedGroups { get; set; }

        [JsonProperty("newline_groups")]
        public List<string> NewlineGroups{ get; set; }

        [JsonProperty("skipped_tests")]
        public List<string> SkippedTests { get; set; }

        [JsonProperty("failing_tests")]
        public List<string> FailingTests { get; set; }

        [JsonProperty("alternate_test_expectations")]
        public Dictionary<string, string> AlternateTestExpectations { get; set; }
    }
}
