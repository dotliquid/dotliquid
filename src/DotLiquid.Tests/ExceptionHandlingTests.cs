using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ExceptionHandlingTests
    {
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
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.syntax_exception }} "); });
            string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }));
            Assert.That(result, Is.EqualTo(" Liquid syntax error: syntax exception "));

            Assert.That(template.Errors.Count, Is.EqualTo(1));
            Assert.That(template.Errors[0], Is.InstanceOf<SyntaxException>());
        }

        [Test]
        public void TestArgumentException()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.argument_exception }} "); });
            string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }));
            Assert.That(result, Is.EqualTo(" Liquid error: argument exception "));

            Assert.That(template.Errors.Count, Is.EqualTo(1));
            Assert.That(template.Errors[0], Is.InstanceOf<ArgumentException>());
        }

        [Test]
        public void TestMissingEndTagParseTimeError()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse(" {% for a in b %} ... "));
        }

        [Test]
        public void TestUnrecognizedOperator()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {% if 1 =! 2 %}ok{% endif %} "); });
            Assert.That(template.Render(), Is.EqualTo(" Liquid error: Unknown operator =! "));

            Assert.That(template.Errors.Count, Is.EqualTo(1));
            Assert.That(template.Errors[0], Is.InstanceOf<ArgumentException>());
        }

        [Test]
        public void TestInterruptException()
        {
            Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.interrupt_exception }} "); });
            var localVariables = Hash.FromAnonymousObject(new { errors = new ExceptionDrop() });
            var exception = Assert.Throws<InterruptException>(() => template.Render(localVariables));

            Assert.That(exception.Message, Is.EqualTo("interrupted"));
        }

        [Test]
        public void TestMaximumIterationsExceededError()
        {
            var template = Template.Parse(" {% for i in (1..100000) %} {{ i }} {% endfor %} ");
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
            var template = Template.Parse(" {% for i in (1..1000000) %} {{ i }} {% endfor %} ");
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
            var template = Template.Parse(" {% for i in (1..1000000) %} {{ i }} {% endfor %} ");
            var source = new CancellationTokenSource(100);
            var context = new Context(
                environments: new List<Hash>(),
                outerScope: new Hash(),
                registers: new Hash(),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                formatProvider: CultureInfo.InvariantCulture,
                cancellationToken: source.Token);

            Assert.Throws<System.OperationCanceledException>(() =>
            {
                template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            });
        }

        [Test]
        public void TestErrorsOutputModeRethrow()
        {
            var template = Template.Parse("{{test}}");
            Hash assigns = new Hash((h, k) => { throw new SyntaxException("Unknown variable '" + k + "'"); });

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
            var template = Template.Parse("{{test}}");
            Hash assigns = new Hash((h, k) => { throw new SyntaxException("Unknown variable '" + k + "'"); });

            var output = template.Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                ErrorsOutputMode = ErrorsOutputMode.Suppress
            });
            Assert.That(output, Is.EqualTo(""));
        }

        [Test]
        public void TestErrorsOutputModeDisplay()
        {
            var template = Template.Parse("{{test}}");
            Hash assigns = new Hash((h, k) => { throw new SyntaxException("Unknown variable '" + k + "'"); });

            var output = template.Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = assigns,
                ErrorsOutputMode = ErrorsOutputMode.Display
            });
            Assert.That(output, Is.Not.Empty);
        }
    }
}
