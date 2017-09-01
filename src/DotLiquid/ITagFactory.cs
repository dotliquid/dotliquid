namespace DotLiquid
{
    /// <summary>
    /// Interface for tag factory.
    /// </summary>
    /// <remarks>Can be usefull when the tag needs a parameter and can't be created with parameterless constructor.</remarks>
    public interface ITagFactory
    {
        /// <summary>
        /// Name of the tag
        /// </summary>
        string TagName { get; }

        /// <summary>
        /// Creates the tag
        /// </summary>
        /// <returns></returns>
        Tag Create();
    }
}
