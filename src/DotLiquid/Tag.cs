using System.Collections.Generic;
using System.IO;

namespace DotLiquid
{
	public abstract class Tag : IRenderable
	{
        public List<IRenderable> NodeList { get; protected set; }
		protected string TagName { get; private set; }
		protected string Markup { get; private set; }

		/// <summary>
		/// Only want to allow Tags to be created in inherited classes or tests.
		/// </summary>
		protected internal Tag()
		{
		}

		internal virtual void AssertTagRulesViolation(List<IRenderable> rootNodeList)
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

		public abstract ReturnCode Render(Context context, TextWriter result);
	}
}