using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ParallelTest
    {
        [Test]
        public void TestCachedTemplateRender()
        {
            Template template = Template.Parse(@"{% assign foo = 'from instance assigns' %}{{foo}}");
            template.MakeThreadSafe();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 30 };

            Parallel.For(0, 10000, parallelOptions, (x) => Assert.That(template.Render(), Is.EqualTo("from instance assigns")));
        }
    }
}
