using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotLiquid.Util;


namespace DotLiquid
{

    public class MarkupParser
    {
        public static readonly string FilterParser = string.Format(R.Q(@"(?:{0}|(?:\s*(?!(?:{0}))(?:{1}|\S+)\s*)+)"), Liquid.FilterSeparator, Liquid.QuotedFragment);

        /// <summary>
        /// Extract a "name" and a "filter" from a string.
        /// </summary>
        /// <param name="markup"></param>
        /// <returns></returns>
        public MarkupParserResult Parse(string markup)
        {
            MarkupParserResult result = new MarkupParserResult();

            Match match = Regex.Match(markup, string.Format(R.Q(@"\s*({0})(.*)"), Liquid.QuotedAssignFragment));

            if (match.Success)
            {
                result.Name = match.Groups[1].Value;
                result.AddFilters(ExtractFilters(match.Groups[2].Value));
            }
            return result;
        }

        private IList<FilterRequest> ExtractFilters(string filterMarkup)
        {
            List<FilterRequest> filters = new List<FilterRequest>();

            Match filterMatch = Regex.Match(filterMarkup, string.Format(R.Q(@"{0}\s*(.*)"), Liquid.FilterSeparator));
            if (filterMatch.Success)
            {
                foreach (string f in R.Scan(filterMatch.Value, MarkupParser.FilterParser))
                {
                    Match filterNameMatch = Regex.Match(f, R.Q(@"\s*(\w+)"));
                    if (filterNameMatch.Success)
                    {
                        string filterName = filterNameMatch.Groups[1].Value;
                        List<string> filterArgs = R.Scan(f,
                                                         string.Format(R.Q(@"(?:{0}|{1})\s*({2})"),
                                                                       Liquid.FilterArgumentSeparator,
                                                                       Liquid.ArgumentSeparator, Liquid.QuotedFragment));
                        filters.Add(new FilterRequest(filterName, filterArgs.ToArray()));
                    }
                }
            }
            return filters;
        }

        public class MarkupParserResult
        {
            private readonly List<FilterRequest> _filters = new List<FilterRequest>();

            public string Name { get; set; }
            public IList<FilterRequest> Filters { get { return _filters; } }

            public void AddFilter(FilterRequest filter)
            {
                _filters.Add(filter);
            }

            public void AddFilters(IList<FilterRequest> filters)
            {
                _filters.AddRange(filters);
            }
        }

    }
}