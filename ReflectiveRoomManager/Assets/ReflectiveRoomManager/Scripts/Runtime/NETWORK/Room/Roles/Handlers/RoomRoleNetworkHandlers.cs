using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles.Handlers
{
    using Messages;

    /// <summary>
    /// Network message handlers for role system.
    /// </summary>
    public static class RoomRoleNetworkHandlers
    {
        private static bool _serverHandlersRegistered;
        private static bool _clientHandlersRegistered;

        #region Handler Registration

        public static void RegisterServerHandlers()
        {
            if (_serverHandlersRegistered)
            {
                Debug.LogWarning("[RoomRoleNetworkHandlers] Server handlers already registered");
                return;
            }

            NetworkServer.RegisterHandler<RoleAssignmentRequest>(OnServerRoleAssignment);
            _serverHandlersRegistered = true;
        }

        public static void RegisterClientHandlers()
        {
            if (_clientHandlersRegistered)
            {
                Debug.LogWarning("[RoomRoleNetworkHandlers] Client handlers already registered");
                return;
            }

            NetworkClient.RegisterHandler<RoomRoleChangeMessage>(OnClientRoleChange);
            NetworkClient.RegisterHandler<RoomRoleListMessage>(OnClientRoleList);
            _clientHandlersRegistered = true;
        }

        public static void UnregisterServerHandlers()
        {
            if (!_serverHandlersRegistered) return;

            NetworkServer.UnregisterHandler<RoleAssignmentRequest>();
            _serverHandlersRegistered = false;
        }

        public static void UnregisterClientHandlers()
        {
            if (!_clientHandlersRegistered) return;

            NetworkClient.UnregisterHandler<RoomRoleChangeMessage>();
            NetworkClient.UnregisterHandler<RoomRoleListMessage>();
            _clientHandlersRegistered = false;
        }

        #endregion

        #region Server Handlers

        /// <summary>
        /// Server receives role assignment request from client.
        /// </summary>
        private static void OnServerRoleAssignment(NetworkConnectionToClient conn, RoleAssignmentRequest msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
            {
                Debug.LogWarning("[RoomRoleNetworkHandlers] RoomManagerBase instance not found");
                return;
            }

            var room = roomManager.GetRoom(msg.RoomID);
            if (room == null || room.RoleManager == null)
            {
                Debug.LogWarning($"[RoomRoleNetworkHandlers] Room ID {msg.RoomID} not found or role system not enabled");
                return;
            }

            var target = GetConnectionByID(msg.TargetConnectionID);
            if (target == null)
            {
                Debug.LogWarning($"[RoomRoleNetworkHandlers] Target connection ID {msg.TargetConnectionID} not found");
                return;
            }

            if (room.RoleManager.AssignRole(conn, target, msg.Role))
            {
                BroadcastRoleChange(room, target, msg.Role);
            }
        }

        #endregion

        #region Client Handlers

        /// <summary>
        /// Client receives role change notification.
        /// </summary>
        private static void OnClientRoleChange(RoomRoleChangeMessage msg)
        {
            OnClientRoomRoleChanged?.Invoke(msg.RoomID, msg.TargetConnectionID, msg.NewRole, msg.CustomPermissions);
        }

        /// <summary>
        /// Client receives full role list (on room join).
        /// </summary>
        private static void OnClientRoleList(RoomRoleListMessage msg)
        {
            OnClientRoomRoleListReceived?.Invoke(msg.RoomID, msg.Roles);
        }

        #endregion

        #region Broadcasting

        /// <summary>
        /// Broadcasts role change to all clients in room.
        /// </summary>
        public static void BroadcastRoleChange(Room room, NetworkConnection target, RoomRole newRole)
        {
            if (room?.RoleManager == null) return;

            var customPerms = room.RoleManager.GetEffectivePermissions(target) ^
                              room.RoleManager.GetRolePermissions(newRole);

            var targetConnId = (target as NetworkConnectionToClient)?.connectionId ?? target.GetHashCode();

            var message = new RoomRoleChangeMessage
            {
                RoomID = room.ID,
                TargetConnectionID = (uint)targetConnId,
                NewRole = newRole,
                CustomPermissions = customPerms
            };

            foreach (var conn in room.Connections)
            {
                conn.Send(message);
            }
        }

        /// <summary>
        /// Sends full role list to newly joined player.
        /// </summary>
        public static void SendRoleListToClient(NetworkConnection conn, Room room)
        {
            if (room?.RoleManager == null) return;

            var entries = new List<RoomRoleEntry>();

            foreach (var connection in room.Connections)
            {
                var role = room.RoleManager.GetPlayerRole(connection);
                var perms = room.RoleManager.GetEffectivePermissions(connection);
                var rolePerms = room.RoleManager.GetRolePermissions(role);
                var connId = (connection as NetworkConnectionToClient)?.connectionId ?? connection.GetHashCode();

                entries.Add(new RoomRoleEntry
                {
                    ConnectionID = (uint)connId,
                    Role = role,
                    CustomPermissions = perms ^ rolePerms
                });
            }

            var message = new RoomRoleListMessage
            {
                RoomID = room.ID,
                Roles = entries.ToArray()
            };

            conn.Send(message);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper: Gets NetworkConnection by ID.
        /// </summary>
        private static NetworkConnection GetConnectionByID(uint connectionId)
        {
            return NetworkServer.connections.TryGetValue((int)connectionId, out var conn) ? conn : null;
        }

        #endregion

        #region Client Events

        public delegate void ClientRoleChangedHandler(uint roomID, uint connectionID, RoomRole newRole, int customPermissions);
        public delegate void ClientRoleListReceivedHandler(uint roomID, RoomRoleEntry[] roles);

        public static event ClientRoleChangedHandler OnClientRoomRoleChanged;
        public static event ClientRoleListReceivedHandler OnClientRoomRoleListReceived;

        public static void ClearClientEvents()
        {
            OnClientRoomRoleChanged = null;
            OnClientRoomRoleListReceived = null;
        }

        #endregion
    }
}
