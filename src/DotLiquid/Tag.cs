using System;
using System.Collections.Generic;
using System.IO;

namespace DotLiquid
{
	public class Tag : IRenderable
	{
		public List<object> NodeList { get; protected set; }
		protected string TagName { get; private set; }
		protected string Markup { get; private set; }

		/// <summary>
		/// Only want to allow Tags to be created in inherited classes or tests.
		/// </summary>
		protected internal Tag()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class by copying values
        /// from an existing Tag class. Creates a new NodeList that contains references
        /// to the same nodes as the original one, but that can be modified independently.
        /// </summary>
        /// <param name="tag">The Tag to copy.</param>
        protected Tag(Tag tag)
        {
            if (tag.NodeList != null) NodeList = new List<object>(tag.NodeList);
            TagName = tag.TagName;
            Markup = tag.Markup;
        }

		internal virtual void AssertTagRulesViolation(List<object> rootNodeList)
		{
		}

		public virtual void Initialize(string tagName, string markup, List<string> tokens)
		{
			TagName = tagName;
			Markup = markup;
			Parse(tokens);
		}
        
		protected virtual void Parse(List<string> tokens)
		{
		}

		public string Name
		{
			get { return GetType().Name.ToLower(); }
		}

		public virtual void Render(Context context, TextWriter result)
		{
		}

		/// <summary>
		/// Primarily intended for testing.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		internal string Render(Context context)
		{
			using (TextWriter result = new StringWriter())
			{
				Render(context, result);
				return result.ToString();
			}
		}
	}
}