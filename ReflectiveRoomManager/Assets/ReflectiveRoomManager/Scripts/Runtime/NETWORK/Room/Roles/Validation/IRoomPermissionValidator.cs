using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles.Validation
{
    /// <summary>
    /// Interface for permission-based validation.
    /// Integrates with RoomRoleManager for RBAC.
    /// </summary>
    public interface IRoomPermissionValidator
    {
        /// <summary>
        /// Validates if actor has permission to perform action.
        /// </summary>
        bool ValidatePermission(
            NetworkConnection actor,
            RoomPermission requiredPermission,
            Room room,
            out string reason
        );

        /// <summary>
        /// Validates if actor can perform action on target.
        /// Checks both permission AND hierarchy (can act on).
        /// </summary>
        bool ValidatePermissionOnTarget(
            NetworkConnection actor,
            NetworkConnection target,
            RoomPermission requiredPermission,
            Room room,
            out string reason
        );
    }
}
