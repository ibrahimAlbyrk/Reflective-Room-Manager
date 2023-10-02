using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomServer
    {
        #region Transaction Methods

        public static void CreateRoom(RoomInfo roomInfo)
        {
            BaseRoomManager.Singleton.CreateRoom(null, roomInfo);
        }
        
        public static void CreateRoom(NetworkConnectionToClient conn, RoomInfo roomInfo)
        {
            BaseRoomManager.Singleton.CreateRoom(conn, roomInfo);
        }

        public static void JoinRoom(string roomName)
        {
            BaseRoomManager.Singleton.JoinRoom(null, roomName);
        }

        public static void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            BaseRoomManager.Singleton.JoinRoom(conn, roomName);
        }

        public static void ExitRoom(NetworkConnectionToClient conn, bool isDisconnected)
        {
            BaseRoomManager.Singleton.ExitRoom(conn, isDisconnected);
        }

        public static void RemoveRoom(string roomName, bool forced = false)
        {
            BaseRoomManager.Singleton.RemoveRoom(roomName, forced);
        }

        public static void RemoveAllRoom(bool forced = false)
        {
            BaseRoomManager.Singleton.RemoveAllRoom(forced);
        }

        #endregion
    }
}