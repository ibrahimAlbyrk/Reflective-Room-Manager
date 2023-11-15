using System.Linq;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Player.Utilities;
using REFLECTIVE.Runtime.NETWORK.Room.Service;

namespace Example.Basic.Network.Room
{
    using Character;
    
    public class RoomPlayerSpawn : MonoBehaviour
    {
        [SerializeField] private GameObject _gamePlayerPrefab;
        [SerializeField] private GameObject _lobbyPlayerPrefab;

        private int _playerCount;

        //If true, it creates a player object for your player in the lobby when you leave the room.
        //If your project has such a design, mark it.
        [SerializeField] private bool _useLobby;
        
        // Necessary event assignments are being made
        private void Start()
        {
            RoomManagerBase.Singleton.Events.OnServerJoinedRoom += CreateGamePlayer;

            //If the lobby option is used, create a player for the lobby when leaving the room,
            //otherwise it will only delete the player.
            if (_useLobby)
                RoomManagerBase.Singleton.Events.OnServerExitedRoom += CreateLobbyPlayer;
            else
                RoomManagerBase.Singleton.Events.OnServerExitedRoom += PlayerCreatorUtilities.RemovePlayer;
        }
        
        private async void CreateGamePlayer(NetworkConnection conn)
        {
            var player = PlayerCreatorUtilities.TryCreatePlayerOrReplace(conn, _gamePlayerPrefab);

            if (player.TryGetComponent(out SimpleCharacterController controller))
            {
                controller.ID = conn.connectionId;
            }
            
            await Task.Delay(2000);

            var room = RoomManagerBase.Singleton.GetRooms().First();
            
            RoomServer.ChangeScene(room, "Game_Scene");
        }
        
        private void CreateLobbyPlayer(NetworkConnection conn)
        {
            PlayerCreatorUtilities.ReplacePlayer(conn, _lobbyPlayerPrefab);
        }
    }
}