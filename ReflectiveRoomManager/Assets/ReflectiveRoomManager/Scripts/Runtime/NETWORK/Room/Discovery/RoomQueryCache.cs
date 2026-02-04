using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    /// <summary>
    /// Server-side query result cache with TTL.
    /// </summary>
    public class RoomQueryCache
    {
        private readonly Dictionary<string, CachedQueryResult> _cache;
        private readonly float _ttl;
        private readonly StringBuilder _keyBuilder;

        public RoomQueryCache(float ttl = 5f)
        {
            _cache = new Dictionary<string, CachedQueryResult>();
            _ttl = ttl;
            _keyBuilder = new StringBuilder(256);
        }

        /// <summary>
        /// Tries to get cached result.
        /// </summary>
        public bool TryGetCached(string cacheKey, out CachedQueryResult result)
        {
            if (_cache.TryGetValue(cacheKey, out result))
            {
                if (!result.IsExpired(Time.time))
                    return true;

                _cache.Remove(cacheKey);
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Caches a query result.
        /// </summary>
        public void Cache(string cacheKey, RoomQueryResponse response)
        {
            var result = new CachedQueryResult(response, Time.time, _ttl);
            _cache[cacheKey] = result;
        }

        /// <summary>
        /// Invalidates all cached results.
        /// </summary>
        public void Invalidate()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Invalidates a specific cached query.
        /// </summary>
        public void Invalidate(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// Generates cache key from request.
        /// </summary>
        public string GenerateCacheKey(RoomQueryRequest request)
        {
            _keyBuilder.Clear();

            var filter = request.Filter;

            _keyBuilder.Append(filter.TextSearch ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(filter.GameMode ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(filter.Region ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(filter.SceneName ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(filter.MinPlayers?.ToString() ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(filter.MaxPlayers?.ToString() ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(filter.IsPrivate?.ToString() ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(filter.ExcludeFull?.ToString() ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(filter.ExcludeEmpty?.ToString() ?? string.Empty);
            _keyBuilder.Append('|');
            _keyBuilder.Append(request.SortBy.ToString());
            _keyBuilder.Append('|');
            _keyBuilder.Append(request.PageNumber);
            _keyBuilder.Append('|');
            _keyBuilder.Append(request.PageSize);

            if (filter.CustomDataFilters != null && filter.CustomDataFilters.Count > 0)
            {
                foreach (var kvp in filter.CustomDataFilters.OrderBy(k => k.Key))
                {
                    _keyBuilder.Append('|');
                    _keyBuilder.Append(kvp.Key);
                    _keyBuilder.Append('=');
                    _keyBuilder.Append(kvp.Value);
                }
            }

            return _keyBuilder.ToString();
        }

        /// <summary>
        /// Cleans up expired entries.
        /// Called periodically by RoomManagerBase.
        /// </summary>
        public void CleanupExpired()
        {
            var currentTime = Time.time;
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired(currentTime))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
                _cache.Remove(key);
        }

        /// <summary>
        /// Gets the number of cached entries.
        /// </summary>
        public int Count => _cache.Count;
    }
}
