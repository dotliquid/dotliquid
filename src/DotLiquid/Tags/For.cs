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
	///      <div {% if forloop.first %}class="first"{% endif %}>
	///        Item {{ forloop.index }}: {{ item.name }}
	///      </div>
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

		private string _variableName, _collectionName, _name;
		private bool _reversed;

		private bool _hasLimit;
		private bool _hasOffset;
		private string _limitAttribute;
		private string _offsetAttribute;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match match = Syntax.Match(markup);
			if (match.Success)
			{
				_variableName = match.Groups[1].Value;
				_collectionName = match.Groups[2].Value;
				_name = string.Format("{0}-{1}", _variableName, _collectionName);
				_reversed = (!string.IsNullOrEmpty(match.Groups[3].Value));
				var attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
				R.Scan(markup, TagAttributesRegex, (key, value) => attributes[key] = value);

				_hasOffset = attributes.TryGetValue("offset", out _offsetAttribute);
				_hasLimit = attributes.TryGetValue("limit", out _limitAttribute);
			}
			else
			{
				throw new SyntaxException(Liquid.ResourceManager.GetString("ForTagSyntaxException"));
			}

			base.Initialize(tagName, markup, tokens);
		}

		public override ReturnCode Render(Context context, TextWriter result)
		{
			object forRegister = context.Registers["for"];
			if (forRegister == null)
			{
				forRegister = new Hash(0);
				context.Registers["for"] = forRegister;
			}

			object collection = context[_collectionName];

			var enumerable = collection as IEnumerable;
			if (enumerable == null)
				return ReturnCode.Return;

			int from = 0;
			if (_hasOffset)
			{
				from = (_offsetAttribute == "continue")
					? Convert.ToInt32(context.Registers.Get<Hash>("for")[_name])
					: Convert.ToInt32(context[_offsetAttribute]);
			}

			int? limit = _hasLimit
						? context[_limitAttribute] as int?
						: null;

			int? to = (limit != null) ? (int?) (limit.Value + from) : null;

			List<object> segment = SliceCollectionUsingEach(enumerable, from, to);

			if (!segment.Any())
				return ReturnCode.Return;

			if (_reversed)
				segment.Reverse();

			int length = segment.Count;

			// Store our progress through the collection for the continue flag
			context.Registers.Get<Hash>("for")[_name] = from + length;

            return context.Stack(() =>
            {
                for (var index = 0; index < segment.Count; ++index)
                {
                    context[_variableName] = segment[index];

                    var forHash = new Hash();

                    forHash["name"] = _name;
                    forHash["length"] = length;
                    forHash["index"] = index + 1;
                    forHash["index0"] = index;
                    forHash["rindex"] = length - index;
                    forHash["rindex0"] = length - index - 1;
                    forHash["first"] = (index == 0);
                    forHash["last"] = (index == length - 1);

                    context["forloop"] = forHash;

                    if (RenderAll(NodeList, context, result) == ReturnCode.Break)
                        break;
                }

                return ReturnCode.Return;
            });
		}

		private static List<object> SliceCollectionUsingEach(IEnumerable collection, int from, int? to)
		{
			List<object> segments = new List<object>();
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