using Mirror;
using System.Linq;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Structs;
    using Service;
    using Utilities;
    
    public abstract partial class RoomManagerBase
    {
        private static void GetRoomIDForClient(uint roomID) => RoomClient.CurrentRoomID = roomID;

        private static void SendRoomIDToClient(NetworkConnection conn, uint roomID)
        {
            conn.Send(new ClientRoomIDMessage
            {
                RoomID = roomID
            });
        }

        private void SendUpdateRoomListForClient(NetworkConnection conn)
        {
            foreach (var message in m_rooms.Select(room =>
                         new RoomListChangeMessage(
                             RoomListUtility.ConvertToRoomList(room),
                             RoomMessageState.Add)))
            {
                conn.Send(message);
            }
        }
        
        private void SendClientExitSceneMessage(NetworkConnection conn)
        {
            conn.Send(new SceneMessage{sceneName = _lobbyScene, sceneOperation = SceneOperation.Normal});
        }

        private void AddRoomList(RoomInfo roomInfo)
        {
            _roomListInfos.Add(roomInfo);
        }

        private void UpdateRoomList(RoomInfo roomInfo)
        {
            var room = _roomListInfos.FirstOrDefault(info => info.ID == roomInfo.ID);

            var index = _roomListInfos.IndexOf(room);

            if (index < 0) return;

            _roomListInfos[index] = roomInfo;
        }

        private void RemoveRoomList(RoomInfo roomInfo)
        {
            _roomListInfos.RemoveAll(info => info.ID == roomInfo.ID);
        }
    }
}