using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Validation
{
    public class DefaultRoomAccessValidator : IRoomAccessValidator
    {
        public bool ValidateAccess(NetworkConnection conn, Room room, string accessToken, out string reason)
        {
            reason = null;
            return true;
        }
    }
}
