using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery
{
    /// <summary>
    /// Cached query result with timestamp.
    /// </summary>
    [Serializable]
    public struct CachedQueryResult
    {
        public RoomQueryResponse Response;
        public float Timestamp;
        public float TTL;

        public CachedQueryResult(RoomQueryResponse response, float timestamp, float ttl)
        {
            Response = response;
            Timestamp = timestamp;
            TTL = ttl;
        }

        public readonly bool IsExpired(float currentTime)
        {
            return currentTime - Timestamp > TTL;
        }
    }
}
