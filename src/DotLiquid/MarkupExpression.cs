using System;
using System.Collections.Generic;
using System.Linq;

namespace DotLiquid
{
    public class MarkupExpression
    {
        private readonly String _markup;

        public MarkupExpression(String markup)
        {
            _markup = markup;
        }


        public object Evaluate(Context context)
        {
            var markupParser = new MarkupParser();
            var parserResult = markupParser.Parse(_markup);

            if (parserResult == null || parserResult.Name == null)
                return null;

            var dereferencedValue = DereferenceName(context, parserResult.Name);

            var filteredValue = ApplyFilters(context, parserResult.Filters, dereferencedValue);

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

        private static object DereferenceName(Context context, string name)
        {
            return context[name];
        }

    }
}
