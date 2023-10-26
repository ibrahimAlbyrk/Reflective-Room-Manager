using Mirror;
using UnityEngine;

namespace Example.Basic.Game
{
    public class ScoreManager : NetworkBehaviour
    {
        public static ScoreManager Instance;

        private readonly SyncDictionary<int, int> _scores = new();

        private void Awake()
        {
            Instance = this;
        }

        public void AddScore(int id, int score)
        {
            if (!_scores.ContainsKey(id))
            {
                _scores.Add(id, score);
            }
            else
                _scores[id] += score;
            
            Debug.Log($"Player {id}, Total score: {_scores[id]}");
        }
    }
}