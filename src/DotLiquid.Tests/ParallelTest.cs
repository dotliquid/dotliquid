using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid.NamingConventions;

namespace DotLiquid.Tests {
    [TestFixture]
    public class ParallelTest
    {
        private INamingConvention NamingConvention { get; } = new RubyNamingConvention();

        [Test]
        public void TestCachedTemplateRender() {
            Template template = Template.Parse(@"{% assign foo = 'from instance assigns' %}{{foo}}", NamingConvention);
            template.MakeThreadSafe();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 30 };

            Parallel.For(0, 10000, parallelOptions, (x) => Assert.AreEqual("from instance assigns", template.Render()));
        }
    }
}
