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
#if REFLECTIVE_SERVER
        private static bool _serverHandlersRegistered;

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

        public static void UnregisterServerHandlers()
        {
            if (!_serverHandlersRegistered) return;

            NetworkServer.UnregisterHandler<RoleAssignmentRequest>();
            _serverHandlersRegistered = false;
        }

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

        private static NetworkConnection GetConnectionByID(uint connectionId)
        {
            return NetworkServer.connections.TryGetValue((int)connectionId, out var conn) ? conn : null;
        }
#endif

#if REFLECTIVE_CLIENT
        private static bool _clientHandlersRegistered;

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

        public static void UnregisterClientHandlers()
        {
            if (!_clientHandlersRegistered) return;

            NetworkClient.UnregisterHandler<RoomRoleChangeMessage>();
            NetworkClient.UnregisterHandler<RoomRoleListMessage>();
            _clientHandlersRegistered = false;
        }

        private static void OnClientRoleChange(RoomRoleChangeMessage msg)
        {
            OnClientRoomRoleChanged?.Invoke(msg.RoomID, msg.TargetConnectionID, msg.NewRole, msg.CustomPermissions);
        }

        private static void OnClientRoleList(RoomRoleListMessage msg)
        {
            OnClientRoomRoleListReceived?.Invoke(msg.RoomID, msg.Roles);
        }

        public delegate void ClientRoleChangedHandler(uint roomID, uint connectionID, RoomRole newRole, int customPermissions);
        public delegate void ClientRoleListReceivedHandler(uint roomID, RoomRoleEntry[] roles);

        public static event ClientRoleChangedHandler OnClientRoomRoleChanged;
        public static event ClientRoleListReceivedHandler OnClientRoomRoleListReceived;

        public static void ClearClientEvents()
        {
            OnClientRoomRoleChanged = null;
            OnClientRoomRoleListReceived = null;
        }
#endif
    }
}
