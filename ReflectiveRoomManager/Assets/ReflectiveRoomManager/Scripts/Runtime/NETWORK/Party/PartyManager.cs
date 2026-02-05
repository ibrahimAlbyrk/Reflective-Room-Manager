using System.Linq;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Party
{
    using Config;
    using Events;
    using Messages;
    using Validation;
    using Utilities;
    using REFLECTIVE.Runtime.Identifier;

    /// <summary>
    /// Manages all party operations on the server.
    /// Parties are global (pre-room) entities that persist until disbanded.
    /// </summary>
    public class PartyManager
    {
        #region Properties

        /// <summary>
        /// Party event manager for subscribing to party events.
        /// </summary>
        public PartyEventManager EventManager { get; }

        /// <summary>
        /// Party validator for custom validation logic.
        /// </summary>
        public IPartyValidator Validator { get; set; }

        /// <summary>
        /// Current party configuration.
        /// </summary>
        public PartyConfig Config { get; }

        /// <summary>
        /// All active parties.
        /// </summary>
        public IReadOnlyCollection<Party> Parties => _parties.Values;

        #endregion

        #region Private Fields

        private readonly Dictionary<uint, Party> _parties;
        private readonly Dictionary<NetworkConnection, uint> _connToPartyMap;
        private readonly UniqueIdentifier _idGenerator;
        private bool _handlersRegistered;

        #endregion

        #region Constructor

        public PartyManager(PartyConfig config)
        {
            Config = config;
            EventManager = new PartyEventManager();
            Validator = new DefaultPartyValidator();

            _parties = new Dictionary<uint, Party>();
            _connToPartyMap = new Dictionary<NetworkConnection, uint>();
            _idGenerator = new UniqueIdentifier();
        }

        #endregion

        #region Network Handler Registration

#if REFLECTIVE_SERVER
        /// <summary>
        /// Registers network message handlers for the server.
        /// </summary>
        public void RegisterServerHandlers()
        {
            if (_handlersRegistered) return;

            NetworkServer.RegisterHandler<PartyCreateMessage>(OnServerPartyCreate);
            NetworkServer.RegisterHandler<PartyInviteMessage>(OnServerPartyInvite);
            NetworkServer.RegisterHandler<PartyInviteResponseMessage>(OnServerInviteResponse);
            NetworkServer.RegisterHandler<PartyLeaveMessage>(OnServerPartyLeave);
            NetworkServer.RegisterHandler<PartyKickMessage>(OnServerPartyKick);

            _handlersRegistered = true;

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log("[PartyManager] Server handlers registered");
        }

        /// <summary>
        /// Unregisters network message handlers.
        /// </summary>
        public void UnregisterServerHandlers()
        {
            if (!_handlersRegistered) return;

            NetworkServer.UnregisterHandler<PartyCreateMessage>();
            NetworkServer.UnregisterHandler<PartyInviteMessage>();
            NetworkServer.UnregisterHandler<PartyInviteResponseMessage>();
            NetworkServer.UnregisterHandler<PartyLeaveMessage>();
            NetworkServer.UnregisterHandler<PartyKickMessage>();

            _handlersRegistered = false;

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log("[PartyManager] Server handlers unregistered");
        }
#endif

#if REFLECTIVE_CLIENT
        /// <summary>
        /// Registers client-side message handlers.
        /// </summary>
        public static void RegisterClientHandlers(PartyEventManager eventManager)
        {
            NetworkClient.RegisterHandler<PartySyncMessage>(msg => eventManager.Invoke_OnClientPartySync(msg));
            NetworkClient.RegisterHandler<PartyLeaderChangeMessage>(msg =>
                eventManager.Invoke_OnClientLeaderChanged(msg.PartyID, msg.NewLeaderConnectionID));
            NetworkClient.RegisterHandler<PartyInviteNotificationMessage>(msg =>
                eventManager.Invoke_OnClientInviteReceived(msg.PartyID, msg.InviterName));
        }

        /// <summary>
        /// Unregisters client-side message handlers.
        /// </summary>
        public static void UnregisterClientHandlers()
        {
            NetworkClient.UnregisterHandler<PartySyncMessage>();
            NetworkClient.UnregisterHandler<PartyLeaderChangeMessage>();
            NetworkClient.UnregisterHandler<PartyInviteNotificationMessage>();
        }
#endif

        #endregion

        #region Party Operations

        /// <summary>
        /// Creates a new party with the specified leader.
        /// </summary>
        public Party CreateParty(NetworkConnection leader, int maxSize, string partyName = null)
        {
            if (leader == null)
            {
                Debug.LogWarning("[PartyManager] Cannot create party with null leader");
                return null;
            }

            // Check if already in a party
            if (_connToPartyMap.ContainsKey(leader))
            {
                Debug.LogWarning($"[PartyManager] Connection {leader.GetConnectionId()} is already in a party");
                return null;
            }

            // Validate
            var clampedSize = Mathf.Clamp(maxSize, 2, Config?.MaxPartySize ?? 10);
            var name = string.IsNullOrEmpty(partyName) ? null : partyName;

            if (Validator != null && !Validator.CanCreateParty(leader, name ?? "Party", clampedSize, out var reason))
            {
                Debug.LogWarning($"[PartyManager] Party creation denied: {reason}");
                return null;
            }

            var id = _idGenerator.CreateID();
            var defaultTimeout = Config?.InviteTimeoutSeconds ?? 60;
            var party = new Party(id, leader, clampedSize, defaultTimeout, name);

            _parties[id] = party;
            _connToPartyMap[leader] = id;

            EventManager.Invoke_OnPartyCreated(party);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Party {id} created by {leader.GetConnectionId()}");

            // Sync to leader
            SendPartySyncToMembers(party);

            return party;
        }

        /// <summary>
        /// Disbands a party and removes all members.
        /// </summary>
        public bool DisbandParty(uint partyID)
        {
            if (!_parties.TryGetValue(partyID, out var party))
            {
                Debug.LogWarning($"[PartyManager] Party {partyID} not found");
                return false;
            }

            // Remove all members from mapping
            foreach (var member in party.Members)
            {
                if (member.Connection != null)
                    _connToPartyMap.Remove(member.Connection);
            }

            _parties.Remove(partyID);

            EventManager.Invoke_OnPartyDisbanded(party);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Party {partyID} disbanded");

            return true;
        }

        /// <summary>
        /// Sends an invite to a player.
        /// </summary>
        public bool InvitePlayer(uint partyID, NetworkConnection inviter, NetworkConnection target)
        {
            if (!_parties.TryGetValue(partyID, out var party))
            {
                Debug.LogWarning($"[PartyManager] Party {partyID} not found");
                return false;
            }

            if (Validator != null && !Validator.CanInviteToParty(inviter, target, party, out var reason))
            {
                Debug.LogWarning($"[PartyManager] Invite denied: {reason}");
                return false;
            }

            // Check if target is already in another party
            if (_connToPartyMap.ContainsKey(target))
            {
                Debug.LogWarning($"[PartyManager] Target {target.GetConnectionId()} is already in a party");
                return false;
            }

            var invite = party.CreateInvite(inviter, target);
            if (invite == null) return false;

            EventManager.Invoke_OnInviteSent(party, inviter, target);

            // Send notification to target
            var inviterMember = party.GetMember(inviter);
            var notification = new PartyInviteNotificationMessage(
                partyID,
                inviterMember?.PlayerName ?? "Unknown",
                party.Name,
                (float)invite.TimeRemaining.TotalSeconds
            );
            target.Send(notification);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Invite sent from {inviter.GetConnectionId()} to {target.GetConnectionId()} for party {partyID}");

            return true;
        }

        /// <summary>
        /// Accepts an invite and adds the player to the party.
        /// </summary>
        public bool AcceptInvite(NetworkConnection conn, uint partyID)
        {
            if (!_parties.TryGetValue(partyID, out var party))
            {
                Debug.LogWarning($"[PartyManager] Party {partyID} not found");
                return false;
            }

            // Check if already in another party
            if (_connToPartyMap.TryGetValue(conn, out var existingPartyID) && existingPartyID != partyID)
            {
                Debug.LogWarning($"[PartyManager] Connection {conn.GetConnectionId()} is already in party {existingPartyID}");
                return false;
            }

            if (!party.AcceptInvite(conn))
                return false;

            _connToPartyMap[conn] = partyID;

            EventManager.Invoke_OnInviteResponse(party, conn, true);
            EventManager.Invoke_OnMemberJoined(party, conn);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Connection {conn.GetConnectionId()} joined party {partyID}");

            // Sync to all members
            SendPartySyncToMembers(party);

            return true;
        }

        /// <summary>
        /// Declines an invite.
        /// </summary>
        public bool DeclineInvite(NetworkConnection conn, uint partyID)
        {
            if (!_parties.TryGetValue(partyID, out var party))
            {
                Debug.LogWarning($"[PartyManager] Party {partyID} not found");
                return false;
            }

            if (!party.DeclineInvite(conn))
                return false;

            EventManager.Invoke_OnInviteResponse(party, conn, false);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Connection {conn.GetConnectionId()} declined invite to party {partyID}");

            return true;
        }

        /// <summary>
        /// Removes a player from their party.
        /// </summary>
        public bool LeaveParty(NetworkConnection conn)
        {
            if (!_connToPartyMap.TryGetValue(conn, out var partyID))
            {
                Debug.LogWarning($"[PartyManager] Connection {conn.GetConnectionId()} is not in a party");
                return false;
            }

            if (!_parties.TryGetValue(partyID, out var party))
            {
                _connToPartyMap.Remove(conn);
                return false;
            }

            var wasLeader = party.IsLeader(conn);
            var oldLeader = party.Leader;

            party.RemoveMember(conn);
            party.RemoveInvitesFrom(conn);
            _connToPartyMap.Remove(conn);

            EventManager.Invoke_OnMemberLeft(party, conn);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Connection {conn.GetConnectionId()} left party {partyID}");

            // Check if party should disband
            if (party.MemberCount == 0)
            {
                DisbandParty(partyID);
                return true;
            }

            // Transfer leadership if needed
            if (wasLeader && Config != null && Config.EnableAutoLeaderTransfer)
            {
                TransferLeadership(party, oldLeader, LeaderChangeReason.LeaderLeft);
            }

            // Sync to remaining members
            SendPartySyncToMembers(party);

            return true;
        }

        /// <summary>
        /// Kicks a member from the party (leader only).
        /// </summary>
        public bool KickMember(uint partyID, NetworkConnection kicker, NetworkConnection target)
        {
            if (!_parties.TryGetValue(partyID, out var party))
            {
                Debug.LogWarning($"[PartyManager] Party {partyID} not found");
                return false;
            }

            if (Validator != null && !Validator.CanKickFromParty(kicker, target, party, out var reason))
            {
                Debug.LogWarning($"[PartyManager] Kick denied: {reason}");
                return false;
            }

            party.RemoveMember(target);
            _connToPartyMap.Remove(target);

            EventManager.Invoke_OnMemberKicked(party, target);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Connection {target.GetConnectionId()} kicked from party {partyID}");

            // Notify kicked player
            EventManager.Invoke_OnClientPartyKicked(partyID);

            // Sync to remaining members
            SendPartySyncToMembers(party);

            return true;
        }

        /// <summary>
        /// Handles a player disconnecting from the server.
        /// </summary>
        public void HandleDisconnect(NetworkConnection conn)
        {
            if (!_connToPartyMap.TryGetValue(conn, out var partyID))
                return;

            if (!_parties.TryGetValue(partyID, out var party))
            {
                _connToPartyMap.Remove(conn);
                return;
            }

            var wasLeader = party.IsLeader(conn);
            var oldLeader = party.Leader;

            party.RemoveMember(conn);
            party.RemoveInvitesFrom(conn);
            _connToPartyMap.Remove(conn);

            EventManager.Invoke_OnMemberLeft(party, conn);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Connection {conn.GetConnectionId()} disconnected from party {partyID}");

            // Check if party should disband
            if (party.MemberCount == 0)
            {
                DisbandParty(partyID);
                return;
            }

            // Transfer leadership if needed
            if (wasLeader && Config != null && Config.EnableAutoLeaderTransfer)
            {
                TransferLeadership(party, oldLeader, LeaderChangeReason.LeaderDisconnected);
            }

            // Sync to remaining members
            SendPartySyncToMembers(party);
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Gets a party by ID.
        /// </summary>
        public Party GetParty(uint partyID)
        {
            return _parties.TryGetValue(partyID, out var party) ? party : null;
        }

        /// <summary>
        /// Gets the party a connection belongs to.
        /// </summary>
        public Party GetPartyByMember(NetworkConnection conn)
        {
            if (conn == null) return null;
            if (!_connToPartyMap.TryGetValue(conn, out var partyID)) return null;
            return _parties.TryGetValue(partyID, out var party) ? party : null;
        }

        /// <summary>
        /// Gets the party ID for a connection.
        /// </summary>
        public uint? GetPartyIDByMember(NetworkConnection conn)
        {
            if (conn == null) return null;
            return _connToPartyMap.TryGetValue(conn, out var partyID) ? partyID : null;
        }

        /// <summary>
        /// Gets all parties.
        /// </summary>
        public IEnumerable<Party> GetAllParties()
        {
            return _parties.Values;
        }

        /// <summary>
        /// Checks if a connection is in any party.
        /// </summary>
        public bool IsInParty(NetworkConnection conn)
        {
            return conn != null && _connToPartyMap.ContainsKey(conn);
        }

        #endregion

        #region Private Methods

        private void TransferLeadership(Party party, NetworkConnection oldLeader, LeaderChangeReason reason)
        {
            var newLeader = party.GetOldestMemberExcept(oldLeader);
            if (newLeader == null)
            {
                Debug.LogWarning($"[PartyManager] No eligible member for leadership transfer in party {party.ID}");
                return;
            }

            party.TransferLeadership(newLeader);

            EventManager.Invoke_OnLeaderChanged(party, oldLeader, newLeader);

            // Notify all members
            var msg = new PartyLeaderChangeMessage(party.ID, newLeader.GetConnectionId(), reason);
            foreach (var member in party.Members)
            {
                member.Connection?.Send(msg);
            }

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[PartyManager] Leadership transferred to {newLeader.GetConnectionId()} in party {party.ID} ({reason})");
        }

        private void SendPartySyncToMembers(Party party)
        {
            var msg = CreatePartySyncMessage(party);
            foreach (var member in party.Members)
            {
                member.Connection?.Send(msg);
            }
        }

        private PartySyncMessage CreatePartySyncMessage(Party party)
        {
            var memberData = party.Members.Select(m => new PartyMemberData(
                m.ConnectionId,
                m.PlayerName,
                m.IsReady
            )).ToArray();

            return new PartySyncMessage(
                party.ID,
                party.Name,
                party.MaxSize,
                party.LeaderConnectionId,
                memberData,
                party.Settings.IsPublic,
                party.Settings.AutoAcceptFriends,
                party.Settings.AllowVoiceChat
            );
        }

        /// <summary>
        /// Cleans up expired invites across all parties.
        /// Call this periodically.
        /// </summary>
        public void CleanupExpiredInvites()
        {
            foreach (var party in _parties.Values)
            {
                party.CleanupExpiredInvites();
            }
        }

        /// <summary>
        /// Clears all parties (for server shutdown).
        /// </summary>
        public void ClearAll()
        {
            _parties.Clear();
            _connToPartyMap.Clear();

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log("[PartyManager] All parties cleared");
        }

        #endregion

        #region Network Message Handlers

        private void OnServerPartyCreate(NetworkConnectionToClient conn, PartyCreateMessage msg)
        {
            var maxSize = msg.MaxSize > 0 ? msg.MaxSize : Config?.DefaultMaxSize ?? 4;
            var party = CreateParty(conn, maxSize, msg.PartyName);

            if (party != null)
            {
                party.Settings.IsPublic = msg.IsPublic;
                party.Settings.AutoAcceptFriends = msg.AutoAcceptFriends;
                party.Settings.AllowVoiceChat = msg.AllowVoiceChat;

                EventManager.Invoke_OnClientPartyCreated(party.ID);
            }
        }

        private void OnServerPartyInvite(NetworkConnectionToClient conn, PartyInviteMessage msg)
        {
            var target = NetworkServer.connections.Values
                .FirstOrDefault(c => c.GetConnectionId() == msg.TargetConnectionID);

            if (target == null)
            {
                Debug.LogWarning($"[PartyManager] Target connection {msg.TargetConnectionID} not found");
                return;
            }

            InvitePlayer(msg.PartyID, conn, target);
        }

        private void OnServerInviteResponse(NetworkConnectionToClient conn, PartyInviteResponseMessage msg)
        {
            if (msg.Accepted)
                AcceptInvite(conn, msg.PartyID);
            else
                DeclineInvite(conn, msg.PartyID);
        }

        private void OnServerPartyLeave(NetworkConnectionToClient conn, PartyLeaveMessage msg)
        {
            LeaveParty(conn);
        }

        private void OnServerPartyKick(NetworkConnectionToClient conn, PartyKickMessage msg)
        {
            var target = NetworkServer.connections.Values
                .FirstOrDefault(c => c.GetConnectionId() == msg.TargetConnectionID);

            if (target == null)
            {
                Debug.LogWarning($"[PartyManager] Target connection {msg.TargetConnectionID} not found");
                return;
            }

            KickMember(msg.PartyID, conn, target);
        }

        #endregion
    }
}
