namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomClient
    {
        #region Public Variables

        public static int ID;

        #endregion
        
        #region Transaction Methods

        public static void CreateRoom(RoomInfo roomInfo)
        {
            BaseRoomManager.RequestCreateRoom(roomInfo);
        }

        public static void JoinRoom(string roomName)
        {
            BaseRoomManager.RequestJoinRoom(roomName);
        }

        public static void ExitRoom()
        {
            BaseRoomManager.RequestExitRoom();
        }

        #endregion
    }
}