using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting
{
    /// <summary>
    /// Tracks vote type and per-player cooldowns.
    /// Prevents vote spam.
    /// </summary>
    public class VoteCooldownTracker
    {
        /// <summary>Global cooldowns per vote type</summary>
        private readonly Dictionary<string, float> _typeCooldowns;

        /// <summary>Per-player cooldowns per vote type</summary>
        private readonly Dictionary<NetworkConnection, Dictionary<string, float>> _playerCooldowns;

        // Cached lists for iteration without allocation
        private readonly List<string> _typeKeysCache;
        private readonly List<NetworkConnection> _playerKeysCache;
        private readonly List<string> _voteTypeKeysCache;

        public VoteCooldownTracker()
        {
            _typeCooldowns = new Dictionary<string, float>();
            _playerCooldowns = new Dictionary<NetworkConnection, Dictionary<string, float>>();
            _typeKeysCache = new List<string>();
            _playerKeysCache = new List<NetworkConnection>();
            _voteTypeKeysCache = new List<string>();
        }

        #region Cooldown Checks

        /// <summary>
        /// Checks if vote type is on cooldown for player.
        /// Checks both global and per-player cooldowns.
        /// </summary>
        public bool IsOnCooldown(string voteTypeID, NetworkConnection player)
        {
            // Check global cooldown
            if (_typeCooldowns.TryGetValue(voteTypeID, out var globalRemaining))
            {
                if (globalRemaining > 0f)
                    return true;
            }

            // Check per-player cooldown
            if (_playerCooldowns.TryGetValue(player, out var playerCooldowns))
            {
                if (playerCooldowns.TryGetValue(voteTypeID, out var playerRemaining))
                {
                    if (playerRemaining > 0f)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets remaining cooldown time for vote type (global).
        /// </summary>
        public float GetCooldownRemaining(string voteTypeID)
        {
            if (_typeCooldowns.TryGetValue(voteTypeID, out var remaining))
                return Mathf.Max(0f, remaining);
            return 0f;
        }

        /// <summary>
        /// Gets remaining cooldown time for player on vote type.
        /// </summary>
        public float GetPlayerCooldownRemaining(NetworkConnection player, string voteTypeID)
        {
            if (_playerCooldowns.TryGetValue(player, out var playerCooldowns))
            {
                if (playerCooldowns.TryGetValue(voteTypeID, out var remaining))
                    return Mathf.Max(0f, remaining);
            }
            return 0f;
        }

        #endregion

        #region Cooldown Management

        /// <summary>
        /// Starts global cooldown for vote type.
        /// </summary>
        public void StartCooldown(string voteTypeID, float duration, bool failed)
        {
            if (duration <= 0f) return;

            _typeCooldowns[voteTypeID] = duration;
        }

        /// <summary>
        /// Starts per-player cooldown for vote type.
        /// </summary>
        public void StartPlayerCooldown(NetworkConnection player, string voteTypeID, float duration)
        {
            if (duration <= 0f) return;

            if (!_playerCooldowns.TryGetValue(player, out var cooldowns))
            {
                cooldowns = new Dictionary<string, float>();
                _playerCooldowns[player] = cooldowns;
            }

            cooldowns[voteTypeID] = duration;
        }

        /// <summary>
        /// Updates all cooldowns (called every frame).
        /// </summary>
        public void Update(float deltaTime)
        {
            // Update global cooldowns
            _typeKeysCache.Clear();
            _typeKeysCache.AddRange(_typeCooldowns.Keys);

            foreach (var typeID in _typeKeysCache)
            {
                _typeCooldowns[typeID] -= deltaTime;
                if (_typeCooldowns[typeID] <= 0f)
                    _typeCooldowns.Remove(typeID);
            }

            // Update per-player cooldowns
            _playerKeysCache.Clear();
            _playerKeysCache.AddRange(_playerCooldowns.Keys);

            foreach (var player in _playerKeysCache)
            {
                var cooldowns = _playerCooldowns[player];

                _voteTypeKeysCache.Clear();
                _voteTypeKeysCache.AddRange(cooldowns.Keys);

                foreach (var typeID in _voteTypeKeysCache)
                {
                    cooldowns[typeID] -= deltaTime;
                    if (cooldowns[typeID] <= 0f)
                        cooldowns.Remove(typeID);
                }

                // Remove empty player entries
                if (cooldowns.Count == 0)
                    _playerCooldowns.Remove(player);
            }
        }

        /// <summary>
        /// Clears all cooldowns for a player (when they leave).
        /// </summary>
        public void ClearPlayerCooldowns(NetworkConnection player)
        {
            _playerCooldowns.Remove(player);
        }

        /// <summary>
        /// Clears all cooldowns (on room close).
        /// </summary>
        public void Clear()
        {
            _typeCooldowns.Clear();
            _playerCooldowns.Clear();
        }

        #endregion
    }
}
