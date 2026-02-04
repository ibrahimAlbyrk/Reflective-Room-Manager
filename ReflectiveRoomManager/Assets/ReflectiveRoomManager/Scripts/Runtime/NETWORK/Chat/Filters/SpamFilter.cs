using System.Text.RegularExpressions;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Filters
{
    /// <summary>
    /// Reduces repeated consecutive characters to prevent spam.
    /// Example: "hiiiiiii" becomes "hiii" (with maxRepeatedChars = 3)
    /// </summary>
    public class SpamFilter : IChatFilter
    {
        private readonly int _maxRepeatedChars;
        private readonly Regex _repeatedCharRegex;

        public SpamFilter(int maxRepeatedChars)
        {
            _maxRepeatedChars = maxRepeatedChars > 0 ? maxRepeatedChars : 3;

            // Pattern: any character repeated more than maxRepeatedChars times
            var pattern = $@"(.)\1{{{_maxRepeatedChars},}}";
            _repeatedCharRegex = new Regex(pattern, RegexOptions.Compiled);
        }

        public FilterResult Filter(string content)
        {
            if (string.IsNullOrEmpty(content))
                return FilterResult.PassThrough(content);

            if (!_repeatedCharRegex.IsMatch(content))
                return FilterResult.PassThrough(content);

            var filtered = _repeatedCharRegex.Replace(content, match =>
                new string(match.Value[0], _maxRepeatedChars)
            );

            return new FilterResult
            {
                FilteredContent = filtered,
                WasModified = true,
                WasCensored = false,
                ShouldBlock = false
            };
        }
    }
}
