using System;
using System.Collections.Generic;
using System.Linq;

namespace DotLiquid
{
    public class MarkupExpression
    {
        private readonly String _name;
        private readonly IEnumerable<FilterRequest> _filters;

        public MarkupExpression(String name, IEnumerable<FilterRequest> filters)
        {
            _filters = filters;
            _name = name;
        }


        public object Evaluate(Context context)
        {

            var dereferencedValue = DereferenceNameFromContext(context, _name);

            var filteredValue = ApplyFilters(context, _filters, dereferencedValue);

            return ApplyFinalTransformation(filteredValue);
        }

        private object ApplyFinalTransformation(object filteredVal)
        {
            if (filteredVal is IValueTypeConvertible)
                filteredVal = ((IValueTypeConvertible)filteredVal).ConvertToValueType();

            if (filteredVal is ILiquidizable)
                filteredVal = null;

            return filteredVal;
        }

        private static object ApplyFilters(
            Context context,
            IEnumerable<FilterRequest> filters,
            object valueToBeFiltered)
        {
            var result = valueToBeFiltered;
            return filters.Aggregate(result, (s, request) => request.Apply(context, s));            
        }

        private static object DereferenceNameFromContext(Context context, string name)
        {
            return context[name];
        }

    }
}
