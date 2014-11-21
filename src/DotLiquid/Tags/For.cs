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
		protected static readonly Regex Syntax = R.B(R.Q(@"(\w+)\s+in\s+({0}+)\s*(reversed)?"), Liquid.QuotedFragment);

		protected string VariableName, CollectionName, Name;
		protected bool Reversed;
		protected Dictionary<string, string> Attributes;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match match = Syntax.Match(markup);
			if (match.Success)
			{
				VariableName = match.Groups[1].Value;
				CollectionName = match.Groups[2].Value;
				Name = string.Format("{0}-{1}", VariableName, CollectionName);
				Reversed = (!string.IsNullOrEmpty(match.Groups[3].Value));
				Attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
				R.Scan(markup, Liquid.TagAttributes,
					(key, value) => Attributes[key] = value);
			}
			else
			{
				throw new SyntaxException(Liquid.ResourceManager.GetString("ForTagSyntaxException"));
			}

			base.Initialize(tagName, markup, tokens);
		}

		public override void Render(Context context, TextWriter result)
		{
			context.Registers["for"] = context.Registers["for"] ?? new Hash(0);

			object collection = context[CollectionName];

			if (!(collection is IEnumerable))
				return;

			int from = (Attributes.ContainsKey("offset"))
				? (Attributes["offset"] == "continue")
					? Convert.ToInt32(context.Registers.Get<Hash>("for")[Name])
					: Convert.ToInt32(context[Attributes["offset"]])
				: 0;

			int? limit = Attributes.ContainsKey("limit") ? context[Attributes["limit"]] as int? : null;
			int? to = (limit != null) ? (int?) (limit.Value + from) : null;

			List<object> segment = SliceCollectionUsingEach((IEnumerable) collection, from, to);

			if (!segment.Any())
				return;

			if (Reversed)
				segment.Reverse();

			int length = segment.Count;

			// Store our progress through the collection for the continue flag
			context.Registers.Get<Hash>("for")[Name] = from + length;

			context.Stack(() => segment.EachWithIndex((item, index) =>
			{
				context[VariableName] = item;
				context["forloop"] = Hash.FromAnonymousObject(new
				{
					name = Name,
					length = length,
					index = index + 1,
					index0 = index,
					rindex = length - index,
					rindex0 = length - index - 1,
					first = (index == 0),
					last = (index == length - 1)
				});
				RenderAll(NodeList, context, result);
			}));
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