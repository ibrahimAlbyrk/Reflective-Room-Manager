using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles
{
    /// <summary>
    /// Per-player role and permission data.
    /// Stored in RoomRoleManager.
    /// </summary>
    [Serializable]
    public class PlayerRoleData
    {
        public NetworkConnection Connection { get; }
        public RoomRole Role { get; private set; }

        /// <summary>Custom permission overrides (bitwise flags)</summary>
        public int CustomPermissions { get; private set; }

        public PlayerRoleData(NetworkConnection conn, RoomRole role)
        {
            Connection = conn;
            Role = role;
            CustomPermissions = 0;
        }

        /// <summary>
        /// Gets effective permissions (role + custom).
        /// </summary>
        public int GetEffectivePermissions(int roleBasePermissions)
        {
            return roleBasePermissions | CustomPermissions;
        }

        /// <summary>
        /// Checks if player has specific permission.
        /// </summary>
        public bool HasPermission(RoomPermission permission, int roleBasePermissions)
        {
            var effective = GetEffectivePermissions(roleBasePermissions);
            return (effective & (int)permission) != 0;
        }

        internal void SetRole(RoomRole newRole)
        {
            Role = newRole;
        }

        internal void GrantPermission(RoomPermission permission)
        {
            CustomPermissions |= (int)permission;
        }

        internal void RevokePermission(RoomPermission permission)
        {
            CustomPermissions &= ~(int)permission;
        }

        internal void ClearCustomPermissions()
        {
            CustomPermissions = 0;
        }
    }
}
