namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;
    
    public static class RoomClient
    {
        #region Public Variables

        /// <summary>
        /// Connection ID of the client connecting to the room
        /// </summary>
        public static int ID;

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

        #endregion
    }
}