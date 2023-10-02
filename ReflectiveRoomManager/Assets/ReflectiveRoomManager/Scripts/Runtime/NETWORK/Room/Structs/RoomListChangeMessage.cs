using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Enums;
    
    [System.Serializable]
    public struct RoomListChangeMessage : NetworkMessage
    {
        public readonly RoomInfo RoomInfo;
        public readonly RoomMessageState State;

        public RoomListChangeMessage(RoomInfo roomInfo, RoomMessageState state)
        {
            RoomInfo = roomInfo;
            State = state;
        }
    }
}