using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Structs;
    using Utilities;
    using SceneManagement;
    
    [AddComponentMenu("REFLECTIVE/Network Room Manager")]
    public class REFLECTIVE_RoomManager : REFLECTIVE_BaseRoomManager
    {
        public override void CreateRoom(NetworkConnection conn = null, REFLECTIVE_RoomInfo roomInfo = default)
        {
            var roomName = roomInfo.Name;
            var maxPlayers = roomInfo.MaxPlayers;

            if (m_rooms.Any(room => room.RoomName == roomName)) return;

            var onServer = conn is null;

            var room = new REFLECTIVE_Room(roomName, maxPlayers, onServer);
            
            REFLECTIVE_RoomListUtility.AddRoomToList(ref m_rooms, room);

            //If it is a client, add in to the room
            if (!onServer)
            {
                REFLECTIVE_RoomMessageUtility.SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Created);

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

        public override void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            var room = m_rooms.FirstOrDefault(r => r.RoomName == roomName);

            if (room == null)
            {
                Debug.LogWarning($"There is no such room");
                
                REFLECTIVE_RoomMessageUtility.SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Fail);
                
                return;
            }
            
            if (room.MaxPlayers <= room.CurrentPlayers) // Handle room is full.
            {
                REFLECTIVE_RoomMessageUtility.SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Fail);
                return;
            }

            room.AddConnection(conn);
            
            REFLECTIVE_RoomMessageUtility.SenRoomUpdateMessage(
                REFLECTIVE_RoomListUtility.ConvertToRoomList(room), REFLECTIVE_RoomMessageState.Update);

            Invoke_OnServerJoinedClient(conn);

            REFLECTIVE_RoomMessageUtility.SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Joined);
        }

        public override void RemoveAllRoom()
        {
            foreach (var room in m_rooms.ToList())
            {
                RemoveRoom(room.RoomName);
            }
        }

        public override void RemoveRoom(string roomName)
        {
            var room = m_rooms.FirstOrDefault(room => room.RoomName == roomName);
            
            if (room == null)
            {
                Debug.LogWarning($"There is no such room");
                
                return;
            }

            var removedConnections = room.RemoveAllConnections();

            if (room.IsServer) return;

            REFLECTIVE_RoomListUtility.RemoveRoomToList(ref m_rooms, room);

            var roomScene = room.Scene;

            REFLECTIVE_SceneManager.UnLoadScene(roomScene);

            removedConnections.ForEach(connection => REFLECTIVE_RoomMessageUtility.SendRoomMessage(connection, REFLECTIVE_ClientRoomState.Removed));
        }

        public override void ExitRoom(NetworkConnection conn, bool isDisconnected)
        {
            var exitedRoom = m_rooms.FirstOrDefault(room => room.RemoveConnection(conn));
            
            if (exitedRoom == null)
            {
                // Handle exit failed (user not in any room).
                
                Debug.LogWarning($"There is no such room");
                
                return;
            }
            
            if (exitedRoom.CurrentPlayers < 1)
                RemoveRoom(exitedRoom.RoomName);
            else
                REFLECTIVE_RoomListUtility.UpdateRoomToList(ref m_rooms, exitedRoom);

            if(!isDisconnected)
                Invoke_OnServerExitedClient(conn.identity?.connectionToClient);
            else
                Invoke_OnServerDisconnectedClient(conn);
            
            REFLECTIVE_RoomMessageUtility.SendRoomMessage(conn, REFLECTIVE_ClientRoomState.Exited);
        }
    }
}