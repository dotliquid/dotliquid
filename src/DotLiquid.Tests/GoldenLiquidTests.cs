using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Newtonsoft.Json;
using System.Globalization;
using System;
using System.Reflection;
using System.Dynamic;

namespace DotLiquid.Tests
{
    public class GoldenLiquidTests
    {
        [Test]
        public void ExecuteGoldenLiquidTests()
        {
            // Load the JSON content
#if NETCOREAPP1_0
            var assembly = typeof(GoldenLiquidTests).GetTypeInfo().Assembly;
#else
            var assembly = Assembly.GetExecutingAssembly();
#endif
            var jsonContent = string.Empty;
            using (Stream stream = assembly.GetManifestResourceStream("DotLiquid.Tests.Embedded.golden_liquid.json"))
            using (StreamReader reader = new StreamReader(stream))
            {
                jsonContent = reader.ReadToEnd();
            }

            // Deserialize the JSON content
            var goldenLiquid = JsonConvert.DeserializeObject<GoldenLiquid>(jsonContent);

            Liquid.UseRubyDateFormat = true;

            Assert.Multiple(() =>
            {
                // Iterate through the tests
                foreach (var testGroup in goldenLiquid.TestGroups)
                {
                    foreach (var test in testGroup.Tests)
                    {
                        // Create a new Hash object to represent the context
                        var context = new Hash();
                        foreach (var pair in test.Context)
                        {
                            context[pair.Key] = pair.Value;
                        }

                        var testName = $"{testGroup.Name} - {test.Name}";
                        var syntax = SyntaxCompatibility.DotLiquid22a;
                        var parameters = new RenderParameters(CultureInfo.CurrentCulture) {
                                SyntaxCompatibilityLevel = syntax,
                                LocalVariables = context,
                                ErrorsOutputMode = test.Error ? ErrorsOutputMode.Rethrow : ErrorsOutputMode.Display
                        };

                        // If the test should produce an error, assert that it does
                        if (test.Error)
                        {
                            Assert.That(() => Template.Parse(test.Template, syntax).Render(parameters), Throws.Exception, testName);
                        }
                        else
                        {
                            Assert.That(Template.Parse(test.Template, syntax).Render(parameters), Is.EqualTo(test.Want), testName);
                        }
                    }
                }
            });

        }
    }

    public class GoldenLiquid
    {
        public string Version { get; set; }

        [JsonProperty("test_groups")]
        public List<GoldenLiquidGroup> TestGroups { get; set; }
    }

    public class GoldenLiquidGroup
    {
        public string Name { get; set; }

        [JsonProperty("tests")]
        public List<Test> Tests { get; set; }
    }

    public class Test
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("want")]
        public string Want { get; set; }

        [JsonProperty("context")]
        public ExpandoObject Context { get; set; }

        [JsonProperty("partials")]
        public Dictionary<string, object> Partials { get; set; }

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("strict")]
        public bool Strict { get; set; }
    }
}
