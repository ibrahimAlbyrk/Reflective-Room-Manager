using TMPro;
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using REFLECTIVE.Runtime.Singleton;

namespace Examples.Basic.Game
{
    using Character;
    
    public class ScoreManager : RoomSingleton<ScoreManager>
    {
        [SerializeField] private TMP_Text _scoresText;
        
        private readonly SyncDictionary<int, int> _scores = new();

        private void Start() => _scores.Callback += ScoresOnChanged;

        private void ScoresOnChanged(SyncIDictionary<int, int>.Operation op, int key, int item) => UpdateText();

        public override void OnStartClient() => UpdateText();

        [ServerCallback]
        public void AddScore(int id, int score)
        {
            if (_scores.TryAdd(id, score))
            {
                return;
            }
            
            _scores[id] += score;
        }
        
        [ClientCallback]
        private void UpdateText()
        {
            if (_scoresText == null) return;
            
            _scoresText.text = string.Empty;
            
            foreach (var (ID, coin ) in _scores)
            {
                // If id equals current client
                if (ID == SimpleCharacterController.Local.ID)
                {
                    _scoresText.text += $"<color=green>Player {ID}, Coin: {coin:000}</color>";
                }
                else 
                    _scoresText.text += $"Player {ID}, Coin: {coin:000}";

                _scoresText.text += "\n";
            }
        }
    }
}