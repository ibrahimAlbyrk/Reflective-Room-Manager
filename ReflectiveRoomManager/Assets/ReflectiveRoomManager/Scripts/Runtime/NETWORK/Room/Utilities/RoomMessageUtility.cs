using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Utilities
{
    using Enums;
    using Structs;
    
    public static class RoomMessageUtility
    {
        public static void SenRoomUpdateMessage(RoomInfo roomInfo, RoomMessageState state)
        {
            var roomListChangeMessage = new RoomListChangeMessage(roomInfo, state);
            
            NetworkServer.SendToAll(roomListChangeMessage);
        }
        
        public static void SendRoomMessage(NetworkConnection conn, ClientRoomState state)
        {
            var roomMessage = new ClientRoomMessage(state);

            conn.Send(roomMessage);
        }
    }
}