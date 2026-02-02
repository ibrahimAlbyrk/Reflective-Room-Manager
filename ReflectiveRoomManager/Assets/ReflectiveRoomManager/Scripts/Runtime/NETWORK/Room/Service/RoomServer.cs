using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Room.Scenes;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomServer
    {
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
    }
}