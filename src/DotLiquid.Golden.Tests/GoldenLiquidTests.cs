using DotLiquid.Exceptions;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace DotLiquid.Golden.Tests
{

    public class GoldenLiquidTests
    {
        // These test groups are not implemented in DotLiquid
        private static readonly List<String> SkippedTestGroups = new List<string>()
        {
            "liquid.golden.echo_tag",
            "liquid.golden.liquid_tag",
            "liquid.golden.render_tag",
        };

        public static IEnumerable<TestData> ReadTestCases()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "DotLiquid.Golden.Tests.Resources.golden_liquid.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonContent = reader.ReadToEnd();
                GoldenLiquidData data = JsonConvert.DeserializeObject<GoldenLiquidData>(jsonContent);

                List<TestData> cases = new List<TestData>();
                foreach (TestGroup group in data.TestGroups)
                {
                    foreach (TestData test in group.Tests)
                    {
                        test.GroupName = group.Name;
                        test.Context = Hash.FromDictionary(test.RawContext);
                        test.Partials = Hash.FromDictionary(test.RawPartials);
                        cases.Add(test);
                    }
                }
                return cases;
            }
        }

        [TestCaseSource(nameof(ReadTestCases))]
        public void TestRender(TestData data)
        {
            if (SkippedTestGroups.Contains(data.GroupName))
            {
                Assert.Ignore($"Test group '{data.GroupName}' not implemented in DotLiquid");
            }

            SyntaxCompatibility syntaxCompatibility = SyntaxCompatibility.DotLiquid22a;

            RenderParameters renderParameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = data.Context,
                Registers = data.Partials,
                ErrorsOutputMode = ErrorsOutputMode.Display,
                SyntaxCompatibilityLevel = syntaxCompatibility
            };
            string result = Template.Parse(data.Template, syntaxCompatibility).Render(renderParameters);
            if (data.Error)
            {
                // The TestData only indicates there's an error, but not what the error message should be.
                Assert.That(result, Does.StartWith("Liquid error:").Or.StartsWith("Liquid syntax error:"));
            }
            else
            {
                Assert.That(result, Is.EqualTo(data.Want));
            }
        }
    }

}