using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection.Messages
{
    public struct PlayerIdentityMessage : NetworkMessage
    {
        public string PlayerId;
    }
}
