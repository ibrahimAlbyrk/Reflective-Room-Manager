using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection.Messages
{
    public struct ReconnectionResultMessage : NetworkMessage
    {
        public bool Success;
        public uint RoomID;
    }
}
