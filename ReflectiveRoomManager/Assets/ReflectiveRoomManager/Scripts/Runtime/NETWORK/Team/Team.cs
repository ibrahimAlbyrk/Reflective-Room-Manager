using System;
using System.Linq;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace REFLECTIVE.Runtime.NETWORK.Team
{
    /// <summary>
    /// Represents a team within a room.
    /// Teams exist only within the context of a room.
    /// </summary>
    [Serializable]
    public class Team
    {
        #region Properties

        /// <summary>
        /// Unique identifier for this team within the room.
        /// </summary>
        public uint ID { get; }

        /// <summary>
        /// Display name of the team.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Team color for UI display.
        /// </summary>
        public Color TeamColor { get; private set; }

        /// <summary>
        /// Maximum number of members allowed.
        /// </summary>
        public int MaxSize { get; private set; }

        /// <summary>
        /// All team members.
        /// </summary>
        public IReadOnlyList<TeamMember> Members => _members;

        /// <summary>
        /// Aggregated team statistics.
        /// </summary>
        public TeamStats Stats { get; }

        /// <summary>
        /// Whether the team is at maximum capacity.
        /// </summary>
        public bool IsFull => _members.Count >= MaxSize;

        /// <summary>
        /// Current number of members.
        /// </summary>
        public int MemberCount => _members.Count;

        /// <summary>
        /// Available slots on this team.
        /// </summary>
        public int AvailableSlots => MaxSize - _members.Count;

        #endregion

        #region Private Fields

        private readonly List<TeamMember> _members;

        #endregion

        #region Constructor

        public Team(uint id, string name, Color color, int maxSize)
        {
            ID = id;
            Name = name;
            TeamColor = color;
            MaxSize = maxSize;
            Stats = new TeamStats();
            _members = new List<TeamMember>();
        }

        #endregion

        #region Member Management

        /// <summary>
        /// Adds a new member to the team.
        /// </summary>
        public bool AddMember(NetworkConnection conn, string playerName = null)
        {
            if (conn == null)
            {
                Debug.LogWarning("[Team] Cannot add null connection as member");
                return false;
            }

            if (IsMember(conn))
            {
                Debug.LogWarning($"[Team] Connection {conn.GetConnectionId()} is already on team {ID}");
                return false;
            }

            if (IsFull)
            {
                Debug.LogWarning($"[Team] Team {ID} ({Name}) is full");
                return false;
            }

            var member = new TeamMember(conn, playerName);
            _members.Add(member);
            return true;
        }

        /// <summary>
        /// Removes a member from the team.
        /// </summary>
        public bool RemoveMember(NetworkConnection conn)
        {
            if (conn == null) return false;

            var member = _members.FirstOrDefault(m => m.Connection == conn);
            if (member == null) return false;

            _members.Remove(member);
            return true;
        }

        /// <summary>
        /// Checks if a connection is a member of this team.
        /// </summary>
        public bool IsMember(NetworkConnection conn)
        {
            return conn != null && _members.Any(m => m.Connection == conn);
        }

        /// <summary>
        /// Gets a member by connection.
        /// </summary>
        public TeamMember GetMember(NetworkConnection conn)
        {
            return _members.FirstOrDefault(m => m.Connection == conn);
        }

        /// <summary>
        /// Gets all member connections.
        /// </summary>
        public IEnumerable<NetworkConnection> GetMemberConnections()
        {
            return _members.Select(m => m.Connection);
        }

        #endregion

        #region Stats Management

        /// <summary>
        /// Adds points to the team score.
        /// </summary>
        public void AddScore(int points)
        {
            Stats.AddScore(points);
        }

        /// <summary>
        /// Adds a kill to team and player stats.
        /// </summary>
        public void AddKill(NetworkConnection killer)
        {
            Stats.AddKill();
            var member = GetMember(killer);
            member?.AddKill();
        }

        /// <summary>
        /// Adds a death to team and player stats.
        /// </summary>
        public void AddDeath(NetworkConnection deceased)
        {
            Stats.AddDeath();
            var member = GetMember(deceased);
            member?.AddDeath();
        }

        /// <summary>
        /// Adds an assist to team and player stats.
        /// </summary>
        public void AddAssist(NetworkConnection assister)
        {
            Stats.AddAssist();
            var member = GetMember(assister);
            member?.AddAssist();
        }

        /// <summary>
        /// Resets all team and member statistics.
        /// </summary>
        public void ResetStats()
        {
            Stats.Reset();
            foreach (var member in _members)
                member.ResetStats();
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Updates the team name.
        /// </summary>
        public void SetName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;
        }

        /// <summary>
        /// Updates the team color.
        /// </summary>
        public void SetColor(Color color)
        {
            TeamColor = color;
        }

        /// <summary>
        /// Updates the maximum team size.
        /// </summary>
        public bool SetMaxSize(int maxSize)
        {
            if (maxSize < _members.Count)
            {
                Debug.LogWarning($"[Team] Cannot reduce max size below current member count");
                return false;
            }
            MaxSize = maxSize;
            return true;
        }

        #endregion
    }
}
