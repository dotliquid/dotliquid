using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// Deep copies this instance by deep copying all ICopyable in the NodeList
        /// (and reference copying all others). Any Tag that maintains internal state
        /// should override this method (making sure to call the base version to get
        /// the initial reference). All overriding methods should deep copy members
        /// that are potentially modified during the render phase (all members are
        /// automatically shallow copied using MemberwiseClone).
        /// </summary>
        /// <returns></returns>
        public virtual object Copy()
        {
            Tag tag = (Tag) MemberwiseClone();
            if (NodeList != null) tag.NodeList = new List<object>(NodeList.Select(n =>
                {
                    ICopyable copyable = n as ICopyable;
                    return copyable == null ? n : copyable.Copy();
                }));
            return tag;
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