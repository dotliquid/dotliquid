using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotLiquid.Exceptions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.syntax_exception }} "); });
            string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }));
            ClassicAssert.AreEqual(" Liquid syntax error: syntax exception ", result);

            ClassicAssert.AreEqual(1, template.Errors.Count);
            ClassicAssert.IsInstanceOf<SyntaxException>(template.Errors[0]);
        }

        [Test]
        public void TestArgumentException()
        {
            Template template = null;
            ClassicAssert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.argument_exception }} "); });
            string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }));
            ClassicAssert.AreEqual(" Liquid error: argument exception ", result);

            ClassicAssert.AreEqual(1, template.Errors.Count);
            ClassicAssert.IsInstanceOf<ArgumentException>(template.Errors[0]);
        }

        [Test]
        public void TestMissingEndTagParseTimeError()
        {
            ClassicAssert.Throws<SyntaxException>(() => Template.Parse(" {% for a in b %} ... "));
        }

        [Test]
        public void TestUnrecognizedOperator()
        {
            Template template = null;
            ClassicAssert.DoesNotThrow(() => { template = Template.Parse(" {% if 1 =! 2 %}ok{% endif %} "); });
            ClassicAssert.AreEqual(" Liquid error: Unknown operator =! ", template.Render());

            ClassicAssert.AreEqual(1, template.Errors.Count);
            ClassicAssert.IsInstanceOf<ArgumentException>(template.Errors[0]);
        }

        [Test]
        public void TestInterruptException()
        {
            Template template = null;
            ClassicAssert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.interrupt_exception }} "); });
            var localVariables = Hash.FromAnonymousObject(new { errors = new ExceptionDrop() });
            var exception = ClassicAssert.Throws<InterruptException>(() => template.Render(localVariables));

            ClassicAssert.AreEqual("interrupted", exception.Message);
        }

        [Test]
        public void TestMaximumIterationsExceededError()
        {
            var template = Template.Parse(" {% for i in (1..100000) %} {{ i }} {% endfor %} ");
            ClassicAssert.Throws<MaximumIterationsExceededException>(() =>
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
            ClassicAssert.Throws<System.TimeoutException>(() =>
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

            ClassicAssert.Throws<System.OperationCanceledException>(() =>
            {
                template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            });
        }

        [Test]
        public void TestErrorsOutputModeRethrow()
        {
            var template = Template.Parse("{{test}}");
            Hash assigns = new Hash((h, k) => { throw new SyntaxException("Unknown variable '" + k + "'"); });

            ClassicAssert.Throws<SyntaxException>(() =>
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
            ClassicAssert.AreEqual("", output);
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
            ClassicAssert.IsNotEmpty(output);
        }
    }
}
