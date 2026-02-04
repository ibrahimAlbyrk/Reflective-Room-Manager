namespace REFLECTIVE.Runtime.NETWORK.Chat.Filters
{
    /// <summary>
    /// Interface for chat content filters.
    /// Implement this interface to create custom filters.
    /// </summary>
    public interface IChatFilter
    {
        /// <summary>
        /// Filters the content and returns the result.
        /// </summary>
        /// <param name="content">The original message content</param>
        /// <returns>Filter result with potentially modified content</returns>
        FilterResult Filter(string content);
    }
}
