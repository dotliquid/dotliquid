using NUnit.Framework;
using System.IO;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class RenderableFactoryTests
    {

        [Test]
        public void TestDefaultVariableFactory()
        {
            Template template = Template.Parse("{{test}}");
            Assert.AreEqual("worked", template.Render(Hash.FromAnonymousObject(new { test = "worked" })));
            Assert.AreEqual("worked wonderfully", template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully" })));
        }
        public class SimpleTestRenderableFactory : IRenderableFactory
        {
            public IRenderable CreateVariable(string markup)
            {
                return new SimpleTestVariable(markup);
            }
        }

        public class SimpleTestVariable : Variable, IRenderable
        {
            public SimpleTestVariable(string markup) : base(markup) { }

            public new void Render(Context context, TextWriter result)
            {
                result.Write("<!-- before -->");
                base.Render(context, result);
                result.Write("<!-- after -->");
            }
        }

        [Test]
        public void TestSimpleVariableFactory()
        {
            Template template = Template.Parse("{{test}}", new SimpleTestRenderableFactory());
            Assert.AreEqual("<!-- before -->worked<!-- after -->", template.Render(Hash.FromAnonymousObject(new { test = "worked" })));
            Assert.AreEqual("<!-- before -->worked wonderfully<!-- after -->", template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully" })));
        }
        public class MoreComplexTestRenderableFactory : IRenderableFactory
        {
            private int VariableRenderCounter = 0;

            public IRenderable CreateVariable(string markup)
            {
                return new MoreComplexTestVariable(markup, ++VariableRenderCounter);
            }
        }

        public class MoreComplexTestVariable : Variable, IRenderable
        {
            private readonly int VariableNumber;

            public MoreComplexTestVariable(string markup, int variableNumber) : base(markup)
            {
                VariableNumber = variableNumber;
            }

            public new void Render(Context context, TextWriter result)
            {
                result.Write($"<span data-render-num=\"{VariableNumber}\" data-original-variable-name=\"{Name}\">");
                base.Render(context, result);
                result.Write("</span>");
            }
        }

        [Test]
        public void TestMoreComplexVariableFactory()
        {
            Template template = Template.Parse("{{test}}{{test2}}{{test3}}", new MoreComplexTestRenderableFactory());
            Assert.AreEqual(
                $"<span data-render-num=\"1\" data-original-variable-name=\"test\">worked</span>" +
                $"<span data-render-num=\"2\" data-original-variable-name=\"test2\">worked2</span>" +
                $"<span data-render-num=\"3\" data-original-variable-name=\"test3\">worked3</span>",
                template.Render(Hash.FromAnonymousObject(new { test = "worked", test2 = "worked2", test3 = "worked3" }))
            );
            Assert.AreEqual(
                $"<span data-render-num=\"1\" data-original-variable-name=\"test\">worked wonderfully</span>" +
                $"<span data-render-num=\"2\" data-original-variable-name=\"test2\">worked wonderfully2</span>" +
                $"<span data-render-num=\"3\" data-original-variable-name=\"test3\">worked wonderfully3</span>",
                template.Render(Hash.FromAnonymousObject(new { test = "worked wonderfully", test2 = "worked wonderfully2", test3 = "worked wonderfully3" }))
            );
        }
    }
}
