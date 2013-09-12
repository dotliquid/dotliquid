using System;
using System.Collections.Generic;
using System.Linq;

namespace DotLiquid
{
    public class FilterRequest
    {
        public string Name { get; set; }
        public string[] Arguments { get; set; }

        public FilterRequest(string name, string[] arguments = null)
        {
            Name = name;
            if (arguments == null)
            {
                arguments = new string[] { };
            }
            Arguments = arguments;
        }

        public Object Apply(Context context, object filteredObject)
        {
            object result = filteredObject;

            List<object> filterArgs = Arguments.Select(a => context[a]).ToList();
           
            filterArgs.Insert(0, result);
            result = context.Invoke(Name, filterArgs);

            return result;
        }


    }
}
