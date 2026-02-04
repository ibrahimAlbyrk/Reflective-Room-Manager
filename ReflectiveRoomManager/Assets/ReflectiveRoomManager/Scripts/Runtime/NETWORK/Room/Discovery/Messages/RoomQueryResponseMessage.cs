using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery.Messages
{
    /// <summary>
    /// Server responds with query results.
    /// </summary>
    public struct RoomQueryResponseMessage : NetworkMessage
    {
        public RoomQueryResponse Response;

        public RoomQueryResponseMessage(RoomQueryResponse response)
        {
            Response = response;
        }
    }
}
