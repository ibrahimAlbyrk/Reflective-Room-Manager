using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Discovery.Messages
{
    /// <summary>
    /// Server broadcasts delta update to all clients.
    /// </summary>
    public struct RoomDeltaUpdateMessage : NetworkMessage
    {
        public RoomDeltaUpdate Update;

        public RoomDeltaUpdateMessage(RoomDeltaUpdate update)
        {
            Update = update;
        }
    }
}
