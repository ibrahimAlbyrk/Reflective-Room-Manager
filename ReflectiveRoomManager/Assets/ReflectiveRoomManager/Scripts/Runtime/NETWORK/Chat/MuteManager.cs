using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Chat
{
    /// <summary>
    /// Manages player mute status with support for temporary and permanent mutes.
    /// </summary>
    public class MuteManager
    {
        private readonly Dictionary<uint, MuteEntry> _mutedPlayers = new();

        /// <summary>
        /// Mutes a player for a specified duration.
        /// </summary>
        /// <param name="connectionID">Player's connection ID</param>
        /// <param name="durationSeconds">Duration in seconds (0 or negative for permanent)</param>
        /// <param name="reason">Reason for the mute</param>
        public void Mute(uint connectionID, float durationSeconds, string reason)
        {
            var entry = new MuteEntry
            {
                ExpireTime = durationSeconds > 0 ? Time.unscaledTime + durationSeconds : float.MaxValue,
                Reason = reason ?? string.Empty,
                IsPermanent = durationSeconds <= 0
            };

            _mutedPlayers[connectionID] = entry;

            var durationText = entry.IsPermanent ? "Permanent" : $"{durationSeconds}s";
            Debug.Log($"[ChatManager] Player {connectionID} muted. Reason: {reason}, Duration: {durationText}");
        }

        /// <summary>
        /// Unmutes a player.
        /// </summary>
        /// <param name="connectionID">Player's connection ID</param>
        public void Unmute(uint connectionID)
        {
            if (_mutedPlayers.Remove(connectionID))
                Debug.Log($"[ChatManager] Player {connectionID} unmuted.");
        }

        /// <summary>
        /// Checks if a player is currently muted.
        /// Automatically removes expired mutes.
        /// </summary>
        /// <param name="connectionID">Player's connection ID</param>
        /// <returns>True if player is muted</returns>
        public bool IsMuted(uint connectionID)
        {
            if (!_mutedPlayers.TryGetValue(connectionID, out var entry))
                return false;

            if (entry.IsPermanent)
                return true;

            if (Time.unscaledTime >= entry.ExpireTime)
            {
                _mutedPlayers.Remove(connectionID);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets detailed mute information for a player.
        /// </summary>
        /// <param name="connectionID">Player's connection ID</param>
        /// <returns>Mute information or default if not muted</returns>
        public MuteInfo GetMuteInfo(uint connectionID)
        {
            if (!_mutedPlayers.TryGetValue(connectionID, out var entry))
                return default;

            var remainingTime = entry.IsPermanent ? -1f : Mathf.Max(0f, entry.ExpireTime - Time.unscaledTime);

            return new MuteInfo
            {
                IsMuted = true,
                Reason = entry.Reason,
                IsPermanent = entry.IsPermanent,
                RemainingSeconds = remainingTime
            };
        }

        /// <summary>
        /// Removes mute tracking for a disconnected player.
        /// </summary>
        /// <param name="connectionID">Player's connection ID</param>
        public void RemovePlayer(uint connectionID)
        {
            _mutedPlayers.Remove(connectionID);
        }

        /// <summary>
        /// Clears all mute data.
        /// </summary>
        public void Clear()
        {
            _mutedPlayers.Clear();
        }

        private struct MuteEntry
        {
            public float ExpireTime;
            public string Reason;
            public bool IsPermanent;
        }
    }

    /// <summary>
    /// Public mute information struct.
    /// </summary>
    public struct MuteInfo
    {
        public bool IsMuted;
        public string Reason;
        public bool IsPermanent;
        public float RemainingSeconds;
    }
}
