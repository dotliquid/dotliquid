using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

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
        private static readonly Regex FilterParserRegex = R.B(R.Q(@"(?:{0}|(?:\s*(?!(?:{0}))(?:{1}|\S+)\s*)+)"), Liquid.FilterSeparator, Liquid.QuotedFragment);
        private static readonly Regex FilterArgRegex = R.B(R.Q(@"(?:{0}|{1})\s*({2})"), Liquid.FilterArgumentSeparator, Liquid.ArgumentSeparator, Liquid.QuotedFragment);
        private static readonly Regex QuotedAssignFragmentRegex = R.B(R.Q(@"\s*({0})(.*)"), Liquid.QuotedAssignFragment);
        private static readonly Regex FilterSeparatorRegex = R.B(R.Q(@"{0}\s*(.*)"), Liquid.FilterSeparator);
        private static readonly Regex FilterNameRegex = R.B(R.Q(@"\s*(\w+)"));

        public List<Filter> Filters { get; set; }
        public string Name { get; set; }

        private readonly string _markup;

        public Variable(string markup)
        {
            _markup = markup;

            Name = null;
            Filters = new List<Filter>();

            Match match = QuotedAssignFragmentRegex.Match(markup);
            if (match.Success)
            {
                Name = match.Groups[1].Value;
                Match filterMatch = FilterSeparatorRegex.Match(match.Groups[2].Value);
                if (filterMatch.Success)
                {
                    foreach (string f in R.Scan(filterMatch.Value, FilterParserRegex))
                    {
                        Match filterNameMatch = FilterNameRegex.Match(f);
                        if (filterNameMatch.Success)
                        {
                            string filterName = filterNameMatch.Groups[1].Value;
                            List<string> filterArgs = R.Scan(f, FilterArgRegex);
                            Filters.Add(new Filter(filterName, filterArgs.ToArray()));
                        }
                    }
                }
            }
        }

        public void Render(Context context, TextWriter result)
        {
            string ToFormattedString(object o, IFormatProvider ifp) => o is IFormattable ifo ? ifo.ToString( null, ifp ) : (o?.ToString() ?? "");

            object output = RenderInternal(context);

            if (output is ILiquidizable)
                output = null;

            if (output != null)
            {
                var transformer = Template.GetValueTypeTransformer(output.GetType());

                if (transformer != null)
                    output = transformer(output);

                //treating Strings as IEnumerable, and was joining Chars in loop
                string outputString = output as string;

              if (outputString != null) {}
              else if (output is IEnumerable)
                 outputString = string.Join(string.Empty, ((IEnumerable)output).Cast<object>().Select(o => ToFormattedString(o,result.FormatProvider)).ToArray());
              else if (output is bool)
                 outputString = output.ToString().ToLower();
              else
                 outputString = ToFormattedString(output,result.FormatProvider);

            
                result.Write(outputString);
            }
        }

        private object RenderInternal(Context context)
        {
            if (Name == null)
                return null;

            object output = context[Name];

            foreach (var filter in Filters.ToList())
            {
                List<object> filterArgs = filter.Arguments.Select(a => context[a]).ToList();
                try
                {
                    filterArgs.Insert(0, output);
                    output = context.Invoke(filter.Name, filterArgs);
                }
                catch (FilterNotFoundException ex)
                {
                    throw new FilterNotFoundException(string.Format(Liquid.ResourceManager.GetString("VariableFilterNotFoundException"), filter.Name, _markup.Trim()), ex);
                }
            };

            if (output is IValueTypeConvertible valueTypeConvertibleOutput)
            { 
                output = valueTypeConvertibleOutput.ConvertToValueType();
            }

            return output;
        }

        /// <summary>
        /// Primarily intended for testing.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal object Render(Context context)
        {
            return RenderInternal(context);
        }

        public class Filter
        {
            public Filter(string name, string[] arguments)
            {
                Name = name;
                Arguments = arguments;
            }

            public string Name { get; set; }
            public string[] Arguments { get; set; }
        }
    }
}
