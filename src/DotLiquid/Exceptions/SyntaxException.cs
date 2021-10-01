using System;

namespace DotLiquid.Exceptions
{
    /// <summary>
    /// An exception that is thrown when an invalid or unknown syntax is encountered in a template.
    /// </summary>
#if !CORE
    [Serializable]
#endif
    public class SyntaxException : LiquidException
    {
        /// <summary>
        /// Raise with a resource key, or custom message.
        /// </summary>
        /// <param name="message">Either a key specified in <c>Resources.resx</c> or a bespoke error message</param>
        /// <param name="args">Optional list of strings that are used to replace numbers tokens in the message</param>
        public SyntaxException(string message, params string[] args)
            : base(SyntaxException.formatMessage(message, args))
        {
        }

        /// <summary>
        /// Check if the message is a resource key, if so replace with the resource value then format the message.
        /// </summary>
        private static string formatMessage(string message, params string[] args)
        {
            var resourceValue = Liquid.ResourceManager.GetString(message);
            return string.Format(resourceValue.IsNullOrWhiteSpace() ? message : resourceValue, args);
        }
    }
}
