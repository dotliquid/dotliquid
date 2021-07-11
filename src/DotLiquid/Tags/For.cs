using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    /// <summary>
    /// "For" iterates over an array or collection.
    /// Several useful variables are available to you within the loop.
    ///
    /// == Basic usage:
    ///    {% for item in collection %}
    ///      {{ forloop.index }}: {{ item.name }}
    ///    {% endfor %}
    ///
    /// == Advanced usage:
    ///    {% for item in collection %}
    ///      &lt;div {% if forloop.first %}class="first"{% endif %}&gt;
    ///        Item {{ forloop.index }}: {{ item.name }}
    ///      &lt;/div&gt;
    ///    {% else %}
    ///      There is nothing in the collection.
    ///    {% endfor %}
    ///
    /// You can also define a limit and offset much like SQL.  Remember
    /// that offset starts at 0 for the first item.
    ///
    ///    {% for item in collection limit:5 offset:10 %}
    ///      {{ item.name }}
    ///    {% end %}
    ///
    ///  To reverse the for loop simply use {% for item in collection reversed %}
    ///
    /// == Available variables:
    ///
    /// forloop.name:: 'item-collection'
    /// forloop.length:: Length of the loop
    /// forloop.index:: The current item's position in the collection;
    ///                 forloop.index starts at 1.
    ///                 This is helpful for non-programmers who start believe
    ///                 the first item in an array is 1, not 0.
    /// forloop.index0:: The current item's position in the collection
    ///                  where the first item is 0
    /// forloop.rindex:: Number of items remaining in the loop
    ///                  (length - index) where 1 is the last item.
    /// forloop.rindex0:: Number of items remaining in the loop
    ///                   where 0 is the last item.
    /// forloop.first:: Returns true if the item is the first item.
    /// forloop.last:: Returns true if the item is the last item.
    /// </summary>
    public class For : DotLiquid.Block
    {
        private static readonly Regex Syntax = R.B(R.Q(@"(\w+)\s+in\s+({0}+)\s*(reversed)?"), Liquid.QuotedFragment);
        private static string ForTagMaxIterationsExceededException = Liquid.ResourceManager.GetString("ForTagMaximumIterationsExceededException");

        private string _variableName, _collectionName, _name;
        private bool _reversed;
        private Dictionary<string, string> _attributes;

        private List<object> ForBlock { get; set; }
        private Condition ElseBlock { get; set; }

        /// <summary>
        /// Initializes the for tag
        /// </summary>
        /// <param name="tagName">Name of the parsed tag</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Tokens of the parsed tag</param>
        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match match = Syntax.Match(markup);
            if (match.Success)
            {
                NodeList = ForBlock = new List<object>();
                _variableName = match.Groups[1].Value;
                _collectionName = match.Groups[2].Value;
                _name = string.Format("{0}-{1}", _variableName, _collectionName);
                _reversed = (!string.IsNullOrEmpty(match.Groups[3].Value));
                _attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
                R.Scan(markup, Liquid.TagAttributes,
                    (key, value) => _attributes[key] = value);
            }
            else
            {
                throw new SyntaxException(Liquid.ResourceManager.GetString("ForTagSyntaxException"));
            }

            base.Initialize(tagName, markup, tokens);
        }

        /// <summary>
        /// Handles the else tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="markup"></param>
        /// <param name="tokens"></param>
        public override void UnknownTag(string tag, string markup, List<string> tokens)
        {
            if (tag == "else")
            {
                ElseBlock = new ElseCondition();
                NodeList = ElseBlock.Attach(new List<object>());
                return;
            }

            base.UnknownTag(tag, markup, tokens);
        }

        /// <summary>
        /// Renders the for tag
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public override void Render(Context context, TextWriter result)
        {
            // treat non IEnumerable as empty
            if (!(context[_collectionName] is IEnumerable collection))
            {
                if (ElseBlock != null)
                    context.Stack(() =>
                    {
                        RenderAll(ElseBlock.Attachment, context, result);
                    });
                return;
            }

            var register = GetRegister<object>(context, "for");
            int from = (_attributes.ContainsKey("offset"))
                ? (_attributes["offset"] == "continue")
                    ? Convert.ToInt32(register[_name])
                    : Convert.ToInt32(context[_attributes["offset"]])
                : 0;

            int? limit = _attributes.ContainsKey("limit") ? (int?)Convert.ToInt32(context[_attributes["limit"]]) : null;
            int? to = (limit != null) ? (int?)(limit.Value + from) : null;

            List<object> segment = SliceCollectionUsingEach(context, collection, from, to);

            if (_reversed)
                segment.Reverse();

            int length = segment.Count;

            // Store our progress through the collection for the continue flag
            register[_name] = from + length;

            context.Stack(() =>
            {
                if (!segment.Any())
                {
                    if (ElseBlock != null)
                        RenderAll(ElseBlock.Attachment, context, result);
                    return;
                }

                for (var index = 0; index < segment.Count; index++)
                {
                    context.CheckTimeout();

                    var item = segment[index];
                    if (item is KeyValuePair<string, object> pair)
                    {
                        var itemKey = pair.Key;
                        var itemValue = pair.Value;
                        BuildContext(context, _variableName, itemKey, itemValue);

                    }
                    else
                        context[_variableName] = item;

                    context["forloop"] = new Dictionary<string, object>
                    {
                        ["name"] = _name,
                        ["length"] = length,
                        ["index"] = index + 1,
                        ["index0"] = index,
                        ["rindex"] = length - index,
                        ["rindex0"] = length - index - 1,
                        ["first"] = (index == 0),
                        ["last"] = (index == length - 1)
                    };
                    try
                    {
                        RenderAll(ForBlock, context, result);
                    }
                    catch (BreakInterrupt)
                    {
                        break;
                    }
                    catch (ContinueInterrupt)
                    {
                        // ContinueInterrupt is used only to skip the current value but not to stop the iteration
                    }
                }
            });
        }

        private static List<object> SliceCollectionUsingEach(Context context, IEnumerable collection, int from, int? to)
        {
            List<object> segments = new List<object>();
            int index = 0;
            foreach (object item in collection)
            {
                context.CheckTimeout();

                if (to != null && to.Value <= index)
                    break;

                if (from <= index)
                    segments.Add(item);

                ++index;

                if (context.MaxIterations > 0 && index > context.MaxIterations)
                {
                    throw new MaximumIterationsExceededException(For.ForTagMaxIterationsExceededException, context.MaxIterations.ToString());
                }
            }
            return segments;
        }

        private void BuildContext(Context context, string parent, string key, object value)
        {
            if (value is IDictionary<string, object> dictionary)
            {
                dictionary["itemName"] = key;
                context[parent] = value;

                // Iterate entries and recursively call this method for any IDcitionary values.
                foreach (var entry in dictionary.Where<KeyValuePair<string, object>>(entry => entry.Value is IDictionary<string, object>))
                    BuildContext(context, parent + "." + key, entry.Key, entry.Value);
            }
        }
    }
}
