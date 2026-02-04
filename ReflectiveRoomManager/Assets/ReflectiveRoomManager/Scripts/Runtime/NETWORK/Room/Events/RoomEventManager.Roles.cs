using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Events
{
    using Roles;

    /// <summary>
    /// Partial class extension for RoomEventManager role events.
    /// </summary>
    public partial class RoomEventManager
    {
        /// <summary>Called on server when player role changes</summary>
        public event Action<uint, NetworkConnection, RoomRole> OnServerRoleChanged;

        /// <summary>Called on server when custom permission granted</summary>
        public event Action<uint, NetworkConnection, RoomPermission> OnServerPermissionGranted;

        /// <summary>Called on server when custom permission revoked</summary>
        public event Action<uint, NetworkConnection, RoomPermission> OnServerPermissionRevoked;

        /// <summary>Called on server when ownership transferred</summary>
        public event Action<uint, NetworkConnection, NetworkConnection> OnServerOwnershipTransferred;

        internal void Invoke_OnServerRoleChanged(uint roomID, NetworkConnection conn, RoomRole newRole)
        {
            OnServerRoleChanged?.Invoke(roomID, conn, newRole);
        }

        internal void Invoke_OnServerPermissionGranted(uint roomID, NetworkConnection conn, RoomPermission perm)
        {
            OnServerPermissionGranted?.Invoke(roomID, conn, perm);
        }

        internal void Invoke_OnServerPermissionRevoked(uint roomID, NetworkConnection conn, RoomPermission perm)
        {
            OnServerPermissionRevoked?.Invoke(roomID, conn, perm);
        }

        internal void Invoke_OnServerOwnershipTransferred(uint roomID, NetworkConnection oldOwner, NetworkConnection newOwner)
        {
            OnServerOwnershipTransferred?.Invoke(roomID, oldOwner, newOwner);
        }
    }
}
