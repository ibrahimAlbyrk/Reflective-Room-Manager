using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles.Extensions
{
    /// <summary>
    /// Extension methods for convenient permission checks.
    /// </summary>
    public static class RoomPermissionExtensions
    {
        /// <summary>
        /// Checks if player can send chat messages.
        /// </summary>
        public static bool CanChat(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.ChatSend) ?? false;
        }

        /// <summary>
        /// Checks if player can use voice communication.
        /// </summary>
        public static bool CanVoice(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.VoiceComm) ?? false;
        }

        /// <summary>
        /// Checks if player can kick others.
        /// </summary>
        public static bool CanKick(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.Kick) ?? false;
        }

        /// <summary>
        /// Checks if player can ban others.
        /// </summary>
        public static bool CanBan(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.Ban) ?? false;
        }

        /// <summary>
        /// Checks if player can mute others.
        /// </summary>
        public static bool CanMute(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.Mute) ?? false;
        }

        /// <summary>
        /// Checks if player can change room settings.
        /// </summary>
        public static bool CanChangeSettings(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.ChangeSettings) ?? false;
        }

        /// <summary>
        /// Checks if player can start game.
        /// </summary>
        public static bool CanStartGame(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.StartGame) ?? false;
        }

        /// <summary>
        /// Checks if player can end game.
        /// </summary>
        public static bool CanEndGame(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.EndGame) ?? false;
        }

        /// <summary>
        /// Checks if player can assign teams.
        /// </summary>
        public static bool CanAssignTeams(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.AssignTeams) ?? false;
        }

        /// <summary>
        /// Checks if player can balance teams.
        /// </summary>
        public static bool CanBalanceTeams(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.TeamBalance) ?? false;
        }

        /// <summary>
        /// Checks if player can force spectator.
        /// </summary>
        public static bool CanForceSpectator(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.ForceSpectator) ?? false;
        }

        /// <summary>
        /// Checks if player can close room.
        /// </summary>
        public static bool CanCloseRoom(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.CloseRoom) ?? false;
        }

        /// <summary>
        /// Checks if player can transfer ownership.
        /// </summary>
        public static bool CanTransferOwnership(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.HasPermission(conn, RoomPermission.TransferOwnership) ?? false;
        }

        /// <summary>
        /// Checks if player is owner.
        /// </summary>
        public static bool IsOwner(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.GetPlayerRole(conn) == RoomRole.Owner;
        }

        /// <summary>
        /// Checks if player is admin or higher.
        /// </summary>
        public static bool IsAdminOrHigher(this Room room, NetworkConnection conn)
        {
            if (room.RoleManager == null) return false;
            var role = room.RoleManager.GetPlayerRole(conn);
            return role >= RoomRole.Admin;
        }

        /// <summary>
        /// Checks if player is moderator or higher.
        /// </summary>
        public static bool IsModeratorOrHigher(this Room room, NetworkConnection conn)
        {
            if (room.RoleManager == null) return false;
            var role = room.RoleManager.GetPlayerRole(conn);
            return role >= RoomRole.Moderator;
        }

        /// <summary>
        /// Checks if player can act on target.
        /// </summary>
        public static bool CanActOn(this Room room, NetworkConnection actor, NetworkConnection target)
        {
            return room.RoleManager?.CanActOn(actor, target) ?? false;
        }

        /// <summary>
        /// Gets player's role.
        /// </summary>
        public static RoomRole GetRole(this Room room, NetworkConnection conn)
        {
            return room.RoleManager?.GetPlayerRole(conn) ?? RoomRole.Guest;
        }
    }
}
