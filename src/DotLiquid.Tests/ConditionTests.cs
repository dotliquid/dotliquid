using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class ConditionTests
	{
		private Context _context;

		[Test]
		public void TestBasicCondition()
		{
			Assert.AreEqual(false, new Condition("1", "==", "2").Evaluate(null));
			Assert.AreEqual(true, new Condition("1", "==", "1").Evaluate(null));
		}

		[Test]
		public void TestDefaultOperatorsEvaluateTrue()
		{
			AssertEvaluatesTrue("1", "==", "1");
			AssertEvaluatesTrue("1", "!=", "2");
			AssertEvaluatesTrue("1", "<>", "2");
			AssertEvaluatesTrue("1", "<", "2");
			AssertEvaluatesTrue("2", ">", "1");
			AssertEvaluatesTrue("1", ">=", "1");
			AssertEvaluatesTrue("2", ">=", "1");
			AssertEvaluatesTrue("1", "<=", "2");
			AssertEvaluatesTrue("1", "<=", "1");
		}

		[Test]
		public void TestDefaultOperatorsEvaluateFalse()
		{
			AssertEvaluatesFalse("1", "==", "2");
			AssertEvaluatesFalse("1", "!=", "1");
			AssertEvaluatesFalse("1", "<>", "1");
			AssertEvaluatesFalse("1", "<", "0");
			AssertEvaluatesFalse("2", ">", "4");
			AssertEvaluatesFalse("1", ">=", "3");
			AssertEvaluatesFalse("2", ">=", "4");
			AssertEvaluatesFalse("1", "<=", "0");
			AssertEvaluatesFalse("1", "<=", "0");
		}

		[Test]
		public void TestContainsWorksOnStrings()
		{
			AssertEvaluatesTrue("'bob'", "contains", "'o'");
			AssertEvaluatesTrue("'bob'", "contains", "'b'");
			AssertEvaluatesTrue("'bob'", "contains", "'bo'");
			AssertEvaluatesTrue("'bob'", "contains", "'ob'");
			AssertEvaluatesTrue("'bob'", "contains", "'bob'");

			AssertEvaluatesFalse("'bob'", "contains", "'bob2'");
			AssertEvaluatesFalse("'bob'", "contains", "'a'");
			AssertEvaluatesFalse("'bob'", "contains", "'---'");
		}

		[Test]
		public void TestContainsWorksOnArrays()
		{
			_context = new Context();
			_context["array"] = new[] { 1, 2, 3, 4, 5 };

			AssertEvaluatesFalse("array", "contains", "0");
			AssertEvaluatesTrue("array", "contains", "1");
			AssertEvaluatesTrue("array", "contains", "2");
			AssertEvaluatesTrue("array", "contains", "3");
			AssertEvaluatesTrue("array", "contains", "4");
			AssertEvaluatesTrue("array", "contains", "5");
			AssertEvaluatesFalse("array", "contains", "6");

			AssertEvaluatesFalse("array", "contains", "'1'");
		}

		[Test]
		public void TestContainsReturnsFalseForNilCommands()
		{
			AssertEvaluatesFalse("not_assigned", "contains", "0");
			AssertEvaluatesFalse("0", "contains", "not_assigned");
		}

		[Test]
		public void TestOrCondition()
		{
			Condition condition = new Condition("1", "==", "2");
			Assert.IsFalse(condition.Evaluate(null));

			condition.Or(new Condition("2", "==", "1"));
			Assert.IsFalse(condition.Evaluate(null));

			condition.Or(new Condition("1", "==", "1"));
			Assert.IsTrue(condition.Evaluate(null));
		}

		[Test]
		public void TestAndCondition()
		{
			Condition condition = new Condition("1", "==", "1");
			Assert.IsTrue(condition.Evaluate(null));

			condition.And(new Condition("2", "==", "2"));
			Assert.IsTrue(condition.Evaluate(null));

			condition.And(new Condition("2", "==", "1"));
			Assert.IsFalse(condition.Evaluate(null));
		}

		[Test]
		public void TestShouldAllowCustomProcOperator()
		{
			try
			{
				Condition.Operators["starts_with"] =
					(left, right) => Regex.IsMatch(left.ToString(), string.Format("^{0}", right.ToString()));

				AssertEvaluatesTrue("'bob'", "starts_with", "'b'");
				AssertEvaluatesFalse("'bob'", "starts_with", "'o'");
			}
			finally
			{
				Condition.Operators.Remove("starts_with");
			}
		}

		[Test]
		public void TestLessThanDecimal()
		{
			var model = new { value = new decimal(-10.5) };

			string output = Template.Parse("{% if model.value < 0 %}passed{% endif %}")
			  .Render(Hash.FromAnonymousObject(new { model }));

			Assert.AreEqual("passed", output);
		}

		#region Helper methods

		private void AssertEvaluatesTrue(string left, string op, string right)
		{
			Assert.IsTrue(new Condition(left, op, right).Evaluate(_context ?? new Context()),
				"Evaluated false: {0} {1} {2}", left, op, right);
		}

		private void AssertEvaluatesFalse(string left, string op, string right)
		{
			Assert.IsFalse(new Condition(left, op, right).Evaluate(_context ?? new Context()),
				"Evaluated true: {0} {1} {2}", left, op, right);
		}

		#endregion
	}
}