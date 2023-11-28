using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Structs;
    using Container;
    using Utilities;
    
    [AddComponentMenu("REFLECTIVE/Network Room Manager")]
    public class RoomManager : RoomManagerBase
    {
        internal override void CreateRoom(NetworkConnection conn = null, RoomInfo roomInfo = default)
        {
            var roomName = roomInfo.Name;
            var maxPlayers = Mathf.Max(roomInfo.MaxPlayers, MaxPlayerCountPerRoom);

            if (m_rooms.Count >= MaxRoomCount)
            {
                Debug.LogWarning("No more rooms can be created as there is a maximum number of rooms");
                
                return;
            }

            if (m_rooms.Any(room => room.RoomName == roomName))
            {
                Debug.LogWarning("There is already a room with this name");
                
                return;
            }

            var isServer = conn is null;

            var room = new Room(roomName, maxPlayers, isServer);
            
            RoomListUtility.AddRoomToList(ref m_rooms, room);
            
            //If it is a client, add in to the room
            if (!isServer)
            {
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Created);
                
                LoadRoom(room, roomInfo, () => JoinRoom(conn, room));
            }
            else
                LoadRoom(room, roomInfo);
            
            m_eventManager.Invoke_OnServerCreatedRoom(roomInfo);
        }

        internal override void JoinRoom(NetworkConnection conn, Room room)
        {
            if (room == null)
            {
                Debug.LogWarning($"There is no such room for join");
                
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                
                return;
            }
            
            if (room.MaxPlayers <= room.CurrentPlayers) // Handle room is full.
            {
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            room.AddConnection(conn);
            
            RoomMessageUtility.SenRoomUpdateMessage(
                RoomListUtility.ConvertToRoomList(room), RoomMessageState.Update);
            
            RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Joined);

            m_eventManager.Invoke_OnServerJoinedClient(conn);
        }
        
        internal override void JoinRoom(NetworkConnection conn, string roomName)
        {
            var room = m_rooms.FirstOrDefault(r => r.RoomName == roomName);

           JoinRoom(conn, room);
        }

        internal override void RemoveAllRoom(bool forced = false)
        {
            foreach (var room in m_rooms.ToList())
            {
                RemoveRoom(room.RoomName, forced);
            }
        }

        internal override void RemoveRoom(string roomName, bool forced = false)
        {
            var room = m_rooms.FirstOrDefault(room => room.RoomName == roomName);
            
            if (room == null)
            {
                Debug.LogWarning($"There is no such room for remove");
                
                return;
            }

            var removedConnections = room.RemoveAllConnections();

            if (room.IsServer && !forced) return;

            UnLoadRoom(room);

            removedConnections.ForEach(connection =>
            {
                RoomMessageUtility.SendRoomMessage(connection, ClientRoomState.Removed);
            });
            
            RoomListUtility.RemoveRoomToList(ref m_rooms, room);
            
            RoomContainer.Listener.RemoveRoomListenerHandlers(roomName);
        }

        internal override void ExitRoom(NetworkConnection conn, bool isDisconnected)
        {
            var exitedRoom = m_rooms.FirstOrDefault(room => room.RemoveConnection(conn));
            
            if (exitedRoom == null)
            {
                // Handle exit failed (user not in any room).
                
                Debug.LogWarning($"There is no such room for exit");
                
                return;
            }
            
            if (exitedRoom.CurrentPlayers < 1 && !exitedRoom.IsServer)
                RemoveRoom(exitedRoom.RoomName);
            else
                RoomListUtility.UpdateRoomToList(ref m_rooms,exitedRoom);
            
            RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Exited);

            if(!isDisconnected)
                m_eventManager.Invoke_OnServerExitedClient(conn);
            else
                m_eventManager.Invoke_OnServerDisconnectedClient(conn);
        }
    }
}