using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// Allows pagination of an array/collection. 
    ///
    /// == Basic usage:
    ///    {% paginate collection by 5 %}
    ///      {% for item in paginate.items %}
    ///         {{ forloop.index }}: {{ item.name }}
    ///      {% endfor %}
    ///    {% endpaginate %}
    /// </summary>
    public class Paginate : DotLiquid.Block
    {
        private static readonly Regex Syntax = R.B(R.Q(@"({0})\s*by\s*(\d+)?"), Liquid.QuotedFragment);

        private string _collectionName;
        private int _pageSize;
        private int _currentPage;
        private Dictionary<string, string> _attributes;

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            var match = Syntax.Match(markup);

            if (match.Success)
            {
                _collectionName = match.Groups[1].Value;
                _pageSize = !string.IsNullOrEmpty(match.Groups[2].Value) ? Convert.ToInt32(match.Groups[2].Value) : 20;
                _attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);

                R.Scan(markup, Liquid.TagAttributes, (key, value) => _attributes[key] = value);
            }
            else
            {
                throw new SyntaxException("Syntax Error in tag 'Paginate' - Valid syntax: paginate [collection] by number");
            }

            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {
            context.Registers["paginate"] = context.Registers["paginate"] ?? new Hash(0);

            // You'll need to make sure that you have already added the request querystring of page to the liquid hash
            _currentPage = context["request.query.page"] != null ? Convert.ToInt32(context["request.query.page"]) : 1;

            object collection = context[_collectionName];

            if (!(collection is IEnumerable))
                return;

            int from = ((_currentPage - 1) * _pageSize);
            int? to = from + _pageSize;

            var segment = SliceCollectionUsingEach((IEnumerable)collection, from, to);

            int length = collection != null ? ((ICollection)collection).Count : 0;

            var pageCount = Math.Ceiling((double)length / _pageSize);

            // These are the variables available within the paginate block example: {{ paginate.next }}
            context["paginate"] = Hash.FromAnonymousObject(new
            {
                items = segment,
                pages = pageCount,
                previous = (_currentPage > 1),
                next = (_currentPage < pageCount),
                size = length,
                current_offset = (_currentPage - 1) * _pageSize,
                current_page = _currentPage,
                page_size = _pageSize
            });

            RenderAll(NodeList, context, result);
        }

        private static List<object> SliceCollectionUsingEach(IEnumerable collection, int from, int? to)
        {
            var segments = new List<object>();

            int index = 0;

            foreach (object item in collection)
            {
                if (to != null && to.Value <= index)
                    break;

                if (from <= index)
                    segments.Add(item);

                ++index;
            }

            return segments;
        }
    }
}