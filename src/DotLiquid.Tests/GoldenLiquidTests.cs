using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;
using System;
using System.Reflection;
using DotLiquid.Tests.Model;
using DotLiquid.Tests.Util;

namespace DotLiquid.Tests
{
    public class GoldenLiquidTests
    {
        #region Static Variables For Test Cases
        private static GoldenLiquidRules rules;

        internal static GoldenLiquidRules Rules
        {
            get
            {
                if (rules == null)
                    rules = DeserializeResource<GoldenLiquidRules>("DotLiquid.Tests.Embedded.golden_rules.json");

                return rules;
            }
        }

        public static List<GoldenLiquidTest> GetGoldenTests(bool passing)
        {
            var tests = new List<GoldenLiquidTest>();
            var goldenLiquid = DeserializeResource<GoldenLiquid>("DotLiquid.Tests.Embedded.golden_liquid.json");

            // Iterate through the tests
            foreach (var testGroup in goldenLiquid.TestGroups)
            {
                if (Rules.SkippedGroups.Contains(testGroup.Name))
                    continue;

                foreach (var test in testGroup.Tests)
                {
                    test.GroupName = testGroup.Name;
                    var uniqueName = test.UniqueName;

                    if (Rules.SkippedTests.Contains(uniqueName))
                        continue;

                    if (Rules.AlternateTestExpectations.ContainsKey(uniqueName))
                    { 
                        test.Want = Rules.AlternateTestExpectations[uniqueName];
                        test.Error = false;
                    }

                    if (Rules.FailingTests.Contains(uniqueName) != passing)
                        tests.Add(test);
                }
            }

            return tests;
        }

        public static List<GoldenLiquidTest> GoldenTestsPassing => GetGoldenTests(passing: true);

        private static T DeserializeResource<T>(string resourceName)
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

        [Test]
        [TestCaseSource(nameof(GoldenTestsPassing))]
        public void ExecuteGoldenLiquidTests(GoldenLiquidTest test)
        {
            // Create a new Hash object to represent the context
            var context = new Hash();
            foreach (var pair in test.Context)
            {
                context[pair.Key] = pair.Value;
            }

            var syntax = SyntaxCompatibility.DotLiquid22a;
            var parameters = new RenderParameters(CultureInfo.CurrentCulture)
            {
                SyntaxCompatibilityLevel = syntax,
                LocalVariables = context,
                ErrorsOutputMode = test.Error ? ErrorsOutputMode.Rethrow : ErrorsOutputMode.Display
            };

            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                Liquid.UseRubyDateFormat = true;
                if (test.Partials?.Count > 0)
                    Template.FileSystem = new DictionaryFileSystem(test.Partials);

                // If the test should produce an error, assert that it does
                if (test.Error)
                {
                    Assert.That(() => Template.Parse(test.Template, syntax).Render(parameters), Throws.Exception, test.UniqueName);
                }
                else
                {
                    Assert.That(Template.Parse(test.Template, syntax).Render(parameters).Replace("\r\n", "\n"), Is.EqualTo(test.Want), test.UniqueName);
                }
            });
        }

        [Test]
        public void ExecuteGoldenLiquidFailingTests()
        {
            var tests = GetGoldenTests(passing: false);
            Assert.Multiple(() =>
            {
                foreach (var test in tests)
                {
                    Assert.That(() => ExecuteGoldenLiquidTests(test), Throws.Exception, test.UniqueName);
                }
            });
        }
    }
}
