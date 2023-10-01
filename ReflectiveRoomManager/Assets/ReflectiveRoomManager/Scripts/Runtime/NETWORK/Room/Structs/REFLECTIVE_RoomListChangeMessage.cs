using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Enums;
    
    [System.Serializable]
    public struct REFLECTIVE_RoomListChangeMessage : NetworkMessage
    {
        public readonly REFLECTIVE_RoomInfo RoomInfo;
        public readonly REFLECTIVE_RoomMessageState State;

        public REFLECTIVE_RoomListChangeMessage(REFLECTIVE_RoomInfo roomInfo, REFLECTIVE_RoomMessageState state)
        {
            RoomInfo = roomInfo;
            State = state;
        }
    }
}