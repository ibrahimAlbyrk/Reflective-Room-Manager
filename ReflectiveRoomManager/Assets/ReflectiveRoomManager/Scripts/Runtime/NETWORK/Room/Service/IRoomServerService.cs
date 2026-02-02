using Mirror;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Service
{
    using Structs;

    public interface IRoomServerService
    {
        void CreateRoom(RoomInfo roomInfo);
        void CreateRoom(NetworkConnectionToClient conn, RoomInfo roomInfo);
        void JoinRoom(string roomName);
        void JoinRoom(NetworkConnectionToClient conn, string roomName);
        void JoinRoom(NetworkConnectionToClient conn, string roomName, string accessToken);
        void ExitRoom(NetworkConnectionToClient conn, bool isDisconnected);
        void RemoveRoom(string roomName, bool forced = false);
        void RemoveAllRoom(bool forced = false);
        void ChangeScene(string roomName, string sceneName, bool keepClientObjects = false);
        void ChangeScene(Room room, string sceneName, bool keepClientObjects = false);
        void UpdateRoomData(string roomName, string key, string value);
        void UpdateRoomData(string roomName, Dictionary<string, string> data);
        void UpdateRoomData(Room room, string key, string value);
        void UpdateRoomData(Room room, Dictionary<string, string> data);
        void GracefulShutdown(float warningSeconds = 10f);
    }
}
