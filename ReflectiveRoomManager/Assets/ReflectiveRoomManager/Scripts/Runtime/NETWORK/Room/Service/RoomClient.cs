using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;

    public static class RoomClient
    {
        #region Public Variables

        /// <summary>
        /// ID of the room where the client is joined.
        /// <remarks>Returns -1 if the client is not in the room</remarks>
        /// </summary>
        public static uint CurrentRoomID;

        #endregion

        #region Transaction Methods

        public static void CreateRoom(RoomInfo roomInfo)
        {
            RoomManagerBase.RequestCreateRoom(roomInfo);
        }

        public static void JoinRoom(string roomName)
        {
            RoomManagerBase.RequestJoinRoom(roomName);
        }

        public static void ExitRoom()
        {
            RoomManagerBase.RequestExitRoom();
        }
        
        public static void ExitRoom(bool isDisconnected)
        {
            RoomManagerBase.RequestExitRoom(isDisconnected);
        }

        #endregion

        #region Data Methods
        
        public static string GetRoomCustomData(string dataName)
        {
            var room = RoomManagerBase.Instance.GetCurrentRoomInfo();

            if (room.CustomData == null || !room.CustomData.TryGetValue(dataName, out var dataValue))
            {
                Debug.LogWarning("Room or custom data not found");
                return default;
            }

            return dataValue;
        }

        #endregion
    }
}