using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using REFLECTIVE.Runtime.Extensions;

namespace Examples.SpaceShooter.Player
{
    using Game;
    
    [RequireComponent(typeof(NetworkIdentity))]
    public class Player_Leaderboard_UI : NetworkBehaviour
    {
        [SerializeField] private GameObject _leaderboardFieldPrefab;

        [SerializeField] private Transform _leaderboardContent;
        
        private readonly Dictionary<string, PlayerLeaderboardField> _leaderboard = new();

        [Command]
        private void CMD_RequestLeaderboard()
        {
            var _leaderboardManager = gameObject.RoomContainer().GetSingleton<LeaderboardManager>();
            
            T_RPC_ReceiveLeaderboard(_leaderboardManager.GetLeaderboard());
        }

        [TargetRpc]
        private void T_RPC_ReceiveLeaderboard(List<PlayerScoreData> leaderboard)
        {
            RenderLeaderboard(leaderboard);
        }

        private void RenderLeaderboard(List<PlayerScoreData> leaderboard)
        {
            foreach (var username in _leaderboard.Keys.ToArray())
            {
                var field = _leaderboard[username];

                Destroy(field.gameObject);
                
                _leaderboard.Remove(username);
            }

            foreach (var data in leaderboard)
            {
                var field = Instantiate(_leaderboardFieldPrefab, _leaderboardContent).GetComponent<PlayerLeaderboardField>();
                field.Init(data.Username, data.Score);
                
                _leaderboard.Add(data.Username, field);
            }
        }

        private void OpenLeaderboard()
        {
            CMD_RequestLeaderboard();
            
            _leaderboardContent.gameObject.SetActive(true);
        }

        private void CloseLeaderboard()
        {
            _leaderboardContent.gameObject.SetActive(false);
        }

        private void UpdateLeaderboard()
        {
            if (!_leaderboardContent.gameObject.activeSelf) return;
            
            CMD_RequestLeaderboard();
        }
        
        [ClientCallback]
        private void OnDestroy()
        {
            var _leaderboardManager = gameObject.RoomContainer().GetSingleton<LeaderboardManager>();
            
            if (_leaderboardManager != null)
                _leaderboardManager.OnLeaderboardUpdated -= UpdateLeaderboard;
        }

        [ClientCallback]
        private void Start()
        {
            CloseLeaderboard();
            
            var _leaderboardManager = gameObject.RoomContainer().GetSingleton<LeaderboardManager>();
            
            if (_leaderboardManager != null)
                _leaderboardManager.OnLeaderboardUpdated += UpdateLeaderboard;
        }

        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;
            
            if(Input.GetKeyDown(KeyCode.Tab))
                OpenLeaderboard();
            
            if(Input.GetKeyUp(KeyCode.Tab))
                CloseLeaderboard();
        }
    }
}