﻿namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class REFLECTIVE_RoomClient
    {
        #region ID

        public static int ID;

        public static void ListenConnectionID(int id)
        {
            ID = id;
        }

        #endregion
        
        #region Transaction Methods

        public static void CreateRoom(REFLECTIVE_RoomInfo reflectiveRoomInfo)
        {
            REFLECTIVE_BaseRoomManager.RequestCreateRoom(reflectiveRoomInfo);
        }

        public static void JoinRoom(string roomName)
        {
            REFLECTIVE_BaseRoomManager.RequestJoinRoom(roomName);
        }

        public static void ExitRoom()
        {
            REFLECTIVE_BaseRoomManager.RequestExitRoom();
        }

        #endregion
    }
}