using System.Collections.Generic;
using System.IO;

namespace DotLiquid
{
    /// <summary>
    /// Represents a tag in Liquid:
    /// {% cycle 'one', 'two', 'three' %}
    /// </summary>
    public class Tag : IRenderable
    {
        /// <summary>
        /// List of the nodes composing the tag
        /// </summary>
        public List<object> NodeList { get; protected set; }

        /// <summary>
        /// Name of the tag
        /// </summary>
        protected string TagName { get; private set; }

        /// <summary>
        /// Content of the tag node except the name.
        /// E.g. for {% tablerow n in numbers cols:3%} {{n}} {% endtablerow %}
        /// It is "n in numbers cols:3"
        /// </summary>
        protected string Markup { get; private set; }

        /// <summary>
        /// Only want to allow Tags to be created in inherited classes or tests.
        /// </summary>
        protected internal Tag()
        {
        }

        internal virtual void AssertTagRulesViolation(List<object> rootNodeList)
        {
        }

        /// <summary>
        /// Initializes the tag
        /// </summary>
        /// <param name="tagName">Name of the parsed tag</param>
        /// <param name="markup">Markup of the parsed tag</param>
        /// <param name="tokens">Tokens of the parsed tag</param>
        public virtual void Initialize(string tagName, string markup, List<string> tokens)
        {
            TagName = tagName;
            Markup = markup;
            Parse(tokens);
        }

        /// <summary>
        /// Parses the tag
        /// </summary>
        /// <param name="tokens"></param>
        protected virtual void Parse(List<string> tokens)
        {
        }

        /// <summary>
        /// Name of the tag, usually the type name in lowercase
        /// </summary>
        public string Name
        {
            get { return GetType().Name.ToLower(); }
        }

        /// <summary>
        /// Renders the tag
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
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
            using (TextWriter result = new StringWriter(context.FormatProvider))
            {
                Render(context, result);
                return result.ToString();
            }
        }
    }
}
