using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace Examples.SpaceShooter.Game.Mod
{
    using Lobby;
    using Spaceship;
    using ShrinkingArea;

    [System.Serializable]
    public class ShrinkingAreaMod : OpenWorldMod
    {
        [Header("Shrinking Settings")]
        [SerializeField] private ShrinkingAreaSystem _shrinkingAreaSystem;
        [SerializeField] private GameLobbySystem gameLobbySystem;

        private bool _isGameEnded;
        
        public override void StartOnServer()
        {
            SpawnGameLobbySystem();
        }

        public void Start()
        {
            base.StartOnServer();

            SpawnShrinkingAreaSystem();
        }

        public override void FixedRun()
        {
            if (!_isSpawned && !_isGameEnded) return;
            
            base.FixedRun();
            
            var livingShips = CalculateLivingShips().ToArray();
            
            if (livingShips.Length == 1)
            {
                var ship = livingShips.FirstOrDefault();
                
                ship?.RPC_OpenWinPanel();
                _isGameEnded = true;
            }
        }

        private IEnumerable<SpaceshipController> CalculateLivingShips()
        {
            var room = RoomManagerBase.Instance.GetRoomOfScene(_manager.gameObject.scene);
            var connections = room.Connections;

            return from conn in connections
                select conn?.identity?.GetComponent<SpaceshipController>()
                into ship
                where ship != null
                where !ship.Health.IsDead
                select ship;
        }

        private void SpawnGameLobbySystem()
        {
            NetworkSpawnUtilities.SpawnObjectForScene(_manager.gameObject.scene, gameLobbySystem.gameObject);
        }
        
        private void SpawnShrinkingAreaSystem()
        {
            NetworkSpawnUtilities.SpawnObjectForScene(_manager.gameObject.scene, _shrinkingAreaSystem.gameObject);
        }
    }
}