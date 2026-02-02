using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    using Room;

    public interface IPlayerStateSerializer
    {
        object CaptureState(NetworkConnectionToClient conn, Room room, GameObject playerObject);
        void RestoreState(NetworkConnectionToClient newConn, Room room, GameObject playerObject, object state);
    }
}
