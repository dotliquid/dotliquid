namespace DotLiquid
{
    /// <summary>
    /// Defines an interface for indexable objects, allowing access to values by key.
    /// </summary>
    public interface IIndexable
    {
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The value associated with the specified key, or null if the key is not found.</returns>
        object this[object key] { get; }

        /// <summary>
        /// Determines whether the indexable object contains a value with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>true if the indexable object contains a value with the specified key; otherwise, false.</returns>
        bool ContainsKey(object key);
    }
}
