using System;
using System.Collections.Generic;
using System.Text;

namespace DotLiquid
{
    internal class ConstructorTagFactory<T> : ITagFactory where T : Tag, new()
    {
        /// <inheritdoc />
        public string TagName { get; }

        /// <inheritdoc />
        public Type TagType => typeof(T);

        public ConstructorTagFactory(string tagName)
        {
            TagName = tagName;
        }

        /// <inheritdoc />
        public Tag Create() => new T();
    }
}
