using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    public struct ClientSceneMessage : NetworkMessage
    {
        public string SceneName;
        public SceneOperation SceneOperation;
        public bool CustomHandling;
    }
}