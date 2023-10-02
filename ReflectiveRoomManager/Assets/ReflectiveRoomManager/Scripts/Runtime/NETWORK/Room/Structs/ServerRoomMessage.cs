namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Enums;
    
    [System.Serializable]
    public struct ServerRoomMessage : Mirror.NetworkMessage
    {
        public readonly RoomInfo RoomInfo;
        public readonly ServerRoomState ServerRoomState;

        public readonly bool IsDisconnected;

        public ServerRoomMessage(ServerRoomState serverRoomState, RoomInfo roomInfo, bool isDisconnected = false)
        {
            RoomInfo = roomInfo;
            ServerRoomState = serverRoomState;
            IsDisconnected = isDisconnected;
        }
    }
}