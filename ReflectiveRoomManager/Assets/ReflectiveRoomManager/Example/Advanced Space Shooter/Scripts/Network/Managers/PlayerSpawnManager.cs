using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Behaviour;
using REFLECTIVE.Runtime.NETWORK.Player.Utilities;

namespace Examples.SpaceShooter.Network.Managers
{
    public class PlayerSpawnManager : NetBehaviour
    {
        [SerializeField] private GameObject _lobbyPlayerPrefab;
        [SerializeField] private GameObject _gamePlayerPrefab;
        
        private void Start()
        {
            if (!isServer) return;

            if (NetworkManager.singleton is SpaceNetworkManager spaceNetworkManager)
            {
                spaceNetworkManager.OnServerReadied += CreateLobbyPlayer;
            }

            RoomManagerBase.Instance.Events.OnServerJoinedRoom += OnServerJoinedClient;
            RoomManagerBase.Instance.Events.OnServerExitedRoom += OnServerExitedClient;
        }

        private void CreateLobbyPlayer(NetworkConnection conn)
        {
            PlayerCreatorUtilities.CreatePlayer(conn, _lobbyPlayerPrefab);
        }
        
        [ServerCallback]
        private void OnServerJoinedClient(NetworkConnection conn, uint roomID)
        {
            var oldPlayer = conn.identity.gameObject;

            var lobbyPlayer = oldPlayer.GetComponent<LobbyPlayer>();

            var prefab = lobbyPlayer != null ? lobbyPlayer.ShipPrefab : _gamePlayerPrefab;
            
            PlayerCreatorUtilities.ReplacePlayer(conn, prefab);
        }
        
        [ServerCallback]
        private void OnServerExitedClient(NetworkConnection conn)
        {
            PlayerCreatorUtilities.ReplacePlayer(conn, _lobbyPlayerPrefab);
        }
    }
}