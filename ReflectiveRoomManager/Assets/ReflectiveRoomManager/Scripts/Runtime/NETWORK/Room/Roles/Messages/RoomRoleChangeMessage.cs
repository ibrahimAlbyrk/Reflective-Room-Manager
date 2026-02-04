using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles.Messages
{
    /// <summary>
    /// Sent from server to client when player role changes.
    /// </summary>
    public struct RoomRoleChangeMessage : NetworkMessage
    {
        public uint RoomID;
        public uint TargetConnectionID;
        public RoomRole NewRole;
        public int CustomPermissions;
    }
}
