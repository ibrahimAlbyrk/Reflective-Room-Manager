using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    using Room;

    public class DefaultReconnectionHandler : MonoBehaviour, IReconnectionHandler
    {
        private float _gracePeriodSeconds = 30f;

        public float GracePeriodSeconds => _gracePeriodSeconds;

        public void SetGracePeriod(float seconds) => _gracePeriodSeconds = seconds;

        public bool CanReconnect(string playerId, Room room)
        {
            return room != null;
        }

        public void OnReconnected(string playerId, NetworkConnectionToClient newConn, Room room)
        {
        }

        public void OnGracePeriodExpired(string playerId, Room room)
        {
        }
    }
}
