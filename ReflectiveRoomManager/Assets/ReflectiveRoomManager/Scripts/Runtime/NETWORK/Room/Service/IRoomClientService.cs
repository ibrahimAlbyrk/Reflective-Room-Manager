namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;

    public interface IRoomClientService
    {
        uint CurrentRoomID { get; }
        void CreateRoom(RoomInfo roomInfo);
        void JoinRoom(string roomName);
        void ExitRoom();
        void ExitRoom(bool isDisconnected);
        string GetRoomCustomData(string dataName);
    }
}
