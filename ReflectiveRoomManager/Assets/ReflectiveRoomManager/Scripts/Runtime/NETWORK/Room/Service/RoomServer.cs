using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomServer
    {
        #region Transaction Methods

        public static void CreateRoom(RoomInfo roomInfo)
        {
            RoomManagerBase.Singleton.CreateRoom(null, roomInfo);
        }
        
        public static void CreateRoom(NetworkConnectionToClient conn, RoomInfo roomInfo)
        {
            RoomManagerBase.Singleton.CreateRoom(conn, roomInfo);
        }

        public static void JoinRoom(string roomName)
        {
            RoomManagerBase.Singleton.JoinRoom(null, roomName);
        }

        public static void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            RoomManagerBase.Singleton.JoinRoom(conn, roomName);
        }

        public static void ExitRoom(NetworkConnectionToClient conn, bool isDisconnected)
        {
            RoomManagerBase.Singleton.ExitRoom(conn, isDisconnected);
        }

        public static void RemoveRoom(string roomName, bool forced = false)
        {
            RoomManagerBase.Singleton.RemoveRoom(roomName, forced);
        }

        public static void RemoveAllRoom(bool forced = false)
        {
            RoomManagerBase.Singleton.RemoveAllRoom(forced);
        }

        #endregion

        #region Scene Methods

        public static void ChangeScene(Room room, string sceneName)
        {
            room.ChangeScene(sceneName);
        }

        #endregion
    }
}