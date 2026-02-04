using System;
using Mirror;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace REFLECTIVE.Runtime.NETWORK.Party
{
    /// <summary>
    /// Represents a member within a party.
    /// Stores connection and member-specific state.
    /// </summary>
    [Serializable]
    public class PartyMember
    {
        /// <summary>
        /// Network connection for this party member.
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
        /// Whether the member is ready (for lobby/matchmaking).
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// When the member joined the party.
        /// </summary>
        public DateTime JoinedAt { get; }

        public PartyMember(NetworkConnection connection, string playerName = null)
        {
            Connection = connection;
            PlayerName = playerName ?? $"Player_{connection.GetConnectionId()}";
            IsReady = false;
            JoinedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the ready state for this member.
        /// </summary>
        public void SetReady(bool ready)
        {
            IsReady = ready;
        }

        /// <summary>
        /// Updates the player name.
        /// </summary>
        public void SetPlayerName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                PlayerName = name;
        }
    }
}
