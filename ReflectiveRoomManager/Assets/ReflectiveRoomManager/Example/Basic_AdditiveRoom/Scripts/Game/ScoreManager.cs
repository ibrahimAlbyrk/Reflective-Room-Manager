using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.Singleton;

namespace Example.Basic.Game
{
    public class ScoreManager : RoomSingleton<ScoreManager>
    {
        private readonly SyncDictionary<int, int> _scores = new();

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