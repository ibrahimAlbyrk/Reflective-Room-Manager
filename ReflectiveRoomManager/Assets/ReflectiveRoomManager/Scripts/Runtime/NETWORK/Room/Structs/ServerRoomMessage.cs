namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Enums;

    [System.Serializable]
    public struct ServerRoomMessage : Mirror.NetworkMessage
    {
        public readonly RoomInfo RoomInfo;
        public readonly ServerRoomState ServerRoomState;

        public readonly bool IsDisconnected;

        public readonly string AccessToken;

        public ServerRoomMessage(ServerRoomState serverRoomState, RoomInfo roomInfo, bool isDisconnected = false)
        {
            RoomInfo = roomInfo;
            ServerRoomState = serverRoomState;
            IsDisconnected = isDisconnected;
            AccessToken = null;
        }

        public ServerRoomMessage(ServerRoomState serverRoomState, RoomInfo roomInfo, string accessToken)
        {
            RoomInfo = roomInfo;
            ServerRoomState = serverRoomState;
            IsDisconnected = false;
            AccessToken = accessToken;
        }
    }
}
