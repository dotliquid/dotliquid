using System;

namespace DotLiquid
{
    public class ActivatorTagFactory : ITagFactory
    {
        private Type _tagType;

        private string _tagName;

        public string TagName { get { return _tagName; } }

        public ActivatorTagFactory(Type tagType, string tagName)
        {
            _tagType = tagType;
            _tagName = tagName;
        }

        public Tag Create()
        {
            return (Tag)Activator.CreateInstance(_tagType);
        }
    }
}
