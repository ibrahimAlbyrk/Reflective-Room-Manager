using System;
using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using REFLECTIVE.Runtime.Singleton;

namespace Examples.SpaceShooter.Game
{
    public class LeaderboardManager : RoomSingleton<LeaderboardManager>
    {
        public event Action OnLeaderboardUpdated;
        
        [SerializeField] private int maxRanking = 10;
        
        private List<PlayerScoreData> scoreEntries = new();

        public List<PlayerScoreData> GetLeaderboard() => scoreEntries;

        #region Message handlers

        #endregion

        #region Server methods

        [ServerCallback]
        public void AddPlayer(string username)
        {
            if (CheckEntry(username)) return;
            
            scoreEntries.Add(new PlayerScoreData{Username = username, Score =  0});
            
            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }
        
        [ServerCallback]
        public void RemovePlayer(string username)
        {
            if (!CheckEntry(username, out var data)) return;

            scoreEntries.Remove(data);
            
            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }

        [ServerCallback]
        public void AddScore(string username, int value)
        {
            if (!CheckEntry(username, out var data)) return;
            
            var index = scoreEntries.IndexOf(data);

            scoreEntries[index] = new PlayerScoreData
            {
                Username = username, Score = data.Score + value
            };

            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }

        [ServerCallback]
        public void SetScore(string username, int value)
        {
            if (!CheckEntry(username, out var data)) return;
            
            var index = scoreEntries.IndexOf(data);

            scoreEntries[index] = new PlayerScoreData
            {
                Username = username, Score = Mathf.Max(value, 0)
            };

            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }

        [ServerCallback]
        public void SubtractScore(string username, int value)
        {
            if (!CheckEntry(username, out var data)) return;

            var index = scoreEntries.IndexOf(data);

            scoreEntries[index] = new PlayerScoreData
            {
                Username = username, Score = Mathf.Max(data.Score - value, 0)
            };
            
            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }

        #endregion

        #region Utilities

        private bool CheckEntry(string username)
        {
            return CheckEntry(username, out _);
        }
        
        private bool CheckEntry(string username, out PlayerScoreData data)
        {
            data = scoreEntries.FirstOrDefault(entry => entry.Username == username);

            return !string.IsNullOrEmpty(data.Username);
        }

        private void OrderToScoreEntries()
        {
            scoreEntries = scoreEntries.OrderByDescending(data => data.Score).Take(maxRanking).ToList();
        }

        #endregion
    }
}