#if REFLECTIVE_CLIENT
namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;

    public interface IRoomClientService
    {
        uint CurrentRoomID { get; }
        void CreateRoom(RoomInfo roomInfo);
        void JoinRoom(string roomName);
        void JoinRoom(string roomName, string accessToken);
        void ExitRoom();
        void ExitRoom(bool isDisconnected);
        string GetRoomCustomData(string dataName);
    }
}
#endif
