namespace REFLECTIVE.Runtime.NETWORK.Chat.Filters
{
    /// <summary>
    /// Result of a content filter operation.
    /// </summary>
    public struct FilterResult
    {
        /// <summary>Content after filtering</summary>
        public string FilteredContent;

        /// <summary>Whether the content was modified at all</summary>
        public bool WasModified;

        /// <summary>Whether content was censored (replaced with asterisks)</summary>
        public bool WasCensored;

        /// <summary>Whether the message should be completely blocked</summary>
        public bool ShouldBlock;

        /// <summary>Creates a pass-through result (no changes)</summary>
        public static FilterResult PassThrough(string content)
        {
            return new FilterResult
            {
                FilteredContent = content,
                WasModified = false,
                WasCensored = false,
                ShouldBlock = false
            };
        }

        /// <summary>Creates a blocked result</summary>
        public static FilterResult Blocked(string originalContent)
        {
            return new FilterResult
            {
                FilteredContent = originalContent,
                WasModified = false,
                WasCensored = false,
                ShouldBlock = true
            };
        }
    }
}
