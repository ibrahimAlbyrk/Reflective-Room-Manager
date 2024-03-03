using Mirror;
using System.Collections.Generic;
using REFLECTIVE.Runtime.NETWORK.Room.Scenes;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomServer
    {
        #region Transaction Methods

        public static void CreateRoom(RoomInfo roomInfo)
        {
            if (RoomManagerBase.Instance == null) return;
            
            RoomManagerBase.Instance.CreateRoom(roomInfo);
        }

        public static void CreateRoom(NetworkConnectionToClient conn, RoomInfo roomInfo)
        {
            if (RoomManagerBase.Instance == null) return;
            
            RoomManagerBase.Instance.CreateRoom(roomInfo, conn);
        }

        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, params (string, string)[] customData)
        {
            CreateRoom(null, roomName, sceneName, maxPlayers, customData);
        }

        public static void CreateRoom(string roomName, string sceneName, int maxPlayers, Dictionary<string, string> customData = null)
        {
            customData ??= new Dictionary<string, string>();

            CreateRoom(null, roomName, sceneName, maxPlayers, customData);
        }

        public static void CreateRoom(NetworkConnectionToClient conn, string roomName, string sceneName, int maxPlayers, params (string, string)[] customData)
        {
            if (RoomManagerBase.Instance == null) return;
            
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
            
            RoomManagerBase.Instance.CreateRoom(roomInfo, conn);
        }

        public static void CreateRoom(NetworkConnectionToClient conn, string roomName, string sceneName, int maxPlayers, Dictionary<string, string> customData = null)
        {
            if (RoomManagerBase.Instance == null) return;
            
            customData ??= new Dictionary<string, string>();
            
            var roomInfo = new RoomInfo
            {
                RoomName = roomName,
                SceneName = sceneName,
                MaxPlayers = maxPlayers,
                CustomData = customData
            };
            
            RoomManagerBase.Instance.CreateRoom(roomInfo, conn);
        }

        public static void JoinRoom(string roomName)
        {
            if (RoomManagerBase.Instance == null) return;
            
            RoomManagerBase.Instance.JoinRoom(null, roomName);
        }

        public static void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            if (RoomManagerBase.Instance == null) return;
            
            RoomManagerBase.Instance.JoinRoom(conn, roomName);
        }

        public static void ExitRoom(NetworkConnectionToClient conn, bool isDisconnected)
        {
            if (RoomManagerBase.Instance == null) return;
            
            RoomManagerBase.Instance.ExitRoom(conn, isDisconnected);
        }

        public static void RemoveRoom(string roomName, bool forced = false)
        {
            if (RoomManagerBase.Instance == null) return;
            
            RoomManagerBase.Instance.RemoveRoom(roomName, forced);
        }

        public static void RemoveAllRoom(bool forced = false)
        {
            if (RoomManagerBase.Instance == null) return;
            
            RoomManagerBase.Instance.RemoveAllRoom(forced);
        }

        #endregion

        #region Scene Methods

        public static void ChangeScene(string roomName, string sceneName, bool keepClientObjects = false)
        {
            if (RoomManagerBase.Instance == null) return;
            
            var room = RoomManagerBase.Instance.GetRoom(roomName);

            if (room == null) return;
            
            RoomSceneChanger.ChangeScene(room, sceneName, keepClientObjects);
        }
        
        public static void ChangeScene(Room room, string sceneName, bool keepClientObjects = false)
        {
            if (room == null) return;
            
            RoomSceneChanger.ChangeScene(room, sceneName, keepClientObjects);
        }

        #endregion
    }
}