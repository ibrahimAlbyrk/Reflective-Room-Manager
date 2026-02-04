using System;
using System.Linq;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles
{
    /// <summary>
    /// Maps a role to its default permissions.
    /// Used in RoomRoleConfig ScriptableObject.
    /// </summary>
    [Serializable]
    public struct RolePermissionMapping
    {
        public RoomRole Role;
        public RoomPermission[] Permissions;

        /// <summary>
        /// Combines permission array into single bitwise int.
        /// </summary>
        public int GetCombinedPermissions()
        {
            if (Permissions == null || Permissions.Length == 0)
                return 0;

            return Permissions.Aggregate(0, (current, perm) => current | (int)perm);
        }
    }
}
