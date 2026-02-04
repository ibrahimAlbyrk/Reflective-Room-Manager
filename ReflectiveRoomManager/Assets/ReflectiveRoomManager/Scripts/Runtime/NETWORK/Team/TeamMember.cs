using System;
using Mirror;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace REFLECTIVE.Runtime.NETWORK.Team
{
    /// <summary>
    /// Represents a player within a team.
    /// Stores connection and player-specific statistics.
    /// </summary>
    [Serializable]
    public class TeamMember
    {
        /// <summary>
        /// Network connection for this team member.
        /// </summary>
        public NetworkConnection Connection { get; }

        /// <summary>
        /// Connection ID for network serialization.
        /// </summary>
        public int ConnectionId => Connection.GetConnectionId();

        /// <summary>
        /// Display name of the player.
        /// </summary>
        public string PlayerName { get; private set; }

        /// <summary>
        /// When the player joined this team.
        /// </summary>
        public DateTime JoinedAt { get; }

        /// <summary>
        /// Number of kills by this player.
        /// </summary>
        public int Kills { get; private set; }

        /// <summary>
        /// Number of deaths for this player.
        /// </summary>
        public int Deaths { get; private set; }

        /// <summary>
        /// Number of assists by this player.
        /// </summary>
        public int Assists { get; private set; }

        /// <summary>
        /// Individual score for this player.
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Kill/Death ratio for this player.
        /// </summary>
        public float KDRatio => Deaths > 0 ? (float)Kills / Deaths : Kills;

        public TeamMember(NetworkConnection connection, string playerName = null)
        {
            Connection = connection;
            PlayerName = playerName ?? $"Player_{connection.GetConnectionId()}";
            JoinedAt = DateTime.UtcNow;
            Kills = 0;
            Deaths = 0;
            Assists = 0;
            Score = 0;
        }

        /// <summary>
        /// Updates the player name.
        /// </summary>
        public void SetPlayerName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                PlayerName = name;
        }

        /// <summary>
        /// Adds a kill to this player's stats.
        /// </summary>
        public void AddKill()
        {
            Kills++;
        }

        /// <summary>
        /// Adds a death to this player's stats.
        /// </summary>
        public void AddDeath()
        {
            Deaths++;
        }

        /// <summary>
        /// Adds an assist to this player's stats.
        /// </summary>
        public void AddAssist()
        {
            Assists++;
        }

        /// <summary>
        /// Adds points to this player's score.
        /// </summary>
        public void AddScore(int points)
        {
            Score += points;
        }

        /// <summary>
        /// Sets the player's score directly.
        /// </summary>
        public void SetScore(int score)
        {
            Score = score;
        }

        /// <summary>
        /// Resets all player statistics.
        /// </summary>
        public void ResetStats()
        {
            Kills = 0;
            Deaths = 0;
            Assists = 0;
            Score = 0;
        }
    }
}
