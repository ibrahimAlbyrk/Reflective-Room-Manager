using System.Collections.Generic;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    public struct SceneLoadMessage : NetworkMessage
    {
        public List<NetworkIdentity> Identities;
    }
}
