using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Validation
{
    public class PasswordRoomAccessValidator : MonoBehaviour, IRoomAccessValidator
    {
        public bool ValidateAccess(NetworkConnection conn, Room room, string accessToken, out string reason)
        {
            var customData = room.GetCustomData();

            if (!customData.TryGetValue("password", out var password) || string.IsNullOrEmpty(password))
            {
                reason = null;
                return true;
            }

            if (accessToken != password)
            {
                reason = "Wrong password";
                return false;
            }

            reason = null;
            return true;
        }
    }
}
