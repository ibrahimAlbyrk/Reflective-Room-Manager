using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    using Structs;

    /// <summary>
    /// Server-to-client room query response.
    /// </summary>
    [Serializable]
    public struct RoomQueryResponse
    {
        public RoomInfo[] Rooms;
        public int TotalRoomCount;
        public int TotalPages;
        public int CurrentPage;
        public uint CacheVersion;
        public bool IsDeltaUpdate;

        public RoomQueryResponse(
            RoomInfo[] rooms,
            int totalRoomCount,
            int totalPages,
            int currentPage,
            uint cacheVersion,
            bool isDeltaUpdate = false)
        {
            Rooms = rooms;
            TotalRoomCount = totalRoomCount;
            TotalPages = totalPages;
            CurrentPage = currentPage;
            CacheVersion = cacheVersion;
            IsDeltaUpdate = isDeltaUpdate;
        }

        /// <summary>
        /// Creates an empty response.
        /// </summary>
        public static RoomQueryResponse Empty => new(
            Array.Empty<RoomInfo>(),
            0,
            0,
            1,
            0,
            false
        );
    }
}
