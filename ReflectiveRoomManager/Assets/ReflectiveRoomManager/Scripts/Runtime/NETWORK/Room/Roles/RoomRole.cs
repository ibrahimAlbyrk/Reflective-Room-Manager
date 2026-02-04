namespace REFLECTIVE.Runtime.NETWORK.Room.Roles
{
    /// <summary>
    /// Hierarchical room roles.
    /// Higher numeric value = higher priority.
    /// </summary>
    public enum RoomRole : byte
    {
        /// <summary>Spectator only, minimal permissions</summary>
        Guest = 0,

        /// <summary>Regular player, basic gameplay permissions</summary>
        Member = 1,

        /// <summary>Limited moderation - mute/kick only</summary>
        Moderator = 2,

        /// <summary>Full moderation + room settings</summary>
        Admin = 3,

        /// <summary>Owner - all permissions, can transfer ownership</summary>
        Owner = 4
    }
}
