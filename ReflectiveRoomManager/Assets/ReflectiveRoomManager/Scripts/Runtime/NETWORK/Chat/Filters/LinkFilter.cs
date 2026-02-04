using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace REFLECTIVE.Runtime.NETWORK.Chat.Filters
{
    /// <summary>
    /// Filters URLs from messages, optionally allowing whitelisted domains.
    /// </summary>
    public class LinkFilter : IChatFilter
    {
        private const string LinkRemovedText = "[LINK REMOVED]";

        private readonly HashSet<string> _allowedDomains;

        private static readonly Regex UrlRegex = new(
            @"(https?:\/\/)?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public LinkFilter(IEnumerable<string> allowedDomains)
        {
            _allowedDomains = new HashSet<string>(allowedDomains, StringComparer.OrdinalIgnoreCase);
        }

        public FilterResult Filter(string content)
        {
            if (string.IsNullOrEmpty(content))
                return FilterResult.PassThrough(content);

            var matches = UrlRegex.Matches(content);
            if (matches.Count == 0)
                return FilterResult.PassThrough(content);

            bool wasModified = false;
            string filtered = content;

            foreach (Match match in matches)
            {
                if (IsAllowedUrl(match.Value))
                    continue;

                filtered = filtered.Replace(match.Value, LinkRemovedText);
                wasModified = true;
            }

            return new FilterResult
            {
                FilteredContent = filtered,
                WasModified = wasModified,
                WasCensored = wasModified,
                ShouldBlock = false
            };
        }

        private bool IsAllowedUrl(string url)
        {
            if (_allowedDomains.Count == 0)
                return false;

            foreach (var domain in _allowedDomains)
            {
                if (url.Contains(domain, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
