using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Reconnection
{
    using Room;

    public class DefaultDisconnectedPlayerHandler : MonoBehaviour, IDisconnectedPlayerHandler
    {
        public void OnPlayerDisconnected(GameObject playerObject, Room room)
        {
            if (playerObject == null) return;

            var identity = playerObject.GetComponent<NetworkIdentity>();
            if (identity == null) return;

            var conn = identity.connectionToClient;
            if (conn != null)
                NetworkServer.RemovePlayerForConnection(conn, false);

            foreach (var behaviour in playerObject.GetComponents<NetworkBehaviour>())
                behaviour.enabled = false;
        }

        public void OnPlayerReconnected(GameObject playerObject, NetworkConnectionToClient newConn, Room room)
        {
            if (playerObject == null) return;

            foreach (var behaviour in playerObject.GetComponents<NetworkBehaviour>())
                behaviour.enabled = true;

            NetworkServer.AddPlayerForConnection(newConn, playerObject);
        }

        public void OnPlayerAbandoned(GameObject playerObject, Room room)
        {
            if (playerObject == null) return;

            NetworkServer.Destroy(playerObject);
        }
    }
}
