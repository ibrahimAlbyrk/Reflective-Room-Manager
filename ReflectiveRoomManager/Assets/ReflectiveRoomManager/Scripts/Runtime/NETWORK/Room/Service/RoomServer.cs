using Mirror;
using UnityEngine;
using System.Collections.Generic;
using REFLECTIVE.Runtime.NETWORK.Room.Scenes;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public class RoomServer : IRoomServerService
    {
        private static readonly RoomServer _instance = new();

        public static RoomServer Instance => _instance;

        #region Transaction Methods

        public static void CreateRoom(RoomInfo roomInfo)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(CreateRoom));
                return;
            }

            RoomManagerBase.Instance.CreateRoom(roomInfo);
        }

        public static void CreateRoom(NetworkConnectionToClient conn, RoomInfo roomInfo)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(CreateRoom));
                return;
            }

            RoomManagerBase.Instance.CreateRoom(roomInfo, conn);
        }

        public static void JoinRoom(string roomName)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(JoinRoom));
                return;
            }

            RoomManagerBase.Instance.JoinRoom(null, roomName);
        }

        public static void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(JoinRoom));
                return;
            }

            RoomManagerBase.Instance.JoinRoom(conn, roomName);
        }

        public static void ExitRoom(NetworkConnectionToClient conn, bool isDisconnected)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(ExitRoom));
                return;
            }

            RoomManagerBase.Instance.ExitRoom(conn, isDisconnected);
        }

        public static void RemoveRoom(string roomName, bool forced = false)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(RemoveRoom));
                return;
            }

            RoomManagerBase.Instance.RemoveRoom(roomName, forced);
        }

        public static void RemoveAllRoom(bool forced = false)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(RemoveAllRoom));
                return;
            }

            RoomManagerBase.Instance.RemoveAllRoom(forced);
        }

        #endregion

        #region Scene Methods

        public static void ChangeScene(string roomName, string sceneName, bool keepClientObjects = false)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(ChangeScene));
                return;
            }

            var room = RoomManagerBase.Instance.GetRoom(roomName);

            if (room == null)
            {
                Debug.LogWarning($"[RoomServer] Room '{roomName}' not found — cannot change scene");
                return;
            }

            RoomSceneChanger.ChangeScene(room, sceneName, keepClientObjects);
        }

        public static void ChangeScene(Room room, string sceneName, bool keepClientObjects = false)
        {
            if (room == null)
            {
                Debug.LogWarning("[RoomServer] Room is null — cannot execute " + nameof(ChangeScene));
                return;
            }

            RoomSceneChanger.ChangeScene(room, sceneName, keepClientObjects);
        }

        #endregion

        #region Data Methods

        public static void UpdateRoomData(string roomName, string key, string value)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(UpdateRoomData));
                return;
            }

            var room = RoomManagerBase.Instance.GetRoom(roomName);

            if (room == null)
            {
                Debug.LogWarning($"[RoomServer] Room '{roomName}' not found — cannot update data");
                return;
            }

            RoomManagerBase.Instance.UpdateRoomData(room, key, value);
        }

        public static void UpdateRoomData(string roomName, Dictionary<string, string> data)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(UpdateRoomData));
                return;
            }

            var room = RoomManagerBase.Instance.GetRoom(roomName);

            if (room == null)
            {
                Debug.LogWarning($"[RoomServer] Room '{roomName}' not found — cannot update data");
                return;
            }

            RoomManagerBase.Instance.UpdateRoomData(room, data);
        }

        public static void UpdateRoomData(Room room, string key, string value)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(UpdateRoomData));
                return;
            }

            RoomManagerBase.Instance.UpdateRoomData(room, key, value);
        }

        public static void UpdateRoomData(Room room, Dictionary<string, string> data)
        {
            if (RoomManagerBase.Instance == null)
            {
                Debug.LogWarning("[RoomServer] RoomManagerBase.Instance is null — cannot execute " + nameof(UpdateRoomData));
                return;
            }

            RoomManagerBase.Instance.UpdateRoomData(room, data);
        }

        #endregion

        #region IRoomServerService

        void IRoomServerService.CreateRoom(RoomInfo roomInfo) => CreateRoom(roomInfo);
        void IRoomServerService.CreateRoom(NetworkConnectionToClient conn, RoomInfo roomInfo) => CreateRoom(conn, roomInfo);
        void IRoomServerService.JoinRoom(string roomName) => JoinRoom(roomName);
        void IRoomServerService.JoinRoom(NetworkConnectionToClient conn, string roomName) => JoinRoom(conn, roomName);
        void IRoomServerService.ExitRoom(NetworkConnectionToClient conn, bool isDisconnected) => ExitRoom(conn, isDisconnected);
        void IRoomServerService.RemoveRoom(string roomName, bool forced) => RemoveRoom(roomName, forced);
        void IRoomServerService.RemoveAllRoom(bool forced) => RemoveAllRoom(forced);
        void IRoomServerService.ChangeScene(string roomName, string sceneName, bool keepClientObjects) => ChangeScene(roomName, sceneName, keepClientObjects);
        void IRoomServerService.ChangeScene(Room room, string sceneName, bool keepClientObjects) => ChangeScene(room, sceneName, keepClientObjects);
        void IRoomServerService.UpdateRoomData(string roomName, string key, string value) => UpdateRoomData(roomName, key, value);
        void IRoomServerService.UpdateRoomData(string roomName, Dictionary<string, string> data) => UpdateRoomData(roomName, data);
        void IRoomServerService.UpdateRoomData(Room room, string key, string value) => UpdateRoomData(room, key, value);
        void IRoomServerService.UpdateRoomData(Room room, Dictionary<string, string> data) => UpdateRoomData(room, data);

        #endregion
    }
}