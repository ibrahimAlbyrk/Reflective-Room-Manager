using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.Singleton;
using REFLECTIVE.Runtime.NETWORK.Room;

namespace Examples.SpaceShooter.Game
{
    using Mod;
    using Data;

    public class GameManager : RoomSingleton<GameManager>
    {
        [Header("Mod Settings")]
        [SerializeField] private ModManager _modManager;
        
        private ModType _modType;

        public GameMod GetMod() => _modManager.GetMod();
        
        public ModType GetModType() => _modType;
        
        public MapGeneratorData GetData() => _modManager?.GetMapData(); 
        
        [ServerCallback]
        private void Start()
        {
            var room = RoomManagerBase.Instance.GetRoomOfScene(gameObject.scene);

            _modType = room.IsServer ? ModType.OpenWorld : ModType.ShrinkingArea;
            
            _modManager?.Init(_modType);
            
            _modManager?.StartGameModOnServer();
        }

        [ServerCallback]
        private void Update()
        {
            _modManager.RunGameMod();
        }
        
        [ServerCallback]
        private void FixedUpdate()
        {
            _modManager.FixedRunGameMod();
        }
    }

    public enum ModType
    {
        OpenWorld,
        ShrinkingArea
    }
}