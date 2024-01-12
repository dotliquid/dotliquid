using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotLiquid.Exceptions;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ExceptionHandlingTests
    {
        private INamingConvention NamingConvention { get; } = new RubyNamingConvention();

        private class ExceptionDrop : Drop
        {
            public void ArgumentException()
            {
                throw new ArgumentException("argument exception");
            }

            public void SyntaxException()
            {
                throw new SyntaxException("syntax exception");
            }

            public void InterruptException()
            {
                throw new InterruptException("interrupted");
            }
        }

        [Test]
        public void TestSyntaxException()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.syntax_exception }} ", NamingConvention); });
            string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }));
            Assert.AreEqual(" Liquid syntax error: syntax exception ", result);

            Assert.AreEqual(1, template.Errors.Count);
            Assert.IsInstanceOf<SyntaxException>(template.Errors[0]);
        }

        [Test]
        public void TestArgumentException()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.argument_exception }} ", NamingConvention); });
            string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }));
            Assert.AreEqual(" Liquid error: argument exception ", result);

            Assert.AreEqual(1, template.Errors.Count);
            Assert.IsInstanceOf<ArgumentException>(template.Errors[0]);
        }

        [Test]
        public void TestMissingEndTagParseTimeError()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse(" {% for a in b %} ... ", NamingConvention));
        }

        [Test]
        public void TestUnrecognizedOperator()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {% if 1 =! 2 %}ok{% endif %} ", NamingConvention); });
            Assert.AreEqual(" Liquid error: Unknown operator =! ", template.Render());

            Assert.AreEqual(1, template.Errors.Count);
            Assert.IsInstanceOf<ArgumentException>(template.Errors[0]);
        }

        [Test]
        public void TestInterruptException()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.interrupt_exception }} ", NamingConvention); });
            var localVariables = Hash.FromAnonymousObject(new { errors = new ExceptionDrop() });
            var exception = Assert.Throws<InterruptException>(() => template.Render(localVariables));

            Assert.AreEqual("interrupted", exception.Message);
        }

        [Test]
        public void TestMaximumIterationsExceededError()
        {
            var template = Template.Parse(" {% for i in (1..100000) %} {{ i }} {% endfor %} ", NamingConvention);
            Assert.Throws<MaximumIterationsExceededException>(() =>
            {
                template.Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    MaxIterations = 50
                });
            });
        }

        [Test]
        public void TestTimeoutError()
        {
            var template = Template.Parse(" {% for i in (1..1000000) %} {{ i }} {% endfor %} ", NamingConvention);
            Assert.Throws<System.TimeoutException>(() =>
            {
                template.Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    Timeout = 100 //ms
                });
            });
        }

        [Test]
        public void TestOperationCancelledError()
        {
            var template = Template.Parse(" {% for i in (1..1000000) %} {{ i }} {% endfor %} ", NamingConvention);
            var source = new CancellationTokenSource(100);
            var context = new Context(
                environments: new List<Hash>(),
                outerScope: new Hash(NamingConvention),
                registers: new Hash(NamingConvention),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                NamingConvention,
                cancellationToken: source.Token);

            Assert.Throws<System.OperationCanceledException>(() =>
            {
                template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            });
        }

        [Test]
        public void TestErrorsOutputModeRethrow()
        {
            var template = Template.Parse("{{test}}", NamingConvention);
            Hash assigns = new Hash((h, k) => { throw new SyntaxException("Unknown variable '" + k + "'"); }, NamingConvention);

            Assert.Throws<SyntaxException>(() =>
            {
                var output = template.Render(new RenderParameters(CultureInfo.InvariantCulture)
                {
                    LocalVariables = assigns,
                    ErrorsOutputMode = ErrorsOutputMode.Rethrow
                });
            });
        }

        [Test]
        public void TestErrorsOutputModeSuppress()
        {
            var template = Template.Parse("{{test}}", NamingConvention);
            Hash assigns = new Hash((h, k) => { throw new SyntaxException("Unknown variable '" + k + "'"); }, NamingConvention);

            var output = template.Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                ErrorsOutputMode = ErrorsOutputMode.Suppress
            });
            Assert.AreEqual("", output);
        }

        [Test]
        public void TestErrorsOutputModeDisplay()
        {
            var template = Template.Parse("{{test}}", NamingConvention);
            Hash assigns = new Hash((h, k) => { throw new SyntaxException("Unknown variable '" + k + "'"); }, NamingConvention);

            var output = template.Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                ErrorsOutputMode = ErrorsOutputMode.Display
            });
            Assert.IsNotEmpty(output);
        }
    }
}
