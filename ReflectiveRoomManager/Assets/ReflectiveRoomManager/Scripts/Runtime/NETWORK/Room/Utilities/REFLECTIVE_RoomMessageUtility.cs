using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Utilities
{
    using Enums;
    using Structs;
    
    public static class REFLECTIVE_RoomMessageUtility
    {
        public static void SenRoomUpdateMessage(REFLECTIVE_RoomInfo roomInfo, REFLECTIVE_RoomMessageState state)
        {
            var roomListChangeMessage = new REFLECTIVE_RoomListChangeMessage(roomInfo, state);
            
            NetworkServer.SendToAll(roomListChangeMessage);
        }
        
        public static void SendRoomMessage(NetworkConnection conn, REFLECTIVE_ClientRoomState state)
        {
            var roomMessage = new REFLECTIVE_ClientRoomMessage(state, conn.connectionId);

            conn.Send(roomMessage);
        }
    }
}