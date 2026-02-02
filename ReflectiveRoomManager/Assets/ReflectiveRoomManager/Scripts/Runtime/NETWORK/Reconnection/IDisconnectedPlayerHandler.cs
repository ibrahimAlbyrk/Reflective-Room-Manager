using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    using Room;

    public interface IDisconnectedPlayerHandler
    {
        void OnPlayerDisconnected(GameObject playerObject, Room room);
        void OnPlayerReconnected(GameObject playerObject, NetworkConnectionToClient newConn, Room room);
        void OnPlayerAbandoned(GameObject playerObject, Room room);
    }
}
