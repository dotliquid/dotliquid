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
using System.Linq;

namespace DotLiquid.Tests
{
    public class GoldenLiquidTests
    {
        #region Static Variables For Test Cases
        private static GoldenLiquidRules rules;

        public static GoldenLiquidRules Rules
        {
            get
            {
                if (rules == null)
                    rules = Deserialize<GoldenLiquidRules>("DotLiquid.Tests.Embedded.golden_rules.json");

                return rules;
            }
        }

        public static List<object[]> GetGoldenTests(bool passing)
        {
            var tests = new List<object[]>();
            var goldenLiquid = Deserialize<GoldenLiquid>("DotLiquid.Tests.Embedded.golden_liquid.json");

            // Iterate through the tests
            foreach (var testGroup in goldenLiquid.TestGroups)
            {
                if (Rules.SkippedGroups.Contains(testGroup.Name))
                    continue;

                foreach (var test in testGroup.Tests)
                {
                    var uniqueName = $"{testGroup.Name} - {test.Name}";
                    if (Rules.AlternateTestExpectations.ContainsKey(uniqueName))
                        test.Want = Rules.AlternateTestExpectations[uniqueName];

                    if (Rules.FailingTests.Contains(uniqueName) != passing)
                        tests.Add(new object[] { uniqueName, test });
                }
            }

            return tests;
        }

        public static List<object[]> GoldenTestsPassing => GetGoldenTests(passing: true);

        public static List<object[]> GoldenTestsFailing => GetGoldenTests(passing: false);

        private static T Deserialize<T>(string resourceName)
        {
            // Load the JSON content
#if NETCOREAPP1_0
            var assembly = typeof(GoldenLiquidTests).GetTypeInfo().Assembly;
#else
            var assembly = Assembly.GetExecutingAssembly();
#endif

            var jsonContent = string.Empty;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                jsonContent = reader.ReadToEnd();
            }

            // Deserialize the JSON content
            return JsonConvert.DeserializeObject<T>(jsonContent);
        }
        #endregion

        #region In-Memory FileSystem Implementation
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
        #endregion

        [Test]
        [TestCaseSource(nameof(GoldenTestsPassing))]
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

    #region Json Parsing Classes
    public class GoldenLiquidRules
    {
        [JsonProperty("skipped_groups")]
        public List<string> SkippedGroups { get; set; }

        [JsonProperty("failing_tests")]
        public List<string> FailingTests { get; set; }

        [JsonProperty("alternate_test_expectations")]
        public Dictionary<string, string> AlternateTestExpectations { get; set; }
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
    #endregion
}
