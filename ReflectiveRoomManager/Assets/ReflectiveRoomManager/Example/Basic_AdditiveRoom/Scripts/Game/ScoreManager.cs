using TMPro;
using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.Singleton;
using REFLECTIVE.Runtime.NETWORK.Room.Service;

namespace Example.Basic.Game
{
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
            if (!_scores.ContainsKey(id))
            {
                _scores.Add(id, score);
                return;
            }
            
            _scores[id] += score;
        }

        [ServerCallback]
        public void AddPlayer(int id)
        {
            if (_scores.ContainsKey(id)) return;

            _scores[id] = 0;
        }

        private void UpdateText()
        {
            _scoresText.text = string.Empty;
            
            foreach (var (ID, coin ) in _scores)
            {
                // If id equals current client
                if (ID == RoomClient.ID)
                {
                    _scoresText.text += $"<color=yellow>Player {ID}, Coin: {coin:0000}</color>";
                }
                else 
                    _scoresText.text += $"Player {ID}, Coin: {coin:0000}";

                _scoresText.text += "\n";
            }
        }
    }
}