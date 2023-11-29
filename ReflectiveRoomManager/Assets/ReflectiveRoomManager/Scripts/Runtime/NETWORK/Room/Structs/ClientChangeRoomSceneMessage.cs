using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    public struct ClientChangeRoomSceneMessage : NetworkMessage
    {
        public string SceneName;
    }
}