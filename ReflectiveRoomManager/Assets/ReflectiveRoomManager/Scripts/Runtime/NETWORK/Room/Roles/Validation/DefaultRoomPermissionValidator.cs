using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles.Validation
{
    /// <summary>
    /// Default permission validator using RoomRoleManager.
    /// </summary>
    public class DefaultRoomPermissionValidator : IRoomPermissionValidator
    {
        public bool ValidatePermission(
            NetworkConnection actor,
            RoomPermission requiredPermission,
            Room room,
            out string reason)
        {
            reason = null;

            if (room.RoleManager == null)
            {
                // No role manager - always allow (backward compatibility)
                return true;
            }

            if (!room.RoleManager.HasPermission(actor, requiredPermission))
            {
                reason = $"Missing permission: {requiredPermission}";
                return false;
            }

            return true;
        }

        public bool ValidatePermissionOnTarget(
            NetworkConnection actor,
            NetworkConnection target,
            RoomPermission requiredPermission,
            Room room,
            out string reason)
        {
            reason = null;

            if (room.RoleManager == null)
            {
                return true;
            }

            // Check permission
            if (!room.RoleManager.HasPermission(actor, requiredPermission))
            {
                reason = $"Missing permission: {requiredPermission}";
                return false;
            }

            // Check hierarchy
            if (!room.RoleManager.CanActOn(actor, target))
            {
                reason = "Cannot act on player with equal or higher role";
                return false;
            }

            return true;
        }
    }
}
