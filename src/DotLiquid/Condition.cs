using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
			{ "==", EqualVariables },
			{ "!=", (left, right) => !EqualVariables(left, right) },
			{ "<>", (left, right) => !EqualVariables(left, right) },
			{ "<", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) == -1 },
			{ ">", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) == 1 },
			{ "<=", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) <= 0 },
			{ ">=", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) >= 0 },
			{ "contains",  Contains },
			{ "startswith", StartsWith },
			{ "endswith", EndsWith },
			{ "hasKey", HasKey },
			{ "hasValue", HasValue }
		};

		private static bool HasValue(object left, object right)
		{
			var dictionary = left as IDictionary;
			return dictionary != null
				&& dictionary.OfType<object>().Contains(right);
		}

		private static bool HasKey(object left, object right)
		{
			var dictionary = left as IDictionary;
			return dictionary != null 
				&& dictionary.Contains(right);
		}

		private static bool StartsWith(object left, object right)
		{
			var list = left as IList;
			if (list != null)
				return EqualVariables(list.OfType<object>().FirstOrDefault(), right);
			
			var str = left as string;
			return str != null && str.StartsWith((string) right);
		}

		private static bool EndsWith(object left, object right)
		{
			var list = left as IList;
			if (list != null)
				return EqualVariables(list.OfType<object>().LastOrDefault(), right);
			
			var str = left as string;
			return str != null && str.EndsWith((string) right);
		}

		private static bool Contains(object left, object right)
		{
			var list = left as IList;
			if (list != null)
				return list.Contains(right);

			var str = left as string;
			return str != null && str.Contains((string) right);
		}

		
		#endregion

		public string Left { get; set; }

		private string Operator
		{
			get { return _operatorString; }
			set
			{
				_operatorString = value;
				if (string.IsNullOrEmpty(value))
				{
					_invalidOperatorString = false;
					_operatorDelegate = null;
				}
				else
					_invalidOperatorString = !Operators.TryGetValue(value, out _operatorDelegate);
			}
		}
		
		public string Right { get; set; }

		private const byte AndCode = 1;
		private const byte OrCode = 2;

		private string _operatorString;
		private bool _invalidOperatorString;
		private ConditionOperatorDelegate _operatorDelegate;
		private byte _childRelation;
		private Condition _childCondition;

        public List<IRenderable> Attachment { get; private set; }

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
			if (_invalidOperatorString)
				throw new Exceptions.ArgumentException(Liquid.ResourceManager.GetString("ConditionUnknownOperatorException"), _operatorString);

			var result = _operatorDelegate == null 
						? NoOperator(context[Left]) 
						: _operatorDelegate(context[Left], context[Right]);
			
			if (_childRelation == OrCode)
				return result || _childCondition.Evaluate(context);
			
			if (_childRelation == AndCode)
				return result && _childCondition.Evaluate(context);

			return result;
		}

		public void Or(Condition condition)
		{
			_childRelation = OrCode;
			_childCondition = condition;
		}

		public void And(Condition condition)
		{
			_childRelation = AndCode;
			_childCondition = condition;
		}

        public List<IRenderable> Attach(List<IRenderable> attachment)
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
			var leftSymbol = left as Symbol;
			if (leftSymbol != null)
				return leftSymbol.EvaluationFunction(right);

			var rightSymbol = right as Symbol;
			if (rightSymbol != null)
				return rightSymbol.EvaluationFunction(left);

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

		private static bool NoOperator(object left)
		{
			return left != null 
				&& (!(left is bool) || (bool)left);
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