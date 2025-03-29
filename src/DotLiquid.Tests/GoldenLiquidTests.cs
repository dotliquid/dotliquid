using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;
using System;
using System.Reflection;
using DotLiquid.Tests.Model;
using DotLiquid.Tests.Util;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;

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

        public static IEnumerable<GoldenLiquidTest> GetGoldenTests()
        {
            var tests = new List<GoldenLiquidTest>();
            var goldenLiquid = DeserializeResource<GoldenLiquid>("DotLiquid.Tests.Embedded.golden_liquid.json");

            // Iterate through the tests
            foreach (var test in goldenLiquid.Tests)
            {
                var uniqueName = test.UniqueName;
                
                if (Rules.SkippedGroups.Any(groupPrefix => uniqueName.StartsWith(groupPrefix)))
                    continue;

                if (Rules.SkippedTests.Contains(uniqueName))
                    continue;

                // Tweak newlines characters in result for tablerow tests
                if (Rules.NewlineGroups.Any(groupPrefix => uniqueName.StartsWith(groupPrefix)))
                {
                    if (test.Result != null)
                    {
                        test.Result = test.Result.Replace("\n", "\r\n");
                    }
                    else
                    {
                        for (int i = 0; i < test.Results.Count; i++)
                        {
                            test.Results[i] = test.Results[i].Replace("\n", "\r\n");
                        }
                    }
                }

                if (Rules.AlternateTestExpectations.ContainsKey(uniqueName))
                {
                    // If we don't have a list, move the Result to the list.
                    if (test.Results == null)
                    {
                        test.Results = new List<string> { test.Result };
                        test.Result = null;
                    }
                    test.Results.Add(Rules.AlternateTestExpectations[uniqueName]);
                    test.IsInvalid = false;
                }

                tests.Add(test);
            }
            return tests;
        }

        public static IEnumerable<GoldenLiquidTest> GetGoldenTests(bool passing)
        {
            return GetGoldenTests().Where(test => Rules.FailingTests.Contains(test.UniqueName) != passing);
        }

        public static IEnumerable<GoldenLiquidTest> GoldenTestsPassing => GetGoldenTests(passing: true);

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

        internal static class RubyFilters
        {
            public static string[] Split(string input, string pattern) => ExtendedFilters.RubySplit(input, pattern);
        }

        [Test]
        [TestCaseSource(nameof(GoldenTestsPassing))]
        public void ExecuteGoldenLiquidTests(GoldenLiquidTest test)
        {
            // Create a new Hash object to represent the context
            var context = Hash.FromDictionary(test.Data);

            var syntax = SyntaxCompatibility.DotLiquidLatest;
            var parameters = new RenderParameters(CultureInfo.CurrentCulture)
            {
                SyntaxCompatibilityLevel = syntax,
                LocalVariables = context,
                ErrorsOutputMode = test.IsInvalid ? ErrorsOutputMode.Rethrow : ErrorsOutputMode.Display,
                Filters = new[] { typeof(RubyFilters) }
            };

            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                Liquid.UseRubyDateFormat = true;
                if (test.Templates?.Count > 0)
                    Template.FileSystem = new DictionaryFileSystem(test.Templates);

                // If the test should produce an error, assert that it does
                if (test.IsInvalid)
                {
                    Assert.That(() => Template.Parse(test.Template, syntax).Render(parameters), Throws.Exception, test.UniqueName);
                }
                else
                {
                    var result = Template.Parse(test.Template, syntax).Render(parameters);
                    // test will contain either Result or Results, but not both.
                    if (test.Result != null)
                    {
                        Assert.That(result, Is.EqualTo(test.Result), test.UniqueName);
                    }
                    else
                    {
                        Assert.That(test.Results, Contains.Item(result), test.UniqueName);
                    }
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

        #region Integrity checks for golden_rules.json

        [Test]
        public void CheckRules_FailingTestsExist()
        {
            // Checks all the tests listed in Rules.FailingTests exist
            var testNames = GetGoldenTests(false).Select(test => test.UniqueName);
            var expectedTestNames = Rules.FailingTests;

            Assert.Multiple(() =>
            {
                foreach (var expectedTestName in expectedTestNames)
                {
                    Assert.That(testNames.Contains(expectedTestName), expectedTestName);
                }
            });
        }

        [Test]
        public void CheckRules_AlternateTestExpectationsExist()
        {
            // Checks all the tests listed in Rules.AlternateTestExpectations exist
            var testNames = GetGoldenTests(true).Select(test => test.UniqueName);
            var expectedTestNames = Rules.AlternateTestExpectations.Keys;

            Assert.Multiple(() =>
            {
                foreach (var expectedTestName in expectedTestNames)
                {
                    Assert.That(testNames.Contains(expectedTestName), expectedTestName);
                }
            });
        }

        [Test]
        public void CheckRules_SkippedTestsExist()
        {
            // Checks all the tests listed in Rules.SkippedTests exist
            var testNames = DeserializeResource<GoldenLiquid>("DotLiquid.Tests.Embedded.golden_liquid.json")
                .Tests
                .Select(test => test.UniqueName);
            var expectedTestNames = Rules.SkippedTests;

            Assert.Multiple(() =>
            {
                foreach (var expectedTestName in expectedTestNames)
                {
                    Assert.That(testNames.Contains(expectedTestName), expectedTestName);
                }
            });
        }

        [Test]
        public void CheckRules_SkippedGroupsExist()
        {
            // Checks all the prefixes listed in Rules.SkippedGroups exist
            var testNames = DeserializeResource<GoldenLiquid>("DotLiquid.Tests.Embedded.golden_liquid.json")
                .Tests
                .Select(test => test.UniqueName);
            var expectedTestPrefixes = Rules.SkippedGroups;

            Assert.Multiple(() =>
            {
                foreach (var expectedTestPrefix in expectedTestPrefixes)
                {
                    Assert.That(testNames.Any(testName => testName.StartsWith(expectedTestPrefix)), expectedTestPrefix);
                }
            });
        }

        [Test]
        public void CheckRules_NewlineGroupsExist()
        {
            // Checks all the prefixes listed in Rules.NewlineGroups exist
            var testNames = DeserializeResource<GoldenLiquid>("DotLiquid.Tests.Embedded.golden_liquid.json")
                .Tests
                .Select(test => test.UniqueName);
            var expectedTestPrefixes = Rules.NewlineGroups;

            Assert.Multiple(() =>
            {
                foreach (var expectedTestPrefix in expectedTestPrefixes)
                {
                    Assert.That(testNames.Any(testName => testName.StartsWith(expectedTestPrefix)), expectedTestPrefix);
                }
            });
        }

        #endregion
    }
}
