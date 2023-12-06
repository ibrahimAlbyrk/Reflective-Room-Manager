using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomServer
    {
        #region Transaction Methods

        public static void CreateRoom(RoomInfo roomInfo)
        {
            RoomManagerBase.Instance?.CreateRoom(null, roomInfo);
        }
        
        public static void CreateRoom(NetworkConnectionToClient conn, RoomInfo roomInfo)
        {
            RoomManagerBase.Instance?.CreateRoom(conn, roomInfo);
        }

        public static void JoinRoom(string roomName)
        {
            RoomManagerBase.Instance?.JoinRoom(null, roomName);
        }

        public static void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            RoomManagerBase.Instance?.JoinRoom(conn, roomName);
        }

        public static void ExitRoom(NetworkConnectionToClient conn, bool isDisconnected)
        {
            RoomManagerBase.Instance?.ExitRoom(conn, isDisconnected);
        }

        public static void RemoveRoom(string roomName, bool forced = false)
        {
            RoomManagerBase.Instance?.RemoveRoom(roomName, forced);
        }

        public static void RemoveAllRoom(bool forced = false)
        {
            RoomManagerBase.Instance?.RemoveAllRoom(forced);
        }

        #endregion

        #region Scene Methods

        public static void ChangeScene(string roomName, string sceneName, bool keepClientObjects = false)
        {
            var room = RoomManagerBase.Instance?.GetRoom(roomName);

            room?.ChangeScene(sceneName, keepClientObjects);
        }
        
        public static void ChangeScene(Room room, string sceneName, bool keepClientObjects = false)
        {
            room?.ChangeScene(sceneName, keepClientObjects);
        }

        #endregion
    }
}