using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct ContainerSceneMessage : Mirror.NetworkMessage
    {
        public readonly bool UseRuntimeContainer;
        public readonly string CustomContainerScene;
        public readonly LocalPhysicsMode PhysicsMode;
        public readonly uint RoomID;

        public ContainerSceneMessage(
            bool useRuntime,
            string customScene,
            LocalPhysicsMode physics,
            uint roomId)
        {
            UseRuntimeContainer = useRuntime;
            CustomContainerScene = customScene;
            PhysicsMode = physics;
            RoomID = roomId;
        }
    }
}
