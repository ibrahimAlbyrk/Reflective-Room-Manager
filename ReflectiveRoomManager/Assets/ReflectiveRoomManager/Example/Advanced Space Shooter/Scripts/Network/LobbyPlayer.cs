using Mirror;
using UnityEngine;

namespace Examples.SpaceShooter.Network
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class LobbyPlayer : NetworkBehaviour
    {
        public static LobbyPlayer LocalLobbyPlayer;
        
        public GameObject ShipPrefab;
        
        [ClientCallback]
        private void Start()
        {
            if (isLocalPlayer)
                LocalLobbyPlayer = this;
        }
    }
}