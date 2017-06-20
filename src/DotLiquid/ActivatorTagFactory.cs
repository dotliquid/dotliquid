using System;

namespace DotLiquid
{
    /// <summary>
    /// Tag factory using System.Activator to instanciate the tag.
    /// </summary>
    public class ActivatorTagFactory : ITagFactory
    {
        private readonly Type _tagType;

        private readonly string _tagName;

        /// <summary>
        /// Name of the tag
        /// </summary>
        public string TagName { get { return _tagName; } }

        /// <summary>
        /// Instanciates a new ActivatorTagFactory
        /// </summary>
        /// <param name="tagType">Name of the tag</param>
        /// <param name="tagName">Type of the tag. must inherit from DotLiquid.Tag.</param>
        public ActivatorTagFactory(Type tagType, string tagName)
        {
            _tagType = tagType;
            _tagName = tagName;
        }

        /// <summary>
        /// Creates the tag
        /// </summary>
        /// <returns></returns>
        public Tag Create()
        {
            return (Tag)Activator.CreateInstance(_tagType);
        }
    }
}
