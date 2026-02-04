using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles.Messages
{
    /// <summary>
    /// Sent from server to client with full room role data (on join).
    /// </summary>
    public struct RoomRoleListMessage : NetworkMessage
    {
        public uint RoomID;
        public RoomRoleEntry[] Roles;
    }

    /// <summary>
    /// Single player role entry for network sync.
    /// </summary>
    [Serializable]
    public struct RoomRoleEntry
    {
        public uint ConnectionID;
        public RoomRole Role;
        public int CustomPermissions;
    }
}
