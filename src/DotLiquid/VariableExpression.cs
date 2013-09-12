using System.Collections.Generic;
using System.Linq;
using DotLiquid.Exceptions;

namespace DotLiquid
{
    public class VariableExpression
    {
        private readonly Context _context;
        private readonly IEnumerable<Variable.Filter> _filters;
        private readonly string _name;
        private readonly string _markup;
        

        public VariableExpression(
            string markup,
            IEnumerable<Variable.Filter> filters,
            string name,
            Context context)
        {
            _context = context;
            _name = name;
            _filters = filters;
            _markup = markup;
        }

        public object Evaluate()
        {
            if (_name == null)
                return null;

            object output = GetValueFromContext();

            output = ApplyFilters(output);

            if (output is IValueTypeConvertible)
                output = ((IValueTypeConvertible) output).ConvertToValueType();

            if (output is ILiquidizable)
                return null;

            return output;
        }

        private object GetValueFromContext()
        {
            return _context[_name];
        }

        private object ApplyFilters(object obj)
        {
            _filters.ToList().ForEach(filter =>
                {
                    obj = filter.Apply(_context, obj);
                });
            return obj;
        }


    }
}