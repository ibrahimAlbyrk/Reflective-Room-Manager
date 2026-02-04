using System;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles
{
    /// <summary>
    /// ScriptableObject configuration for room roles.
    /// Designer-friendly permission mapping.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomRoleConfig", menuName = "REFLECTIVE/Room Role Config")]
    public class RoomRoleConfig : ScriptableObject
    {
        [Header("Default Permissions")]
        [Tooltip("Default permission mappings for each role")]
        public RolePermissionMapping[] DefaultPermissions = new RolePermissionMapping[]
        {
            new RolePermissionMapping
            {
                Role = RoomRole.Guest,
                Permissions = new RoomPermission[] { RoomPermission.None }
            },
            new RolePermissionMapping
            {
                Role = RoomRole.Member,
                Permissions = new RoomPermission[]
                {
                    RoomPermission.ChatSend,
                    RoomPermission.VoiceComm
                }
            },
            new RolePermissionMapping
            {
                Role = RoomRole.Moderator,
                Permissions = new RoomPermission[]
                {
                    RoomPermission.ChatSend,
                    RoomPermission.VoiceComm,
                    RoomPermission.Kick,
                    RoomPermission.Mute
                }
            },
            new RolePermissionMapping
            {
                Role = RoomRole.Admin,
                Permissions = new RoomPermission[]
                {
                    RoomPermission.ChatSend,
                    RoomPermission.VoiceComm,
                    RoomPermission.Kick,
                    RoomPermission.Ban,
                    RoomPermission.Mute,
                    RoomPermission.ChangeSettings,
                    RoomPermission.StartGame,
                    RoomPermission.EndGame,
                    RoomPermission.AssignTeams,
                    RoomPermission.TeamBalance,
                    RoomPermission.ForceSpectator
                }
            },
            new RolePermissionMapping
            {
                Role = RoomRole.Owner,
                Permissions = new RoomPermission[] { RoomPermission.All }
            }
        };

        [Header("Role Assignment")]
        [Tooltip("Allow custom roles beyond the 5 standard roles")]
        public bool AllowCustomRoles;

        [Tooltip("Auto-assign Member role when player joins")]
        public bool AutoAssignMemberOnJoin = true;

        [Tooltip("Default role for joining players (if auto-assign enabled)")]
        public RoomRole DefaultJoinRole = RoomRole.Member;

        [Header("Advanced")]
        [Tooltip("Enable role debug logging")]
        public bool EnableDebugLogs = true;

        /// <summary>
        /// Gets default permissions for a role.
        /// </summary>
        public int GetRolePermissions(RoomRole role)
        {
            var mapping = Array.Find(DefaultPermissions, m => m.Role == role);
            return mapping.GetCombinedPermissions();
        }
    }
}
