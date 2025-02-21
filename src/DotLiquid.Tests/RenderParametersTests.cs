using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class RenderParametersTests
    {
        [Test]
        public void TestRethrowErrorsIsTrueWhenRethrow()
        {
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                ErrorsOutputMode = ErrorsOutputMode.Rethrow
            };
#pragma warning disable CS0618 // Type or member is obsolete
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.True);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Test]
        public void TestRethrowErrorsIsFalseWhenSuppress()
        {
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                ErrorsOutputMode = ErrorsOutputMode.Suppress
            };
#pragma warning disable CS0618 // Type or member is obsolete
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.False);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Test]
        public void TestRethrowErrorsIsFalseWhenDisplay()
        {
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                ErrorsOutputMode = ErrorsOutputMode.Display
            };
#pragma warning disable CS0618 // Type or member is obsolete
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.False);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Test]
        public void TestRethrowErrorsObsoleteTrue()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                RethrowErrors = true
            };
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.True);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.That(parameters.ErrorsOutputMode, Is.EqualTo(ErrorsOutputMode.Rethrow));
        }

        [Test]
        public void TestRethrowErrorsObsoleteFalse()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                RethrowErrors = false
            };
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.False);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.That(parameters.ErrorsOutputMode, Is.EqualTo(ErrorsOutputMode.Display));
        }
    }
}