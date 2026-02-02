using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection.Messages
{
    public struct PlayerIdentityResponseMessage : NetworkMessage
    {
        public string PlayerId;
    }
}
