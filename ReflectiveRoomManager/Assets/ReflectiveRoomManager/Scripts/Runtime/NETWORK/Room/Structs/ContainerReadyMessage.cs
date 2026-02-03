namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct ContainerReadyMessage : Mirror.NetworkMessage
    {
        public readonly uint RoomID;
        public readonly bool Success;

        public ContainerReadyMessage(uint roomId, bool success = true)
        {
            RoomID = roomId;
            Success = success;
        }
    }
}
