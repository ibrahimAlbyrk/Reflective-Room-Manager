using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Validation
{
    using Structs;

    public class DefaultRoomValidator : IRoomValidator
    {
        public bool CanCreateRoom(NetworkConnection conn, RoomInfo roomInfo, out string reason)
        {
            reason = null;
            return true;
        }

        public bool CanJoinRoom(NetworkConnection conn, Room room, out string reason)
        {
            reason = null;
            return true;
        }
    }
}
