using System;
using System.Collections.Generic;
using DotLiquid.Exceptions;
using NUnit.Framework;
using ArgumentException = DotLiquid.Exceptions.ArgumentException;

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
		}

		[Test]
		public void TestSyntaxException()
		{
			Template template = null;
			Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.syntax_exception }} "); });
		    List<Exception> errors;
			string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }), out errors);
			Assert.AreEqual(" Liquid syntax error: syntax exception ", result);

			Assert.AreEqual(1, errors.Count);
			Assert.IsInstanceOf<SyntaxException>(errors[0]);
		}

		[Test]
		public void TestArgumentException()
		{
			Template template = null;
            Assert.DoesNotThrow(() => { template = Template.Parse(" {{ errors.argument_exception }} "); });
            List<Exception> errors;
			string result = template.Render(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() }), out errors);
			Assert.AreEqual(" Liquid error: argument exception ", result);

			Assert.AreEqual(1, errors.Count);
			Assert.IsInstanceOf<ArgumentException>(errors[0]);
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
            List<Exception> errors;
            Assert.AreEqual(" Liquid error: Unknown operator =! ", template.Render(out errors));

			Assert.AreEqual(1, errors.Count);
			Assert.IsInstanceOf<ArgumentException>(errors[0]);
		}
	}
}