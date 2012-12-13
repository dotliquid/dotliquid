using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid
{
	/// <summary>
	/// Container for liquid nodes which conveniently wraps decision making logic
	/// 
	/// Example:
	/// 
	/// c = Condition.new('1', '==', '1')
	/// c.evaluate #=> true
	/// </summary>
	public class Condition
	{
		#region Condition operator delegates

		public static readonly Dictionary<string, ConditionOperatorDelegate> Operators = new Dictionary<string, ConditionOperatorDelegate>(Template.NamingConvention.StringComparer)
		{
			{ "==", (left, right) => EqualVariables(left, right) },
			{ "!=", (left, right) => !EqualVariables(left, right) },
			{ "<>", (left, right) => !EqualVariables(left, right) },
			{ "<", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) == -1 },
			{ ">", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) == 1 },
			{ "<=", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) <= 0 },
			{ ">=", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) >= 0 },
			{ "contains", (left, right) => (left is IList) ? ((IList) left).Contains(right) : ((left is string) ? ((string) left).Contains((string) right) : false) },
            { "startswith", (left, right) => (left is IList) ? EqualVariables(((IList) left).OfType<object>().FirstOrDefault(), right) : ((left is string) ? ((string)left).StartsWith((string) right) : false) },
            { "endswith", (left, right) => (left is IList) ? EqualVariables(((IList) left).OfType<object>().LastOrDefault(), right) : ((left is string) ? ((string)left).EndsWith((string) right) : false) },
			{ "hasKey", (left, right) => (left is IDictionary) ? ((IDictionary) left).Contains(right) : false },
			{ "hasValue", (left, right) => (left is IDictionary) ? ((IDictionary) left).OfType<object>().Contains(right) : false }
		};

		#endregion

		public string Left { get; set; }
		public string Operator { get; set; }
		public string Right { get; set; }

		private string _childRelation;
		private Condition _childCondition;

		public List<object> Attachment { get; private set; }

		public virtual bool IsElse
		{
			get { return false; }
		}

		public Condition(string left, string @operator, string right)
		{
			Left = left;
			Operator = @operator;
			Right = right;
		}

		public Condition()
		{
		}

		public virtual bool Evaluate(Context context)
		{
			context = context ?? new Context();
			bool result = InterpretCondition(Left, Right, Operator, context);

			switch (_childRelation)
			{
				case "or":
					return result || _childCondition.Evaluate(context);
				case "and":
					return result && _childCondition.Evaluate(context);
				default:
					return result;
			}
		}

		public void Or(Condition condition)
		{
			_childRelation = "or";
			_childCondition = condition;
		}

		public void And(Condition condition)
		{
			_childRelation = "and";
			_childCondition = condition;
		}

		public List<object> Attach(List<object> attachment)
		{
			Attachment = attachment;
			return attachment;
		}

		public override string ToString()
		{
			return string.Format("<Condition {0} {1} {2}>", Left, Operator, Right);
		}

		private static bool EqualVariables(object left, object right)
		{
			if (left is Symbol)
				return ((Symbol) left).EvaluationFunction(right);

			if (right is Symbol)
				return ((Symbol) right).EvaluationFunction(left);

			if (left != null && right != null && left.GetType() != right.GetType())
			{
				try
				{
					right = Convert.ChangeType(right, left.GetType());
				}
				catch (Exception)
				{
				}
			}

			return Equals(left, right);
		}

		private static bool InterpretCondition(string left, string right, string op, Context context)
		{
			// If the operator is empty this means that the decision statement is just
			// a single variable. We can just poll this variable from the context and
			// return this as the result.
			if (string.IsNullOrEmpty(op))
			{
				object result = context[left];
				return (result != null && (!(result is bool) || (bool) result));
			}

			object leftObject = context[left];
			object rightObject = context[right];

			if (!Operators.ContainsKey(op))
				throw new Exceptions.ArgumentException(Liquid.ResourceManager.GetString("ConditionUnknownOperatorException"), op);
			return Operators[op](leftObject, rightObject);
		}
	}

	public class ElseCondition : Condition
	{
		public override bool IsElse
		{
			get { return true; }
		}

		public override bool Evaluate(Context context)
		{
			return true;
		}
	}

	public delegate bool ConditionOperatorDelegate(object left, object right);
}