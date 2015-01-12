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
			{ "==", EqualVariables },
			{ "!=", (left, right) => !EqualVariables(left, right) },
			{ "<>", (left, right) => !EqualVariables(left, right) },
			{ "<", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) == -1 },
			{ ">", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) == 1 },
			{ "<=", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) <= 0 },
			{ ">=", (left, right) => left != null && right != null && Comparer.Default.Compare(left, Convert.ChangeType(right, left.GetType())) >= 0 },
			{ "contains", (left, right) => (left is IList) ? ((IList) left).Contains(right) : ((left is string) ? ((string) left).Contains((string) right) : false) },
            { "startswith", (left, right) => (left is IList) ? EqualVariables(((IList) left).OfType<object>().FirstOrDefault(), right) : ((left is string) ? ((string)left).StartsWith((string) right) : false) },
            { "endswith", (left, right) => (left is IList) ? EqualVariables(((IList) left).OfType<object>().LastOrDefault(), right) : ((left is string) ? ((string)left).EndsWith((string) right) : false) },
			{ "hasKey", (left, right) => (left is IDictionary) && ((IDictionary) left).Contains(right) },
			{ "hasValue", (left, right) => (left is IDictionary) && ((IDictionary) left).OfType<object>().Contains(right) }
		};

		#endregion

	    private string Operator
	    {
	        get { return _operatorString; }
	        set
	        {
	            _operatorString = value;
	            if (string.IsNullOrEmpty(value))
	                _operatorDelegate = (l, r) => NoOperator(l);
	            else if (!Operators.TryGetValue(value, out _operatorDelegate))
	                throw new Exceptions.ArgumentException(Liquid.ResourceManager.GetString("ConditionUnknownOperatorException"), value);
	        }
	    }

	    private const byte AndCode = 1;
	    private const byte OrCode = 2;

	    private string _left;
	    private string _right;
	    private string _operatorString;
	    private ConditionOperatorDelegate _operatorDelegate;
		private byte _childRelation;
		private Condition _childCondition;

		public List<object> Attachment { get; private set; }

		public virtual bool IsElse
		{
			get { return false; }
		}

		public Condition(string left, string @operator, string right)
		{
			_left = left;
			Operator = @operator;
			_right = right;
		}

		public Condition()
		{
		}

	    public virtual bool Evaluate(Context context)
	    {
            var result = _operatorDelegate(context[_left], context[_right]);
            
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

		public List<object> Attach(List<object> attachment)
		{
			Attachment = attachment;
			return attachment;
		}

		public override string ToString()
		{
			return string.Format("<Condition {0} {1} {2}>", _left, Operator, _right);
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