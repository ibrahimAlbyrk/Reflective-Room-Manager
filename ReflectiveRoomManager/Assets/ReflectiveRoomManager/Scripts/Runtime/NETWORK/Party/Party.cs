using System;
using System.Linq;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace REFLECTIVE.Runtime.NETWORK.Party
{
    /// <summary>
    /// Represents a party (pre-room group of players).
    /// Parties exist outside of rooms and persist until disbanded.
    /// </summary>
    [Serializable]
    public class Party
    {
        #region Properties

        /// <summary>
        /// Unique identifier for this party.
        /// </summary>
        public uint ID { get; }

        /// <summary>
        /// Display name of the party.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Maximum number of members allowed.
        /// </summary>
        public int MaxSize { get; private set; }

        /// <summary>
        /// Current party leader's connection.
        /// </summary>
        public NetworkConnection Leader { get; private set; }

        /// <summary>
        /// Connection ID of the leader for serialization.
        /// </summary>
        public int LeaderConnectionId => Leader.GetConnectionId();

        /// <summary>
        /// All party members including the leader.
        /// </summary>
        public IReadOnlyList<PartyMember> Members => _members;

        /// <summary>
        /// All pending invitations.
        /// </summary>
        public IReadOnlyList<PartyInvite> PendingInvites => _pendingInvites;

        /// <summary>
        /// Party configuration settings.
        /// </summary>
        public PartySettings Settings { get; private set; }

        /// <summary>
        /// Whether the party is at maximum capacity.
        /// </summary>
        public bool IsFull => _members.Count >= MaxSize;

        /// <summary>
        /// Current number of members.
        /// </summary>
        public int MemberCount => _members.Count;

        #endregion

        #region Private Fields

        private readonly List<PartyMember> _members;
        private readonly List<PartyInvite> _pendingInvites;
        private Dictionary<string, string> _customData;
        private readonly int _defaultInviteTimeout;

        #endregion

        #region Constructor

        public Party(uint id, NetworkConnection leader, int maxSize, int defaultInviteTimeout, string name = null)
        {
            ID = id;
            Name = name ?? $"Party_{id}";
            MaxSize = maxSize;
            Leader = leader;
            Settings = new PartySettings();
            _members = new List<PartyMember>();
            _pendingInvites = new List<PartyInvite>();
            _customData = new Dictionary<string, string>();
            _defaultInviteTimeout = defaultInviteTimeout;

            // Add leader as first member
            AddMember(leader);
        }

        #endregion

        #region Member Management

        /// <summary>
        /// Adds a new member to the party.
        /// </summary>
        public bool AddMember(NetworkConnection conn, string playerName = null)
        {
            if (conn == null)
            {
                Debug.LogWarning("[Party] Cannot add null connection as member");
                return false;
            }

            if (IsMember(conn))
            {
                Debug.LogWarning($"[Party] Connection {conn.GetConnectionId()} is already a member");
                return false;
            }

            if (IsFull)
            {
                Debug.LogWarning($"[Party] Party {ID} is full");
                return false;
            }

            var member = new PartyMember(conn, playerName);
            _members.Add(member);
            return true;
        }

        /// <summary>
        /// Removes a member from the party.
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
        /// Checks if a connection is a member of this party.
        /// </summary>
        public bool IsMember(NetworkConnection conn)
        {
            return conn != null && _members.Any(m => m.Connection == conn);
        }

        /// <summary>
        /// Checks if a connection is the party leader.
        /// </summary>
        public bool IsLeader(NetworkConnection conn)
        {
            return conn != null && Leader == conn;
        }

        /// <summary>
        /// Gets a member by connection.
        /// </summary>
        public PartyMember GetMember(NetworkConnection conn)
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

        #region Leadership

        /// <summary>
        /// Transfers leadership to another member.
        /// </summary>
        public bool TransferLeadership(NetworkConnection newLeader)
        {
            if (newLeader == null)
            {
                Debug.LogWarning("[Party] Cannot transfer leadership to null connection");
                return false;
            }

            if (!IsMember(newLeader))
            {
                Debug.LogWarning($"[Party] Connection {newLeader.GetConnectionId()} is not a member");
                return false;
            }

            Leader = newLeader;
            return true;
        }

        /// <summary>
        /// Gets the oldest member (excluding the current leader) for auto-transfer.
        /// </summary>
        public NetworkConnection GetOldestMemberExcept(NetworkConnection exclude)
        {
            return _members
                .Where(m => m.Connection != exclude)
                .OrderBy(m => m.JoinedAt)
                .FirstOrDefault()?.Connection;
        }

        #endregion

        #region Invite Management

        /// <summary>
        /// Creates an invite for a target player.
        /// </summary>
        public PartyInvite CreateInvite(NetworkConnection inviter, NetworkConnection target)
        {
            if (inviter == null || target == null)
            {
                Debug.LogWarning("[Party] Cannot create invite with null connections");
                return null;
            }

            if (!IsMember(inviter))
            {
                Debug.LogWarning($"[Party] Inviter {inviter.GetConnectionId()} is not a party member");
                return null;
            }

            if (IsMember(target))
            {
                Debug.LogWarning($"[Party] Target {target.GetConnectionId()} is already a member");
                return null;
            }

            if (HasPendingInvite(target))
            {
                Debug.LogWarning($"[Party] Target {target.GetConnectionId()} already has a pending invite");
                return null;
            }

            if (IsFull)
            {
                Debug.LogWarning($"[Party] Party {ID} is full, cannot send invite");
                return null;
            }

            var timeout = Settings.InviteTimeoutSeconds > 0
                ? Settings.InviteTimeoutSeconds
                : _defaultInviteTimeout;

            var invite = new PartyInvite(inviter, target, timeout);
            _pendingInvites.Add(invite);
            return invite;
        }

        /// <summary>
        /// Accepts an invite and adds the target as a member.
        /// </summary>
        public bool AcceptInvite(NetworkConnection conn)
        {
            var invite = GetPendingInvite(conn);
            if (invite == null)
            {
                Debug.LogWarning($"[Party] No pending invite for connection {conn.GetConnectionId()}");
                return false;
            }

            if (invite.IsExpired)
            {
                _pendingInvites.Remove(invite);
                Debug.LogWarning($"[Party] Invite for {conn.GetConnectionId()} has expired");
                return false;
            }

            _pendingInvites.Remove(invite);

            if (IsFull)
            {
                Debug.LogWarning($"[Party] Party {ID} is full, cannot accept invite");
                return false;
            }

            return AddMember(conn);
        }

        /// <summary>
        /// Declines and removes an invite.
        /// </summary>
        public bool DeclineInvite(NetworkConnection conn)
        {
            var invite = GetPendingInvite(conn);
            if (invite == null) return false;

            _pendingInvites.Remove(invite);
            return true;
        }

        /// <summary>
        /// Checks if a connection has a pending invite.
        /// </summary>
        public bool HasPendingInvite(NetworkConnection conn)
        {
            return GetPendingInvite(conn) != null;
        }

        /// <summary>
        /// Gets a pending invite for a connection.
        /// </summary>
        public PartyInvite GetPendingInvite(NetworkConnection conn)
        {
            if (conn == null) return null;
            return _pendingInvites.FirstOrDefault(i => i.Target == conn && !i.IsExpired);
        }

        /// <summary>
        /// Removes all expired invites.
        /// </summary>
        public int CleanupExpiredInvites()
        {
            var expired = _pendingInvites.Where(i => i.IsExpired).ToList();
            foreach (var invite in expired)
                _pendingInvites.Remove(invite);
            return expired.Count;
        }

        /// <summary>
        /// Removes all invites from a specific inviter (when they leave).
        /// </summary>
        public void RemoveInvitesFrom(NetworkConnection inviter)
        {
            _pendingInvites.RemoveAll(i => i.Inviter == inviter);
        }

        #endregion

        #region Settings

        /// <summary>
        /// Updates party settings (leader only in practice).
        /// </summary>
        public void UpdateSettings(PartySettings settings)
        {
            Settings = new PartySettings(settings);
        }

        /// <summary>
        /// Updates the party name.
        /// </summary>
        public void SetName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;
        }

        /// <summary>
        /// Updates the maximum party size.
        /// </summary>
        public bool SetMaxSize(int maxSize)
        {
            if (maxSize < _members.Count)
            {
                Debug.LogWarning($"[Party] Cannot reduce max size below current member count");
                return false;
            }
            MaxSize = maxSize;
            return true;
        }

        #endregion

        #region Custom Data

        /// <summary>
        /// Gets a copy of all custom data.
        /// </summary>
        public Dictionary<string, string> GetCustomData()
        {
            return new Dictionary<string, string>(_customData);
        }

        /// <summary>
        /// Sets custom data, replacing existing data.
        /// </summary>
        public void SetCustomData(Dictionary<string, string> customData)
        {
            _customData = customData != null
                ? new Dictionary<string, string>(customData)
                : new Dictionary<string, string>();
        }

        /// <summary>
        /// Adds or updates a custom data entry.
        /// </summary>
        public void SetCustomDataValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            _customData[key] = value;
        }

        /// <summary>
        /// Gets a custom data value.
        /// </summary>
        public string GetCustomDataValue(string key)
        {
            return _customData.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Removes a custom data entry.
        /// </summary>
        public bool RemoveCustomData(string key)
        {
            return _customData.Remove(key);
        }

        #endregion
    }
}
