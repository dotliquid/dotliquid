using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotLiquid
{
	/// <summary>
	/// Holds variables. Variables are only loaded "just in time"
	/// and are not evaluated as part of the render stage
	///
	/// {{ monkey }}
	/// {{ user.name }}
	///
	/// Variables can be combined with filters:
	///
	/// {{ user | link }}
	/// </summary>
	public class Variable : IRenderable
	{		
	    //private readonly MarkupExpression _expression;
	    private readonly string Name;
	    private readonly IList<FilterRequest> Filters;

	    public Variable(string name, IList<FilterRequest> filters)
	    {
	        Name = name;
	        Filters = filters;
	    }

	    public void Render(Context context, TextWriter result)
	    {
	        var expression = new MarkupExpression(Name, Filters);
            var expressionValue = expression.Evaluate(context);

	        if (expressionValue != null)
			{
                var transformer = Template.GetValueTypeTransformer(expressionValue.GetType());
                
                if(transformer != null)
                    expressionValue = transformer(expressionValue);

				string outputString;
				if (expressionValue is IEnumerable)
#if NET35
					outputString = string.Join(string.Empty, ((IEnumerable)output).Cast<object>().Select(o => o.ToString()).ToArray());
#else
					outputString = string.Join(string.Empty, ((IEnumerable)expressionValue).Cast<object>());
#endif
				else if (expressionValue is bool)
					outputString = expressionValue.ToString().ToLower();
				else
					outputString = expressionValue.ToString();
				result.Write(outputString);
			}
	    }


	    
	}
}