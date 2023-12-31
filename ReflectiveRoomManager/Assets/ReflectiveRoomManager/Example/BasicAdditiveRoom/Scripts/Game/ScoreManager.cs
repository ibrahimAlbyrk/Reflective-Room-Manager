﻿using TMPro;
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using REFLECTIVE.Runtime.Singleton;
using REFLECTIVE.Runtime.NETWORK.Room.Service;

namespace Examples.Basic.Game
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
            if (_scores.TryAdd(id, score))
            {
                return;
            }
            
            _scores[id] += score;
        }
        
        private void UpdateText()
        {
            _scoresText.text = string.Empty;
            
            foreach (var (ID, coin ) in _scores)
            {
                // If id equals current client
                if (ID == RoomClient.CurrentRoomID)
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