using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    using Structs;

    /// <summary>
    /// Delta update for real-time room list changes.
    /// </summary>
    [Serializable]
    public struct RoomDeltaUpdate
    {
        public RoomChangeType ChangeType;
        public RoomInfo RoomData;
        public uint CacheVersion;

        public RoomDeltaUpdate(RoomChangeType changeType, RoomInfo roomData, uint cacheVersion)
        {
            ChangeType = changeType;
            RoomData = roomData;
            CacheVersion = cacheVersion;
        }
    }
}
