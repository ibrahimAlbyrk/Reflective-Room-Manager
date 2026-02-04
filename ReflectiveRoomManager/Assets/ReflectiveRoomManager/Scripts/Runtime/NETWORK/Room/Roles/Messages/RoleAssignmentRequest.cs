using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Roles.Messages
{
    /// <summary>
    /// Sent from client to server to request role assignment.
    /// </summary>
    public struct RoleAssignmentRequest : NetworkMessage
    {
        public uint RoomID;
        public uint TargetConnectionID;
        public RoomRole Role;
    }
}
