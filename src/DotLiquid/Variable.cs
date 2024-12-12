using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid.Exceptions;

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

        public List<Filter> Filters { get; set; }
        public string Name { get; set; }

        private readonly string _markup;

        public Variable(string markup)
        {
            var partsEnumerator = new DotLiquid.Util.CharEnumerator(markup);

            _markup = markup;

            Name = Tokenizer.ReadToSearchChars(partsEnumerator, Tokenizer.SearchPipeOrQuoted).Trim();
            Filters = new List<Filter>();

            while (partsEnumerator.HasNext() && partsEnumerator.Next == Tokenizer.CharPipe)
            {
                partsEnumerator.MoveNext(); // pass over the pipe
                var filterName = Tokenizer.ReadToSearchChars(partsEnumerator, Tokenizer.SearchPipeColonOrQuoted).Trim();

                var filterArgs = new List<string>();
                if (partsEnumerator.HasNext() && partsEnumerator.Next == Tokenizer.CharColon)
                {
                    do
                    {
                        partsEnumerator.MoveNext(); // pass over the colon or comma
                        var arg = Tokenizer.ReadToSearchChars(partsEnumerator, Tokenizer.SearchPipeCommaOrQuoted).Trim();
                        filterArgs.Add(arg);
                    } while (partsEnumerator.HasNext() && partsEnumerator.Next == Tokenizer.CharComma);
                }
                Filters.Add(new Filter(filterName, filterArgs.ToArray()));
            }
        }

        public void Render(Context context, TextWriter result)
        {
            // NOTE(David Burg): The decimal type default string serialization behavior adds non-significant trailing zeroes
            // to indicate the precision of the result.
            // This is not a desirable default for Liquid as it confuses the users as to why '12.5 |times 10' becomes '125.0'.
            // So we overwrite the default serialization behavior to specify a format with maximum significant precision.
            // Decimal type has a maximum of 29 significant digits.
            string ToFormattedString(object obj, IFormatProvider formatProvider) =>
                (obj is decimal decimalValue)
                    ? decimalValue.ToString(format: "0.#############################", provider: formatProvider)
                    : obj is IFormattable ifo
                        ? ifo.ToString(format: null, formatProvider: formatProvider)
                        : (obj?.ToString() ?? "");

            object output = RenderInternal(context);

            if (output is ILiquidizable)
                output = null;

            if (output != null)
            {
                var transformer = Template.GetValueTypeTransformer(output.GetType());

                if (transformer != null)
                    output = transformer(output);

                // Treating Strings as IEnumerable, and was joining Chars in loop
                if (!(output is string outputString))
                {
                    if (output is IEnumerable enumerable)
                        outputString = string.Join(string.Empty, enumerable.Cast<object>().Select(o => ToFormattedString(o, result.FormatProvider)).ToArray());
                    else if (output is bool)
                        outputString = output.ToString().ToLower();
                    else
                        outputString = ToFormattedString(output, result.FormatProvider);
                }

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
            }

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
