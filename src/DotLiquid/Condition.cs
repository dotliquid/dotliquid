using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static readonly Dictionary<string, ConditionOperatorDelegate> Operators = new Dictionary<string, ConditionOperatorDelegate>()
        {
            { "==", (left, right) => EqualVariables(left, right) },
            { "!=", (left, right) => !EqualVariables(left, right) },
            { "<>", (left, right) => !EqualVariables(left, right) },
            { "<", (left, right) => left != null && right != null && Comparer<object>.Default.Compare(left, Convert.ChangeType(right, left.GetType())) == -1 },
            { ">", (left, right) => left != null && right != null && Comparer<object>.Default.Compare(left, Convert.ChangeType(right, left.GetType())) == 1 },
            { "<=", (left, right) => left != null && right != null && Comparer<object>.Default.Compare(left, Convert.ChangeType(right, left.GetType())) <= 0 },
            { ">=", (left, right) => left != null && right != null && Comparer<object>.Default.Compare(left, Convert.ChangeType(right, left.GetType())) >= 0 },
            { "contains", (left, right) => ((left is string) ? ((string)left).Contains((string)right) : (left is IEnumerable) ? Any((IEnumerable) left, (element) => element.BackCompatSafeTypeInsensitiveEqual(right)) : false) },
            { "startsWith", (left, right) => (left is IList) ? EqualVariables(((IList) left).OfType<object>().FirstOrDefault(), right) : ((left is string) ? ((string)left).StartsWith((string) right) : false) },
            { "endsWith", (left, right) => (left is IList) ? EqualVariables(((IList) left).OfType<object>().LastOrDefault(), right) : ((left is string) ? ((string)left).EndsWith((string) right) : false) },
            { "hasKey", (left, right) => (left is IDictionary) ? ((IDictionary) left).Contains(right) : false },
            { "hasValue", (left, right) => (left is IDictionary) ? ((IDictionary) left).Values.Cast<object>().Contains(right) : false }
        };

        private static bool Any(IEnumerable enumerable, Func<object, bool> condition)
        {
            foreach (var obj in enumerable)
            {
                if (condition(obj))
                {
                    return true;
                }
            }

            return false;
        }

        private string _childRelation;

        private Condition _childCondition;

        public string Left { get; set; }

        public string Operator { get; set; }

        public string Right { get; set; }

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

        public virtual bool Evaluate(Context context, IFormatProvider formatProvider)
        {
            context = context ?? new Context(formatProvider);
            bool result = InterpretCondition(Left, Right, Operator, context);

            switch (_childRelation)
            {
                case "or":
                    return result || _childCondition.Evaluate(context, formatProvider);
                case "and":
                    return result && _childCondition.Evaluate(context, formatProvider);
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
            if (left is Symbol leftSymbol)
            { 
                return leftSymbol.EvaluationFunction(right);
            }

            if (right is Symbol rightSymbol)
            { 
                return rightSymbol.EvaluationFunction(left);
            }

            return left.SafeTypeInsensitiveEqual(right);
        }

        private static bool InterpretCondition(string left, string right, string op, Context context)
        {
            // If the operator is empty this means that the decision statement is just
            // a single variable. We can just poll this variable from the context and
            // return this as the result.
            if (string.IsNullOrEmpty(op))
            {
                object result = context[left, false];
                return (result != null && (!(result is bool) || (bool) result));
            }

            object leftObject = context[left];
            object rightObject = context[right];

            var opKey = Operators.Keys.FirstOrDefault(opk => opk.Equals(op)
                                                                || opk.ToLowerInvariant().Equals(op)
                                                                || Template.NamingConvention.OperatorEquals(opk, op)
                                                     );
            if (opKey == null)
            { 
                throw new Exceptions.ArgumentException(Liquid.ResourceManager.GetString("ConditionUnknownOperatorException"), op);
            }

            return Operators[opKey](leftObject, rightObject);
        }
    }

    public class ElseCondition : Condition
    {
        public override bool IsElse
        {
            get { return true; }
        }

        public override bool Evaluate(Context context, IFormatProvider formatProvider)
        {
            return true;
        }
    }

    public delegate bool ConditionOperatorDelegate(object left, object right);
}
