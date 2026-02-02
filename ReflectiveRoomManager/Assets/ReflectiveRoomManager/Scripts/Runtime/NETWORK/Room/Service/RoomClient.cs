using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;

    public class RoomClient : IRoomClientService
    {
        private static readonly RoomClient _instance = new();

        public static RoomClient Instance => _instance;

        #region Public Variables

        public static uint CurrentRoomID { get; internal set; }

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

        #region IRoomClientService

        uint IRoomClientService.CurrentRoomID => CurrentRoomID;
        void IRoomClientService.CreateRoom(RoomInfo roomInfo) => CreateRoom(roomInfo);
        void IRoomClientService.JoinRoom(string roomName) => JoinRoom(roomName);
        void IRoomClientService.ExitRoom() => ExitRoom();
        void IRoomClientService.ExitRoom(bool isDisconnected) => ExitRoom(isDisconnected);
        string IRoomClientService.GetRoomCustomData(string dataName) => GetRoomCustomData(dataName);

        #endregion
    }
}