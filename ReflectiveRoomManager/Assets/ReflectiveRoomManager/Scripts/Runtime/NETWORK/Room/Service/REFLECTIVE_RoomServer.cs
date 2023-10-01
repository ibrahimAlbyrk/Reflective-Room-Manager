using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class REFLECTIVE_RoomServer
    {
        #region Transaction Methods

        public static void CreateRoom(REFLECTIVE_RoomInfo reflectiveRoomInfo)
        {
            REFLECTIVE_BaseRoomManager.Singleton.CreateRoom(null, reflectiveRoomInfo);
        }
        
        public static void CreateRoom(NetworkConnectionToClient conn, REFLECTIVE_RoomInfo reflectiveRoomInfo)
        {
            REFLECTIVE_BaseRoomManager.Singleton.CreateRoom(conn, reflectiveRoomInfo);
        }

        public static void JoinRoom(string roomName)
        {
            REFLECTIVE_BaseRoomManager.Singleton.JoinRoom(null, roomName);
        }

        public static void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            REFLECTIVE_BaseRoomManager.Singleton.JoinRoom(conn, roomName);
        }

        public static void ExitRoom(NetworkConnectionToClient conn, bool isDisconnected)
        {
            REFLECTIVE_BaseRoomManager.Singleton.ExitRoom(conn, isDisconnected);
        }

        public static void RemoveRoom(string roomName, bool forced = false)
        {
            REFLECTIVE_BaseRoomManager.Singleton.RemoveRoom(roomName, forced);
        }

        public static void RemoveAllRoom(bool forced = false)
        {
            REFLECTIVE_BaseRoomManager.Singleton.RemoveAllRoom(forced);
        }

        #endregion
    }
}