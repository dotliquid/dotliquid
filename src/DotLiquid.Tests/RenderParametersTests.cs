using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class RenderParametersTests
    {
        [Test]
        [Obsolete("Tests an obsolete property")]
        public void TestRethrowErrorsIsTrueWhenRethrow()
        {
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                ErrorsOutputMode = ErrorsOutputMode.Rethrow
            };
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.True);
        }

        [Test]
        [Obsolete("Tests an obsolete property")]
        public void TestRethrowErrorsIsFalseWhenSuppress()
        {
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                ErrorsOutputMode = ErrorsOutputMode.Suppress
            };
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.False);
        }

        [Test]
        [Obsolete("Tests an obsolete property")]
        public void TestRethrowErrorsIsFalseWhenDisplay()
        {
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                ErrorsOutputMode = ErrorsOutputMode.Display
            };
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.False);
        }

        [Test]
        [Obsolete("Tests an obsolete property")]
        public void TestRethrowErrorsObsoleteTrue()
        {
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                RethrowErrors = true
            };
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.True);
            Assert.That(parameters.ErrorsOutputMode, Is.EqualTo(ErrorsOutputMode.Rethrow));
        }

        [Test]
        [Obsolete("Tests an obsolete property")]
        public void TestRethrowErrorsObsoleteFalse()
        {
            RenderParameters parameters = new RenderParameters(CultureInfo.InvariantCulture)
            {
                RethrowErrors = false
            };
            // We want to test the obsolete property 
            Assert.That(parameters.RethrowErrors, Is.False);
            Assert.That(parameters.ErrorsOutputMode, Is.EqualTo(ErrorsOutputMode.Display));
        }
    }
}