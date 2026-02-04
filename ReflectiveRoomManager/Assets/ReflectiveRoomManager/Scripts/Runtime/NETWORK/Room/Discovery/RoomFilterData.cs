using System;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    /// <summary>
    /// Immutable filter data structure for network transmission.
    /// </summary>
    [Serializable]
    public struct RoomFilterData
    {
        public string TextSearch;
        public string GameMode;
        public string Region;
        public string SceneName;
        public int? MinPlayers;
        public int? MaxPlayers;
        public bool? IsPrivate;
        public bool? ExcludeFull;
        public bool? ExcludeEmpty;
        public Dictionary<string, string> CustomDataFilters;

        /// <summary>
        /// Checks if filter has any criteria.
        /// </summary>
        public readonly bool IsEmpty()
        {
            return string.IsNullOrEmpty(TextSearch) &&
                   string.IsNullOrEmpty(GameMode) &&
                   string.IsNullOrEmpty(Region) &&
                   string.IsNullOrEmpty(SceneName) &&
                   !MinPlayers.HasValue &&
                   !MaxPlayers.HasValue &&
                   !IsPrivate.HasValue &&
                   !ExcludeFull.HasValue &&
                   !ExcludeEmpty.HasValue &&
                   (CustomDataFilters == null || CustomDataFilters.Count == 0);
        }
    }
}
