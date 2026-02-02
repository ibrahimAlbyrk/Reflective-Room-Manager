using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Validation
{
    public interface IRoomAccessValidator
    {
        bool ValidateAccess(NetworkConnection conn, Room room, string accessToken, out string reason);
    }
}
