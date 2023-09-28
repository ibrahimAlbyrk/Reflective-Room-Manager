namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Enums;
    
    [System.Serializable]
    public struct REFLECTIVE_ServerRoomMessage : Mirror.NetworkMessage
    {
        public readonly REFLECTIVE_RoomInfo RoomInfo;
        public readonly REFLECTIVE_ServerRoomState ServerRoomState;

        public readonly bool IsDisconnected;

        public REFLECTIVE_ServerRoomMessage(REFLECTIVE_ServerRoomState serverRoomState, REFLECTIVE_RoomInfo roomInfo, bool isDisconnected = false)
        {
            RoomInfo = roomInfo;
            ServerRoomState = serverRoomState;
            IsDisconnected = isDisconnected;
        }
    }
}