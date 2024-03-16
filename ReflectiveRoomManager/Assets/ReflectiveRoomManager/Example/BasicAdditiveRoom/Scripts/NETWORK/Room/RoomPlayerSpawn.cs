using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Player.Utilities;

namespace Examples.Basic.Network.Room
{
    public class RoomPlayerSpawn : NetworkBehaviour
    {
        [SerializeField] private GameObject _gamePlayerPrefab;
        [SerializeField] private GameObject _lobbyPlayerPrefab;

        //If true, it creates a player object for your player in the lobby when you leave the room.
        //If your project has such a design, mark it.
        [SerializeField] private bool _useLobby;
        
        // Necessary event assignments are being made
        [ServerCallback]
        private void Start()
        {
            RoomManagerBase.Instance.Events.OnServerJoinedRoom += CreateGamePlayer;
            
            RoomManagerBase.Instance.Events.OnServerExitedRoom += PlayerCreatorUtilities.RemovePlayer;

            //If the lobby option is used, create a player for the lobby when leaving the room,
            //otherwise it will only delete the player.
            if (_useLobby)
                RoomManagerBase.Instance.Events.OnServerExitedRoom += CreateLobbyPlayer;

        }
        
        [ServerCallback]
        private void CreateGamePlayer(NetworkConnection conn, uint roomID)
        {
            PlayerCreatorUtilities.TryCreatePlayerOrReplace(conn, _gamePlayerPrefab);
        }
        
        [ServerCallback]
        private void CreateLobbyPlayer(NetworkConnection conn)
        {
            PlayerCreatorUtilities.TryCreatePlayerOrReplace(conn, _lobbyPlayerPrefab);
        }
    }
}