using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using DotLiquid;
using Newtonsoft.Json;
using System.Globalization;
using System;
using System.Reflection;
using System.Dynamic;
using DotLiquid.FileSystems;

namespace DotLiquid.Tests
{
    public class GoldenLiquidTests
    {
        public static List<object[]> GoldenTests
        {
            get
            {
                var tests = new List<object[]>();

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
                

                // Iterate through the tests
                foreach (var testGroup in goldenLiquid.TestGroups)
                {
                    foreach (var test in testGroup.Tests)
                    {
                        tests.Add(new object[] { $"{testGroup.Name} - {test.Name}", test });
                    }
                }

                return tests;
            }
        }

        private class TestFileSystem : IFileSystem
        {
            public Dictionary<string, string> Templates = new Dictionary<string, string>();

            public TestFileSystem(Dictionary<string, string> templates)
            {
                Templates = templates;
            }

            public string ReadTemplateFile(Context context, string templateName)
            {
                string templatePath = (string)context[templateName];

                if (Templates.TryGetValue(templatePath, out var template))
                    return template;

                return templatePath;
            }
        }

        private IFileSystem _originalFileSystem;

        [OneTimeSetUp]
        public void SetUp()
        {
            _originalFileSystem = Template.FileSystem;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Template.FileSystem = _originalFileSystem;
        }

        [Test]
        [TestCaseSource(nameof(GoldenTests))]
        public void ExecuteGoldenLiquidTests(string uniqueName, GoldenLiquidTest test)
        {   
            // Create a new Hash object to represent the context
            var context = new Hash();
            foreach (var pair in test.Context)
            {
                context[pair.Key] = pair.Value;
            }

            var syntax = SyntaxCompatibility.DotLiquid22a;
            var parameters = new RenderParameters(CultureInfo.CurrentCulture) {
                SyntaxCompatibilityLevel = syntax,
                LocalVariables = context,
                ErrorsOutputMode = test.Error ? ErrorsOutputMode.Rethrow : ErrorsOutputMode.Display
            };

            Liquid.UseRubyDateFormat = true;
            Template.FileSystem = new TestFileSystem(test.Partials);

            // If the test should produce an error, assert that it does
            if (test.Error)
            {
                Assert.That(() => Template.Parse(test.Template, syntax).Render(parameters), Throws.Exception, uniqueName);
            }
            else
            {
                Assert.That(Template.Parse(test.Template, syntax).Render(parameters), Is.EqualTo(test.Want), uniqueName);
            }
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
        public List<GoldenLiquidTest> Tests { get; set; }
    }

    public class GoldenLiquidTest
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
        public Dictionary<string, string> Partials { get; set; }

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("strict")]
        public bool Strict { get; set; }
    }
}
