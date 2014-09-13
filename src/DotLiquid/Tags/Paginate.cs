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
    /// 
    /// == Exposed variables within the paginate tags:
    /// 
    ///    paginate.items -> returned paged items
    ///    paginate.pages -> total page count
    ///    paginate.previous -> previous true or false
    ///    paginate.next -> next page true or false
    ///    paginate.size -> total records
    ///    paginate.current_offset -> offset of paged records
    ///    paginate.current_page -> the current page number
    ///    paginate.page_size -> records per page
    /// 
    /// == Example full markup with paging tags and current_page tag:
    /// 
    ///   {% current_page = 1 %}
    ///   {% paginate collection by 2 %}
    ///
    ///     {% for item in paginate.items %}
    ///       {{ item.name }}
    ///     {% endfor %}
    ///   
    ///     {% if paginate.previous %}
    ///       {% capture prev_page %}/?page={{ paginate.current_page | minus:1 }}{% endcapture %}
    ///       <a href="{{ prev_page }}">Previous</a>
    ///     {% endif %}
    ///   
    ///     Showing items {{ paginate.current_offset | plus: 1 }}-{% if paginate.next %}{{ paginate.current_offset | plus: paginate.page_size }}{% else %}{{ paginate.size }}{% endif %} of {{ paginate.size }}.
    ///   
    ///     {% if paginate.next %}
    ///       {% capture next_page %}/?page={{ paginate.current_page | plus:1 }}{% endcapture %}
    ///       <a href="{{ next_page }}">Next</a>
    ///     {% endif %}
    ///   			
    ///   {% endpaginate %}
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

                if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    if (!Int32.TryParse(match.Groups[2].Value, out _pageSize))
                        _pageSize = 20;

                _attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);

                R.Scan(markup, Liquid.TagAttributes, (key, value) => _attributes[key] = value);
            }
            else
            {
                throw new SyntaxException(Liquid.ResourceManager.GetString("PaginateSyntaxException"));
            }

            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {
            context.Registers["paginate"] = context.Registers["paginate"] ?? new Hash(0);

            if (context.Registers["current_page"] != null)
                if (!Int32.TryParse(context.Registers["current_page"].ToString(), out _currentPage))
                    _currentPage = 1;

            if (_currentPage == 0)
                _currentPage = 1;

            object collection = context[_collectionName];

            if (!(collection is IEnumerable))
                return;

            int from = ((_currentPage - 1) * _pageSize);
            int? to = from + _pageSize;

            var segment = SliceCollectionUsingEach((IEnumerable)collection, from, to);

            int length = collection != null ? ((ICollection)collection).Count : 0;

            var pageCount = Math.Ceiling((double)length / _pageSize);

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