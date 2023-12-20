using System.Collections.Generic;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomServer
    {
        #region Transaction Methods

        public static void CreateRoom(RoomInfo roomInfo)
        {
            RoomManagerBase.Instance?.CreateRoom(roomInfo);
        }

        public static void CreateRoom(NetworkConnectionToClient conn, RoomInfo roomInfo)
        {
            RoomManagerBase.Instance?.CreateRoom(roomInfo, conn);
        }

        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, params (string, string)[] customData)
        {
            CreateRoom(null, roomName, sceneName, maxPlayers, customData);
        }

        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, Dictionary<string, string> customData = default)
        {
            CreateRoom(null, roomName, sceneName, maxPlayers, customData);
        }

        public static void CreateRoom(NetworkConnectionToClient conn, string roomName, string sceneName, int maxPlayers, params (string, string)[] customData)
        {
            var data = new Dictionary<string, string>();

            foreach (var (key, value) in customData)
            {
                data.Add(key, value);
            }
            
            var roomInfo = new RoomInfo
            {
                RoomName = roomName,
                SceneName = sceneName,
                MaxPlayers = maxPlayers,
                CustomData = data
            };
            
            RoomManagerBase.Instance?.CreateRoom(roomInfo, conn);
        }

        public static void CreateRoom(NetworkConnectionToClient conn, string roomName, string sceneName, int maxPlayers, Dictionary<string, string> customData = default)
        {
            var roomInfo = new RoomInfo
            {
                RoomName = roomName,
                SceneName = sceneName,
                MaxPlayers = maxPlayers,
                CustomData = customData
            };
            
            RoomManagerBase.Instance?.CreateRoom(roomInfo, conn);
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