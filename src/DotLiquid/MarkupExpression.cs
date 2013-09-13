using System;
using System.Collections.Generic;
using System.Linq;

namespace DotLiquid
{
    public class MarkupExpression
    {
        private readonly String Name;
        private readonly IEnumerable<FilterRequest> Filters;

        public MarkupExpression(String name, IEnumerable<FilterRequest> filters)
        {
            Filters = filters;
            Name = name;
        }


        public object Evaluate(Context context)
        {

            var dereferencedValue = DereferenceNameFromContext(context, Name);

            var filteredValue = ApplyFilters(context, Filters, dereferencedValue);

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
