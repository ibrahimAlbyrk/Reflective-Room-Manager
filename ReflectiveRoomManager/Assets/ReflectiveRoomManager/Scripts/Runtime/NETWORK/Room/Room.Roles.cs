using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Roles;
    using Events;

    /// <summary>
    /// Partial class extension for Room role system support.
    /// </summary>
    public partial class Room
    {
        /// <summary>
        /// Role manager instance (null if roles not enabled).
        /// </summary>
        public RoomRoleManager RoleManager { get; private set; }

        /// <summary>
        /// Initializes role manager for this room.
        /// Called during room creation.
        /// </summary>
        internal void InitializeRoleManager(RoomRoleConfig config, RoomEventManager eventManager)
        {
            if (RoleManager != null)
            {
                Debug.LogWarning($"[Room] Role manager already initialized for room '{Name}'");
                return;
            }

            if (config == null)
            {
                Debug.LogError($"[Room] Cannot initialize role manager for room '{Name}': config is null");
                return;
            }

            RoleManager = new RoomRoleManager(this, config, eventManager);
        }

        /// <summary>
        /// Called when player joins (notify role manager).
        /// </summary>
        internal void NotifyPlayerJoinedForRoles(NetworkConnection conn)
        {
            RoleManager?.OnPlayerJoined(conn);
        }

        /// <summary>
        /// Called when player leaves (notify role manager).
        /// </summary>
        internal void NotifyPlayerLeftForRoles(NetworkConnection conn)
        {
            RoleManager?.OnPlayerLeft(conn);
        }

        /// <summary>
        /// Assigns initial owner role to room creator.
        /// </summary>
        internal void AssignInitialOwner(NetworkConnection owner)
        {
            RoleManager?.AssignRole(owner, owner, RoomRole.Owner);
        }

        /// <summary>
        /// Cleans up role manager (on room close).
        /// </summary>
        internal void CleanupRoleManager()
        {
            RoleManager?.Cleanup();
            RoleManager = null;
        }
    }
}
