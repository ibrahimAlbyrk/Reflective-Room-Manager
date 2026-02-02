using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Validation
{
    using Structs;

    public interface IRoomValidator
    {
        bool CanCreateRoom(NetworkConnection conn, RoomInfo roomInfo, out string reason);
        bool CanJoinRoom(NetworkConnection conn, Room room, out string reason);
    }
}
