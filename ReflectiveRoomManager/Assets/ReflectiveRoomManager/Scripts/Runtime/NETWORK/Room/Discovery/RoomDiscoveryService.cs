using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    using Structs;
    using Utilities;

    /// <summary>
    /// Server-side room discovery and filtering service.
    /// Handles query execution, caching, and delta updates.
    /// </summary>
    public class RoomDiscoveryService
    {
        private readonly List<Room> _allRooms;
        private readonly RoomQueryCache _queryCache;
        private uint _cacheVersion;

        public uint CacheVersion => _cacheVersion;
        public RoomQueryCache QueryCache => _queryCache;

        public RoomDiscoveryService(List<Room> roomList, float cacheTTL = 5f)
        {
            _allRooms = roomList ?? throw new ArgumentNullException(nameof(roomList));
            _queryCache = new RoomQueryCache(cacheTTL);
            _cacheVersion = 1;
        }

        /// <summary>
        /// Executes a room query with filtering, sorting, and pagination.
        /// </summary>
        public RoomQueryResponse ExecuteQuery(RoomQueryRequest request)
        {
            var cacheKey = _queryCache.GenerateCacheKey(request);
            if (_queryCache.TryGetCached(cacheKey, out var cachedResult))
            {
                return cachedResult.Response;
            }

            var filteredRooms = FilterRooms(request.Filter);
            var sortedRooms = SortRooms(filteredRooms, request.SortBy);

            var totalRooms = sortedRooms.Count;
            var pageSize = Mathf.Max(1, request.PageSize);
            var totalPages = Mathf.Max(1, Mathf.CeilToInt((float)totalRooms / pageSize));
            var paginatedRooms = PaginateRooms(sortedRooms, request.PageNumber, pageSize);

            var response = new RoomQueryResponse(
                paginatedRooms,
                totalRooms,
                totalPages,
                request.PageNumber,
                _cacheVersion,
                false
            );

            _queryCache.Cache(cacheKey, response);

            return response;
        }

        /// <summary>
        /// Filters rooms based on filter criteria.
        /// </summary>
        private List<Room> FilterRooms(RoomFilterData filter)
        {
            if (filter.IsEmpty())
                return _allRooms.ToList();

            return _allRooms.Where(room => MatchesFilter(room, filter)).ToList();
        }

        /// <summary>
        /// Static method for filter matching (usable by RoomFilter.Matches).
        /// </summary>
        public static bool MatchesFilter(Room room, RoomFilterData filter)
        {
            if (room == null) return false;

            // Text search (partial room name, case-insensitive)
            if (!string.IsNullOrEmpty(filter.TextSearch))
            {
                if (string.IsNullOrEmpty(room.Name) ||
                    !room.Name.Contains(filter.TextSearch, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Game mode (from CustomData)
            if (!string.IsNullOrEmpty(filter.GameMode))
            {
                var customData = room.GetCustomData();
                if (customData == null ||
                    !customData.TryGetValue("gameMode", out var gameMode) ||
                    gameMode != filter.GameMode)
                    return false;
            }

            // Region (from CustomData)
            if (!string.IsNullOrEmpty(filter.Region))
            {
                var customData = room.GetCustomData();
                if (customData == null ||
                    !customData.TryGetValue("region", out var region) ||
                    region != filter.Region)
                    return false;
            }

            // Scene name
            if (!string.IsNullOrEmpty(filter.SceneName))
            {
                if (room.Scene.name != filter.SceneName)
                    return false;
            }

            // Min players
            if (filter.MinPlayers.HasValue && room.CurrentPlayers < filter.MinPlayers.Value)
                return false;

            // Max players
            if (filter.MaxPlayers.HasValue && room.CurrentPlayers > filter.MaxPlayers.Value)
                return false;

            // Privacy
            if (filter.IsPrivate.HasValue && room.IsPrivate != filter.IsPrivate.Value)
                return false;

            // Exclude full
            if (filter.ExcludeFull == true && room.CurrentPlayers >= room.MaxPlayers)
                return false;

            // Exclude empty
            if (filter.ExcludeEmpty == true && room.CurrentPlayers == 0)
                return false;

            // Custom data filters
            if (filter.CustomDataFilters != null && filter.CustomDataFilters.Count > 0)
            {
                var customData = room.GetCustomData();
                if (customData == null) return false;

                foreach (var kvp in filter.CustomDataFilters)
                {
                    if (!customData.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sorts rooms based on sort option.
        /// </summary>
        private static List<Room> SortRooms(List<Room> rooms, RoomSortOptions sortBy)
        {
            return sortBy switch
            {
                RoomSortOptions.PlayerCountAsc => rooms.OrderBy(r => r.CurrentPlayers).ToList(),
                RoomSortOptions.PlayerCountDesc => rooms.OrderByDescending(r => r.CurrentPlayers).ToList(),
                RoomSortOptions.NameAsc => rooms.OrderBy(r => r.Name).ToList(),
                RoomSortOptions.NameDesc => rooms.OrderByDescending(r => r.Name).ToList(),
                RoomSortOptions.CreationTimeAsc => rooms.OrderBy(r => r.ID).ToList(),
                RoomSortOptions.CreationTimeDesc => rooms.OrderByDescending(r => r.ID).ToList(),
                _ => rooms
            };
        }

        /// <summary>
        /// Paginates rooms.
        /// </summary>
        private static RoomInfo[] PaginateRooms(List<Room> rooms, int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            return rooms.Skip(skip).Take(pageSize).Select(ConvertToRoomInfo).ToArray();
        }

        /// <summary>
        /// Converts Room to RoomInfo (network-safe struct).
        /// </summary>
        private static RoomInfo ConvertToRoomInfo(Room room)
        {
            return RoomListUtility.ConvertToRoomList(room);
        }

        #region Cache Invalidation & Delta Updates

        /// <summary>
        /// Invalidates all cached queries and increments version.
        /// </summary>
        public void InvalidateCache()
        {
            _queryCache.Invalidate();
            _cacheVersion++;
        }

        /// <summary>
        /// Called when a room is added.
        /// </summary>
        public void OnRoomAdded(Room room)
        {
            if (room == null)
            {
                Debug.LogWarning("[RoomDiscoveryService] OnRoomAdded called with null room");
                return;
            }
            InvalidateCache();
        }

        /// <summary>
        /// Called when a room is updated.
        /// </summary>
        public void OnRoomUpdated(Room room)
        {
            if (room == null)
            {
                Debug.LogWarning("[RoomDiscoveryService] OnRoomUpdated called with null room");
                return;
            }
            InvalidateCache();
        }

        /// <summary>
        /// Called when a room is removed.
        /// </summary>
        public void OnRoomRemoved(Room room)
        {
            if (room == null)
            {
                Debug.LogWarning("[RoomDiscoveryService] OnRoomRemoved called with null room");
                return;
            }
            InvalidateCache();
        }

        /// <summary>
        /// Creates a delta update for broadcasting.
        /// </summary>
        public RoomDeltaUpdate CreateDeltaUpdate(Room room, RoomChangeType changeType)
        {
            var roomInfo = ConvertToRoomInfo(room);
            return new RoomDeltaUpdate(changeType, roomInfo, _cacheVersion);
        }

        #endregion
    }
}
