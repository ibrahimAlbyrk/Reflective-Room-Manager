using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Structs;
    using SceneManagement;
    
    [AddComponentMenu("REFLECTIVE/Network Room Manager")]
    public class REFLECTIVE_SimpleRoomManager : REFLECTIVE_BaseRoomManager
    {
        [ServerCallback]
        public override void CreateRoom(NetworkConnection conn = null, REFLECTIVE_RoomInfo roomInfo = default)
        {
            var roomName = roomInfo.Name;
            var maxPlayers = roomInfo.MaxPlayers;

            if (m_rooms.Any(room => room.RoomName == roomName)) return;

            var onServer = conn is null;

            var room = new REFLECTIVE_Room(roomName, maxPlayers, onServer);
            
            AddToList(room);

            //If it is a client, add in to the room
            if (!onServer)
            {
                SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Created);

                REFLECTIVE_SceneManager.LoadScene(roomInfo.SceneName, LoadSceneMode.Additive,
                    scene =>
                    {
                        room.Scene = scene;
                        JoinRoom(conn.identity.connectionToClient, roomName);
                    });
            }
            else
                REFLECTIVE_SceneManager.LoadScene(roomInfo.SceneName, LoadSceneMode.Additive,
                    scene => room.Scene = scene);
            
            Invoke_OnServerCreatedRoom(roomInfo);
        }

        [ServerCallback]
        public override void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            var room = m_rooms.FirstOrDefault(r => r.RoomName == roomName);

            if (string.IsNullOrEmpty(room.RoomName)) // Handle room not found.
            {
                SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Fail);
                return;
            }

            if (room.MaxPlayers <= room.CurrentPlayers) // Handle room is full.
            {
                SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Fail);
                return;
            }

            room.AddConnection(conn);
            
            UpdateRoomInfo(room);

            Invoke_OnServerJoinedClient(conn);

            SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Joined);
        }

        [ServerCallback]
        public override void RemoveAllRoom()
        {
            foreach (var room in m_rooms.ToList())
            {
                RemoveRoom(room.RoomName);
            }
        }

        [ServerCallback]
        public override void RemoveRoom(string roomName)
        {
            var room = m_rooms.FirstOrDefault(room => room.RoomName == roomName);

            if (string.IsNullOrEmpty(room.RoomName)) return;

            var removedConnections = room.RemoveAllConnections();

            if (room.IsServer) return;

            RemoveToList(room);

            var roomScene = room.Scene;

            REFLECTIVE_SceneManager.UnLoadScene(roomScene);

            removedConnections.ForEach(connection => SendRoomMessage(connection, REFLECTIVE_ClientRoomState.Removed));
        }

        [ServerCallback]
        public override void ExitRoom(NetworkConnection conn, bool isDisconnected)
        {
            var exitedRoom = m_rooms.FirstOrDefault(room => room.RemoveConnection(conn));

            if (string.IsNullOrEmpty(exitedRoom.RoomName))
            {
                // Handle exit failed (user not in any room).
                return;
            }
            
            if (exitedRoom.CurrentPlayers < 1)
                RemoveRoom(exitedRoom.RoomName);
            else
                UpdateRoomInfo(exitedRoom);

            if(!isDisconnected)
                Invoke_OnServerExitedClient(conn.identity?.connectionToClient);
            else
                Invoke_OnServerDisconnectedClient(conn);
            
            SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Exited);
        }
    }
}