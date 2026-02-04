using System;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    /// <summary>
    /// Fluent builder for constructing room filters.
    /// Use RoomFilter.Create().WithXXX().Build() pattern.
    /// </summary>
    public sealed class RoomFilter
    {
        private string _textSearch;
        private string _gameMode;
        private string _region;
        private string _sceneName;
        private int? _minPlayers;
        private int? _maxPlayers;
        private bool? _isPrivate;
        private bool? _excludeFull;
        private bool? _excludeEmpty;
        private readonly Dictionary<string, string> _customDataFilters;

        private RoomFilter()
        {
            _customDataFilters = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates a new RoomFilter builder instance.
        /// </summary>
        public static RoomFilter Create()
        {
            return new RoomFilter();
        }

        /// <summary>
        /// Filters rooms by partial room name match (case-insensitive).
        /// </summary>
        public RoomFilter WithTextSearch(string searchText)
        {
            _textSearch = searchText;
            return this;
        }

        /// <summary>
        /// Filters rooms by game mode (exact match).
        /// Typically stored in CustomData["gameMode"].
        /// </summary>
        public RoomFilter WithGameMode(string gameMode)
        {
            _gameMode = gameMode;
            return this;
        }

        /// <summary>
        /// Filters rooms by region (exact match).
        /// Typically stored in CustomData["region"].
        /// </summary>
        public RoomFilter WithRegion(string region)
        {
            _region = region;
            return this;
        }

        /// <summary>
        /// Filters rooms by scene name (exact match).
        /// </summary>
        public RoomFilter WithScene(string sceneName)
        {
            _sceneName = sceneName;
            return this;
        }

        /// <summary>
        /// Filters rooms with at least this many players.
        /// </summary>
        public RoomFilter MinPlayers(int min)
        {
            if (min < 0)
                throw new ArgumentOutOfRangeException(nameof(min), "Min players cannot be negative");

            _minPlayers = min;
            return this;
        }

        /// <summary>
        /// Filters rooms with at most this many players.
        /// </summary>
        public RoomFilter MaxPlayers(int max)
        {
            if (max < 0)
                throw new ArgumentOutOfRangeException(nameof(max), "Max players cannot be negative");

            _maxPlayers = max;
            return this;
        }

        /// <summary>
        /// Filters rooms by privacy status.
        /// True = only private, False = only public, null = both.
        /// </summary>
        public RoomFilter IsPrivate(bool isPrivate)
        {
            _isPrivate = isPrivate;
            return this;
        }

        /// <summary>
        /// Excludes full rooms if true.
        /// </summary>
        public RoomFilter ExcludeFull(bool exclude = true)
        {
            _excludeFull = exclude;
            return this;
        }

        /// <summary>
        /// Excludes empty rooms if true.
        /// </summary>
        public RoomFilter ExcludeEmpty(bool exclude = true)
        {
            _excludeEmpty = exclude;
            return this;
        }

        /// <summary>
        /// Filters by custom data key-value pair (exact match).
        /// Can be chained multiple times for multiple filters.
        /// </summary>
        public RoomFilter WithCustomData(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Custom data key cannot be null or empty", nameof(key));

            _customDataFilters[key] = value;
            return this;
        }

        /// <summary>
        /// Builds the immutable filter data structure.
        /// </summary>
        public RoomFilterData Build()
        {
            return new RoomFilterData
            {
                TextSearch = _textSearch,
                GameMode = _gameMode,
                Region = _region,
                SceneName = _sceneName,
                MinPlayers = _minPlayers,
                MaxPlayers = _maxPlayers,
                IsPrivate = _isPrivate,
                ExcludeFull = _excludeFull,
                ExcludeEmpty = _excludeEmpty,
                CustomDataFilters = new Dictionary<string, string>(_customDataFilters)
            };
        }

        /// <summary>
        /// Tests if a room matches this filter.
        /// Useful for client-side filtering.
        /// </summary>
        public bool Matches(Room room)
        {
            var data = Build();
            return RoomDiscoveryService.MatchesFilter(room, data);
        }
    }
}
