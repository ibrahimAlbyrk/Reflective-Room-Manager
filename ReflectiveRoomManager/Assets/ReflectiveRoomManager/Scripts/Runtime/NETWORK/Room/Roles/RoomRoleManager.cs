using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles
{
    using Events;

    /// <summary>
    /// Manages player roles and permissions within a room.
    /// Server-authoritative role assignment.
    /// </summary>
    public class RoomRoleManager
    {
        private readonly Room _room;
        private readonly RoomRoleConfig _config;
        private readonly Dictionary<NetworkConnection, PlayerRoleData> _playerRoles;
        private readonly Dictionary<RoomRole, int> _rolePermissions;
        private readonly RoomEventManager _eventManager;

        public RoomRoleManager(Room room, RoomRoleConfig config, RoomEventManager eventManager)
        {
            _room = room;
            _config = config ?? CreateDefaultConfig();
            _eventManager = eventManager;
            _playerRoles = new Dictionary<NetworkConnection, PlayerRoleData>();
            _rolePermissions = new Dictionary<RoomRole, int>();

            InitializeDefaultPermissions();
        }

        /// <summary>
        /// Initializes default permission mappings from config.
        /// </summary>
        private void InitializeDefaultPermissions()
        {
            foreach (var mapping in _config.DefaultPermissions)
            {
                _rolePermissions[mapping.Role] = mapping.GetCombinedPermissions();
            }
        }

        /// <summary>
        /// Creates fallback config if none provided.
        /// </summary>
        private static RoomRoleConfig CreateDefaultConfig()
        {
            var config = ScriptableObject.CreateInstance<RoomRoleConfig>();
            return config;
        }

        #region Role Assignment

        /// <summary>
        /// Assigns a role to a player.
        /// Server-only operation.
        /// </summary>
        /// <param name="actor">Player performing the assignment (must have permission)</param>
        /// <param name="target">Player receiving the role</param>
        /// <param name="role">Role to assign</param>
        /// <returns>True if assignment successful</returns>
        public bool AssignRole(NetworkConnection actor, NetworkConnection target, RoomRole role)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("[RoomRoleManager] AssignRole can only be called on server");
                return false;
            }

            if (!ValidateRoleAssignment(actor, target, role, out var reason))
            {
                if (_config.EnableDebugLogs)
                    Debug.LogWarning($"[RoomRoleManager] Role assignment denied: {reason}");
                return false;
            }

            var previousRole = GetPlayerRole(target);
            var oldOwner = previousRole != RoomRole.Owner && role == RoomRole.Owner
                ? GetPlayersWithRole(RoomRole.Owner).FirstOrDefault()
                : null;

            // Get or create PlayerRoleData
            if (!_playerRoles.TryGetValue(target, out var roleData))
            {
                roleData = new PlayerRoleData(target, role);
                _playerRoles[target] = roleData;
            }
            else
            {
                roleData.SetRole(role);
                roleData.ClearCustomPermissions();
            }

            if (_config.EnableDebugLogs)
                Debug.Log($"[RoomRoleManager] Player assigned role: {role} in room '{_room.Name}'");

            // Trigger events
            _eventManager?.Invoke_OnServerRoleChanged(_room.ID, target, role);

            // Handle ownership transfer
            if (role == RoomRole.Owner && oldOwner != null && oldOwner != target)
            {
                // Demote old owner to Admin
                if (_playerRoles.TryGetValue(oldOwner, out var oldOwnerData))
                {
                    oldOwnerData.SetRole(RoomRole.Admin);
                    _eventManager?.Invoke_OnServerRoleChanged(_room.ID, oldOwner, RoomRole.Admin);
                }
                _eventManager?.Invoke_OnServerOwnershipTransferred(_room.ID, oldOwner, target);
            }

            return true;
        }

        /// <summary>
        /// Removes explicit role assignment (reverts to default).
        /// </summary>
        public bool RevokeRole(NetworkConnection actor, NetworkConnection target)
        {
            if (!CanActOn(actor, target))
                return false;

            if (_playerRoles.ContainsKey(target))
            {
                var defaultRole = _config.DefaultJoinRole;
                return AssignRole(actor, target, defaultRole);
            }

            return false;
        }

        /// <summary>
        /// Validates if actor can assign role to target.
        /// </summary>
        private bool ValidateRoleAssignment(NetworkConnection actor, NetworkConnection target, RoomRole role, out string reason)
        {
            reason = null;

            // Self-assignment for initial owner setup
            if (actor == target && role == RoomRole.Owner && GetPlayersWithRole(RoomRole.Owner).Count == 0)
                return true;

            // Room owner can always assign roles
            var actorRole = GetPlayerRole(actor);
            if (actorRole == RoomRole.Owner)
                return true;

            // Check if actor can act on target
            if (!CanActOn(actor, target))
            {
                reason = "Insufficient role priority";
                return false;
            }

            // Check if actor can assign this role (must be higher priority)
            if (role >= actorRole)
            {
                reason = $"Cannot assign role {role} (equal or higher than your role {actorRole})";
                return false;
            }

            return true;
        }

        #endregion

        #region Permission Checks

        /// <summary>
        /// Checks if player has specific permission.
        /// </summary>
        public bool HasPermission(NetworkConnection conn, RoomPermission permission)
        {
            if (!_playerRoles.TryGetValue(conn, out var roleData))
            {
                var defaultPerms = GetRolePermissions(_config.DefaultJoinRole);
                return (defaultPerms & (int)permission) != 0;
            }

            var rolePerms = GetRolePermissions(roleData.Role);
            return roleData.HasPermission(permission, rolePerms);
        }

        /// <summary>
        /// Checks if actor can perform actions on target (hierarchical).
        /// </summary>
        public bool CanActOn(NetworkConnection actor, NetworkConnection target)
        {
            var actorRole = GetPlayerRole(actor);
            var targetRole = GetPlayerRole(target);

            return actorRole > targetRole;
        }

        /// <summary>
        /// Gets role permissions (bitwise combined).
        /// </summary>
        public int GetRolePermissions(RoomRole role)
        {
            return _rolePermissions.TryGetValue(role, out var perms) ? perms : 0;
        }

        #endregion

        #region Player Queries

        /// <summary>
        /// Gets player's current role.
        /// </summary>
        public RoomRole GetPlayerRole(NetworkConnection conn)
        {
            if (_playerRoles.TryGetValue(conn, out var roleData))
                return roleData.Role;

            return _config.DefaultJoinRole;
        }

        /// <summary>
        /// Gets player's effective permissions (role + custom).
        /// </summary>
        public int GetEffectivePermissions(NetworkConnection conn)
        {
            if (!_playerRoles.TryGetValue(conn, out var roleData))
            {
                return GetRolePermissions(_config.DefaultJoinRole);
            }

            var rolePerms = GetRolePermissions(roleData.Role);
            return roleData.GetEffectivePermissions(rolePerms);
        }

        /// <summary>
        /// Gets all players with specific role.
        /// </summary>
        public List<NetworkConnection> GetPlayersWithRole(RoomRole role)
        {
            var result = new List<NetworkConnection>();
            foreach (var kvp in _playerRoles)
            {
                if (kvp.Value.Role == role)
                    result.Add(kvp.Key);
            }
            return result;
        }

        /// <summary>
        /// Gets all player role data for network sync.
        /// </summary>
        public IReadOnlyDictionary<NetworkConnection, PlayerRoleData> GetAllPlayerRoles()
        {
            return _playerRoles;
        }

        #endregion

        #region Custom Permissions

        /// <summary>
        /// Grants custom permission to a player (beyond role).
        /// </summary>
        public bool GrantPermission(NetworkConnection actor, NetworkConnection target, RoomPermission permission)
        {
            if (!CanActOn(actor, target))
                return false;

            if (!_playerRoles.TryGetValue(target, out var roleData))
            {
                roleData = new PlayerRoleData(target, _config.DefaultJoinRole);
                _playerRoles[target] = roleData;
            }

            roleData.GrantPermission(permission);
            _eventManager?.Invoke_OnServerPermissionGranted(_room.ID, target, permission);
            return true;
        }

        /// <summary>
        /// Revokes custom permission from a player.
        /// </summary>
        public bool RevokePermission(NetworkConnection actor, NetworkConnection target, RoomPermission permission)
        {
            if (!CanActOn(actor, target))
                return false;

            if (_playerRoles.TryGetValue(target, out var roleData))
            {
                roleData.RevokePermission(permission);
                _eventManager?.Invoke_OnServerPermissionRevoked(_room.ID, target, permission);
                return true;
            }

            return false;
        }

        #endregion

        #region Custom Roles

        /// <summary>
        /// Registers a custom role with permission set.
        /// For developers extending the role system.
        /// </summary>
        public void RegisterCustomRole(RoomRole customRole, int permissions)
        {
            if (!_config.AllowCustomRoles)
            {
                Debug.LogWarning("[RoomRoleManager] Custom roles not allowed in config");
                return;
            }

            _rolePermissions[customRole] = permissions;
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Called when player joins room.
        /// Auto-assigns default role if configured.
        /// </summary>
        internal void OnPlayerJoined(NetworkConnection conn)
        {
            if (_config.AutoAssignMemberOnJoin)
            {
                var roleData = new PlayerRoleData(conn, _config.DefaultJoinRole);
                _playerRoles[conn] = roleData;

                if (_config.EnableDebugLogs)
                    Debug.Log($"[RoomRoleManager] Auto-assigned {_config.DefaultJoinRole} to player");
            }
        }

        /// <summary>
        /// Called when player leaves room.
        /// </summary>
        internal void OnPlayerLeft(NetworkConnection conn)
        {
            var wasOwner = _playerRoles.TryGetValue(conn, out var roleData) && roleData.Role == RoomRole.Owner;

            _playerRoles.Remove(conn);

            if (wasOwner)
            {
                TransferOwnershipToNext(conn);
            }
        }

        /// <summary>
        /// Transfers ownership to next highest priority player.
        /// </summary>
        private void TransferOwnershipToNext(NetworkConnection previousOwner)
        {
            if (GetPlayersWithRole(RoomRole.Owner).Count > 0)
                return;

            NetworkConnection newOwner = null;
            var highestRole = RoomRole.Guest;

            foreach (var kvp in _playerRoles)
            {
                if (kvp.Value.Role > highestRole)
                {
                    highestRole = kvp.Value.Role;
                    newOwner = kvp.Key;
                }
            }

            if (newOwner != null && _room.Connections.Contains(newOwner))
            {
                _playerRoles[newOwner].SetRole(RoomRole.Owner);

                if (_config.EnableDebugLogs)
                    Debug.Log($"[RoomRoleManager] Ownership transferred to player with role {highestRole}");

                _eventManager?.Invoke_OnServerRoleChanged(_room.ID, newOwner, RoomRole.Owner);
                _eventManager?.Invoke_OnServerOwnershipTransferred(_room.ID, previousOwner, newOwner);
            }
            else if (_room.Connections.Count > 0)
            {
                newOwner = _room.Connections[0];
                var roleData = new PlayerRoleData(newOwner, RoomRole.Owner);
                _playerRoles[newOwner] = roleData;

                if (_config.EnableDebugLogs)
                    Debug.Log("[RoomRoleManager] Ownership assigned to first remaining player");

                _eventManager?.Invoke_OnServerRoleChanged(_room.ID, newOwner, RoomRole.Owner);
                _eventManager?.Invoke_OnServerOwnershipTransferred(_room.ID, previousOwner, newOwner);
            }
        }

        /// <summary>
        /// Cleans up all role data (on room close).
        /// </summary>
        internal void Cleanup()
        {
            _playerRoles.Clear();
        }

        #endregion
    }
}
