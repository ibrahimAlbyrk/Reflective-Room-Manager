using System.Collections.Generic;
using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Roles;
    using Roles.Handlers;
    using State;
    using State.Handlers;
    using Structs;
    using Container;
    using Utilities;
    
    [AddComponentMenu("REFLECTIVE/Network Room Manager")]
    public class RoomManager : RoomManagerBase
    {
        internal override void CreateRoom(RoomInfo roomInfo, NetworkConnection conn = default)
        {
            if (_isShuttingDown)
            {
                if (conn != null)
                    RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            var roomName = roomInfo.RoomName;
            var maxPlayers = Mathf.Min(roomInfo.MaxPlayers, MaxPlayerCountPerRoom);

            if (m_rooms.Count >= MaxRoomCount)
            {
                Debug.LogWarning("No more rooms can be created as there is a maximum number of rooms");

                return;
            }

            if (m_rooms.Any(room => room.Name == roomName))
            {
                Debug.LogWarning("There is already a room with this name");
                
                return;
            }

            if (conn != null && !RoomValidator.CanCreateRoom(conn, roomInfo, out var createReason))
            {
                Debug.LogWarning($"Room creation denied: {createReason}");
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            var isServer = conn is null;

            var room = new Room(roomName, maxPlayers, isServer)
            {
                ID = m_uniqueIdentifier.CreateID()
            };

            room.SetCustomData(roomInfo.CustomData);

            // Initialize state machine if enabled
            if (_enableStateMachine && _stateConfig != null)
            {
                var configOverride = RoomStateConfigOverride.FromCustomData(roomInfo.CustomData ?? new Dictionary<string, string>());
                room.InitializeStateMachine(_stateConfig, m_eventManager, configOverride);
            }

            // Initialize role manager if enabled
            if (_enableRoleSystem && _roleConfig != null)
            {
                room.InitializeRoleManager(_roleConfig, m_eventManager);
            }

            RoomListUtility.AddRoomToList(m_rooms, room);
            
            //If it is a client, add in to the room
            if (!isServer)
            {
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Created);
                
                LoadRoom(room, roomInfo, () => JoinRoom(conn, room));
            }
            else
            {
                LoadRoom(room, roomInfo);
            }

            m_eventManager.Invoke_OnServerCreatedRoom(roomInfo);
        }

        internal override void JoinRoom(NetworkConnection conn, Room room)
        {
            JoinRoomInternal(conn, room, string.Empty);
        }

        internal void JoinRoom(NetworkConnection conn, Room room, string accessToken)
        {
            JoinRoomInternal(conn, room, accessToken);
        }

        private void JoinRoomInternal(NetworkConnection conn, Room room, string accessToken)
        {
            if (_isShuttingDown)
            {
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            if (room == null)
            {
                Debug.LogWarning($"There is no such room for join");

                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);

                return;
            }

            if (room.MaxPlayers <= room.CurrentPlayers + room.ReservedSlots) // Handle room is full.
            {
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            if (!RoomAccessValidator.ValidateAccess(conn, room, accessToken, out var accessReason))
            {
                Debug.LogWarning($"Room access denied: {accessReason}");
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            if (!RoomValidator.CanJoinRoom(conn, room, out var joinReason))
            {
                Debug.LogWarning($"Room join denied: {joinReason}");
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            // Check state machine validation
            if (_enableStateMachine && !room.CanPlayerJoinState(conn, out var stateReason))
            {
                Debug.LogWarning($"Room join denied by state: {stateReason}");
                RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            // Determine if this is the room creator (first connection)
            var isRoomCreator = room.CurrentPlayers == 0;

            room.AddConnection(conn);

            // Notify role manager
            if (_enableRoleSystem && room.RoleManager != null)
            {
                if (isRoomCreator)
                {
                    room.AssignInitialOwner(conn);
                }
                else
                {
                    room.NotifyPlayerJoinedForRoles(conn);
                }
                RoomRoleNetworkHandlers.SendRoleListToClient(conn, room);
            }

            // Notify state machine
            if (_enableStateMachine)
            {
                room.NotifyStateMachinePlayerJoined(conn);
                RoomStateNetworkHandlers.SendStateToConnection(room, conn);
            }

            RoomMessageUtility.SendRoomUpdateMessage(
                RoomListUtility.ConvertToRoomList(room), RoomMessageState.Update);

            RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Joined);

            if (conn is NetworkConnectionToClient connToClient)
                m_eventManager.Invoke_OnServerJoinedClient(connToClient, room.ID);
        }

        internal override void JoinRoom(NetworkConnection conn, string roomName)
        {
            var room = m_rooms.FirstOrDefault(r => r.Name == roomName && !r.IsPrivate);

           JoinRoom(conn, room);
        }

        internal override void JoinRoom(NetworkConnection conn, string roomName, string accessToken)
        {
            var room = m_rooms.FirstOrDefault(r => r.Name == roomName && !r.IsPrivate);

            JoinRoom(conn, room, accessToken);
        }

        internal override void JoinRoom(NetworkConnection conn, uint roomID)
        {
            var room = m_rooms.FirstOrDefault(r => r.ID == roomID);
            
            JoinRoom(conn, room);
        }

        internal override void RemoveAllRoom(bool forced = false)
        {
            foreach (var room in m_rooms.ToList())
            {
                RemoveRoom(room, forced);
            }
        }

        internal override void RemoveRoom(Room room, bool forced = false)
        {
            if (room == null)
            {
                Debug.LogWarning($"There is no such room for remove");

                return;
            }

            var removedConnections = room.RemoveAllConnections();

            if (room.IsServer && !forced) return;

            m_eventManager.Invoke_OnServerRoomRemoving(room.ID);

            // Cleanup role manager
            if (_enableRoleSystem && room.RoleManager != null)
            {
                room.CleanupRoleManager();
            }

            UnLoadRoom(room);

            removedConnections.ForEach(connection =>
            {
                RoomMessageUtility.SendRoomMessage(connection, ClientRoomState.Removed);
            });
            
            RoomListUtility.RemoveRoomToList(m_rooms, room);
            
            RoomContainer.Listener.RemoveRoomListenerHandlers(room.Name);
        }

        internal override void RemoveRoom(string roomName, bool forced = false)
        {
            var room = m_rooms.FirstOrDefault(room => room.Name == roomName);
            
            RemoveRoom(room, forced);
        }

        internal override void ExitRoom(NetworkConnection conn, bool isDisconnected)
        {
            // Find room containing this connection first (before removing)
            var exitedRoom = m_rooms.FirstOrDefault(room => room.Connections.Contains(conn));

            if (exitedRoom == null)
            {
                Debug.LogWarning($"There is no such room for exit");
                return;
            }

            // Notify state machine before removal
            if (_enableStateMachine)
            {
                exitedRoom.NotifyStateMachinePlayerLeft(conn);
            }

            // Notify role manager before removal
            if (_enableRoleSystem && exitedRoom.RoleManager != null)
            {
                exitedRoom.NotifyPlayerLeftForRoles(conn);
            }

            // Now remove the connection
            exitedRoom.RemoveConnection(conn);

            if (exitedRoom.CurrentPlayers < 1 && exitedRoom.ReservedSlots < 1 && !exitedRoom.IsServer)
                RemoveRoom(exitedRoom);
            else
                RoomListUtility.UpdateRoomToList(m_rooms, exitedRoom);
            
            RoomMessageUtility.SendRoomMessage(conn, ClientRoomState.Exited);
            
            if (conn is NetworkConnectionToClient connToClient)
            {
                if(!isDisconnected)
                    m_eventManager.Invoke_OnServerExitedClient(connToClient);
                else
                    m_eventManager.Invoke_OnServerDisconnectedClient(connToClient);
            }
        }

        internal override void UpdateRoomData(Room room, string key, string value)
        {
            if (room == null)
            {
                Debug.LogWarning("There is no such room for data update");
                return;
            }

            room.UpdateCustomData(key, value);

            RoomListUtility.UpdateRoomToList(m_rooms, room);

            m_eventManager.Invoke_OnServerRoomDataUpdated(room);
        }

        internal override void UpdateRoomData(Room room, Dictionary<string, string> data)
        {
            if (room == null)
            {
                Debug.LogWarning("There is no such room for data update");
                return;
            }

            room.UpdateCustomData(data);

            RoomListUtility.UpdateRoomToList(m_rooms, room);

            m_eventManager.Invoke_OnServerRoomDataUpdated(room);
        }
    }
}