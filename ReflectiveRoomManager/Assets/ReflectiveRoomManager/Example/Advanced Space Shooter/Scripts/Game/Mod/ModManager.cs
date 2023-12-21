using Mirror;
using UnityEngine;

namespace Examples.SpaceShooter.Game
{
    using Mod;
    using Data;
    
    [RequireComponent(typeof(NetworkIdentity))]
    public class ModManager : NetworkBehaviour
    {
        [SerializeField] private OpenWorldMod _openWorldMod;
        [SerializeField] private ShrinkingAreaMod _shrinkingAreaMod;
        
        private GameMod _selectedMod;

        public GameMod GetMod() => _selectedMod;
        
        public MapGeneratorData GetMapData() => _selectedMod?.GetMapData();
        
        public void Init(ModType _modType)
        {
            SetMod(_modType);
        }
        
        private void SetMod(ModType _modType)
        {
            _selectedMod = _modType switch
            {
                ModType.OpenWorld => _openWorldMod,
                ModType.ShrinkingArea => _shrinkingAreaMod,
                _ => _selectedMod
            };
            _selectedMod.Init(this);
        }
        
        [ServerCallback]
        public void StartGameModOnServer()
        {
            _selectedMod?.StartOnServer();
        }
        
        [ServerCallback]
        public void RunGameMod()
        {
            _selectedMod?.Run();
        }
        
        [ServerCallback]
        public void FixedRunGameMod()
        {
            _selectedMod?.FixedRun();
        }
    }
}