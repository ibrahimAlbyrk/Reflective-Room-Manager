using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery.Messages
{
    /// <summary>
    /// Client requests room query.
    /// </summary>
    public struct RoomQueryRequestMessage : NetworkMessage
    {
        public RoomQueryRequest Request;

        public RoomQueryRequestMessage(RoomQueryRequest request)
        {
            Request = request;
        }
    }
}
