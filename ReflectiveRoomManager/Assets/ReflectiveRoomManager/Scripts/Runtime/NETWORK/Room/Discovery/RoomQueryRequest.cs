using System;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    /// <summary>
    /// Client-to-server room query request.
    /// </summary>
    [Serializable]
    public struct RoomQueryRequest
    {
        public RoomFilterData Filter;
        public RoomSortOptions SortBy;
        public int PageNumber;
        public int PageSize;
        public uint LastKnownVersion;

        private const int MinPageSize = 1;
        private const int MaxPageSize = 100;
        private const int DefaultPageSize = 20;

        public RoomQueryRequest(
            RoomFilterData filter,
            RoomSortOptions sortBy = RoomSortOptions.None,
            int pageNumber = 1,
            int pageSize = DefaultPageSize)
        {
            Filter = filter;
            SortBy = sortBy;
            PageNumber = Mathf.Max(1, pageNumber);
            PageSize = Mathf.Clamp(pageSize, MinPageSize, MaxPageSize);
            LastKnownVersion = 0;
        }

        /// <summary>
        /// Creates a default query request with no filters.
        /// </summary>
        public static RoomQueryRequest Default => new(default, RoomSortOptions.None, 1, DefaultPageSize);
    }
}
