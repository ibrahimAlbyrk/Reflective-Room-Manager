using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Filters
{
    /// <summary>
    /// Filters messages for banned words using regex word-boundary matching.
    /// </summary>
    public class WordFilter : IChatFilter
    {
        private readonly HashSet<string> _bannedWords;
        private readonly WordFilterMode _mode;
        private readonly Dictionary<string, Regex> _regexCache;

        public WordFilter(IEnumerable<string> bannedWords, WordFilterMode mode)
        {
            _bannedWords = new HashSet<string>(bannedWords, System.StringComparer.OrdinalIgnoreCase);
            _mode = mode;
            _regexCache = new Dictionary<string, Regex>();

            foreach (var word in _bannedWords)
            {
                var pattern = $@"\b{Regex.Escape(word)}\b";
                _regexCache[word] = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
        }

        public FilterResult Filter(string content)
        {
            if (string.IsNullOrEmpty(content))
                return FilterResult.PassThrough(content);

            bool wasModified = false;
            string filtered = content;

            foreach (var word in _bannedWords)
            {
                if (!_regexCache.TryGetValue(word, out var regex))
                    continue;

                if (!regex.IsMatch(filtered))
                    continue;

                wasModified = true;

                switch (_mode)
                {
                    case WordFilterMode.Block:
                        return FilterResult.Blocked(content);

                    case WordFilterMode.Censor:
                        filtered = regex.Replace(filtered, match => new string('*', match.Length));
                        break;

                    case WordFilterMode.Remove:
                        filtered = regex.Replace(filtered, string.Empty);
                        break;
                }
            }

            return new FilterResult
            {
                FilteredContent = filtered,
                WasModified = wasModified,
                WasCensored = wasModified && _mode == WordFilterMode.Censor,
                ShouldBlock = false
            };
        }
    }
}
