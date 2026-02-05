using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using REFLECTIVE.Runtime.NETWORK.Party;
    using REFLECTIVE.Runtime.NETWORK.Party.Config;
    using REFLECTIVE.Runtime.NETWORK.Party.Events;
    using REFLECTIVE.Runtime.NETWORK.Utilities;

    /// <summary>
    /// Partial class for Party system integration in RoomManagerBase.
    /// </summary>
    public abstract partial class RoomManagerBase
    {
        #region Serialize Variables

        [Header("Party System")]
        [Tooltip("Enable the party system")]
        [SerializeField] protected bool _enablePartySystem;

        [Tooltip("Party system configuration")]
        [SerializeField] protected PartyConfig _partyConfig;

        #endregion

        #region Properties

        /// <summary>
        /// Whether the party system is enabled.
        /// </summary>
        public bool EnablePartySystem => _enablePartySystem;

        /// <summary>
        /// The global party manager instance.
        /// </summary>
        public PartyManager PartyManager => m_partyManager;

        /// <summary>
        /// Party system event manager (server-side).
        /// </summary>
        public PartyEventManager PartyEvents => m_partyManager?.EventManager;

        /// <summary>
        /// Client-side party event manager.
        /// </summary>
        public PartyEventManager ClientPartyEvents => m_clientPartyEventManager;

        #endregion

        #region Private Fields

        protected PartyManager m_partyManager;
        protected PartyEventManager m_clientPartyEventManager;
        private float _partyInviteCleanupTimer;
        private const float PARTY_INVITE_CLEANUP_INTERVAL = 10f;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the party system.
        /// Called during Awake if party system is enabled.
        /// </summary>
        protected virtual void InitializePartySystem()
        {
            if (!_enablePartySystem) return;

            if (_partyConfig == null)
            {
                Debug.LogWarning("[RoomManagerBase] Party system enabled but no config assigned. Using defaults.");
            }

#if REFLECTIVE_SERVER
            m_partyManager = new PartyManager(_partyConfig);

            // Subscribe to party events
            m_partyManager.EventManager.OnPartyCreated += OnPartyCreated;
            m_partyManager.EventManager.OnPartyDisbanded += OnPartyDisbanded;
            m_partyManager.EventManager.OnMemberJoined += OnPartyMemberJoined;
            m_partyManager.EventManager.OnMemberLeft += OnPartyMemberLeft;
            m_partyManager.EventManager.OnLeaderChanged += OnPartyLeaderChanged;
#endif

#if REFLECTIVE_CLIENT
            m_clientPartyEventManager = new PartyEventManager();
#endif

            Debug.Log("[RoomManagerBase] Party system initialized");
        }

#if REFLECTIVE_SERVER
        /// <summary>
        /// Registers party system network handlers.
        /// Called when server starts.
        /// </summary>
        protected virtual void RegisterPartyHandlers()
        {
            if (!_enablePartySystem || m_partyManager == null) return;

            m_partyManager.RegisterServerHandlers();
        }

        /// <summary>
        /// Unregisters party system network handlers.
        /// Called when server stops.
        /// </summary>
        protected virtual void UnregisterPartyHandlers()
        {
            if (m_partyManager == null) return;

            m_partyManager.UnregisterServerHandlers();
        }
#endif

#if REFLECTIVE_CLIENT
        /// <summary>
        /// Registers client-side party handlers.
        /// Called when client connects.
        /// </summary>
        protected virtual void RegisterPartyClientHandlers()
        {
            if (!_enablePartySystem || m_clientPartyEventManager == null) return;

            PartyManager.RegisterClientHandlers(m_clientPartyEventManager);

            if (_partyConfig != null && _partyConfig.EnableDebugLogs)
                Debug.Log("[RoomManagerBase] Party client handlers registered");
        }

        /// <summary>
        /// Unregisters client-side party handlers.
        /// Called when client disconnects.
        /// </summary>
        protected virtual void UnregisterPartyClientHandlers()
        {
            PartyManager.UnregisterClientHandlers();
        }
#endif

        #endregion

#if REFLECTIVE_SERVER
        #region Update

        /// <summary>
        /// Updates the party system.
        /// Called from Update if party system is enabled.
        /// </summary>
        protected virtual void UpdatePartySystem()
        {
            if (!_enablePartySystem || m_partyManager == null) return;

            // Periodic cleanup of expired invites
            _partyInviteCleanupTimer += Time.deltaTime;
            if (_partyInviteCleanupTimer >= PARTY_INVITE_CLEANUP_INTERVAL)
            {
                _partyInviteCleanupTimer = 0f;
                m_partyManager.CleanupExpiredInvites();
            }
        }

        #endregion

        #region Disconnect Handling

        /// <summary>
        /// Handles party cleanup when a player disconnects.
        /// </summary>
        protected virtual void HandlePartyDisconnect(NetworkConnectionToClient conn)
        {
            if (!_enablePartySystem || m_partyManager == null) return;

            m_partyManager.HandleDisconnect(conn);
        }

        #endregion
#endif

#if REFLECTIVE_SERVER
        #region Party Event Handlers

        /// <summary>
        /// Called when a party is created.
        /// </summary>
        protected virtual void OnPartyCreated(Party party)
        {
            if (_partyConfig != null && _partyConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Party {party.ID} created");
        }

        /// <summary>
        /// Called when a party is disbanded.
        /// </summary>
        protected virtual void OnPartyDisbanded(Party party)
        {
            if (_partyConfig != null && _partyConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Party {party.ID} disbanded");
        }

        /// <summary>
        /// Called when a member joins a party.
        /// </summary>
        protected virtual void OnPartyMemberJoined(Party party, NetworkConnection conn)
        {
            if (_partyConfig != null && _partyConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Connection {conn.GetConnectionId()} joined party {party.ID}");
        }

        /// <summary>
        /// Called when a member leaves a party.
        /// </summary>
        protected virtual void OnPartyMemberLeft(Party party, NetworkConnection conn)
        {
            if (_partyConfig != null && _partyConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Connection {conn.GetConnectionId()} left party {party.ID}");
        }

        /// <summary>
        /// Called when party leadership changes.
        /// </summary>
        protected virtual void OnPartyLeaderChanged(Party party, NetworkConnection oldLeader, NetworkConnection newLeader)
        {
            if (_partyConfig != null && _partyConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Party {party.ID} leadership changed from {oldLeader.GetConnectionId()} to {newLeader.GetConnectionId()}");
        }

        #endregion
#endif

        #region Cleanup

        /// <summary>
        /// Cleans up the party system.
        /// Called during OnDestroy.
        /// </summary>
        protected virtual void CleanupPartySystem()
        {
#if REFLECTIVE_SERVER
            if (m_partyManager != null)
            {
                m_partyManager.EventManager.OnPartyCreated -= OnPartyCreated;
                m_partyManager.EventManager.OnPartyDisbanded -= OnPartyDisbanded;
                m_partyManager.EventManager.OnMemberJoined -= OnPartyMemberJoined;
                m_partyManager.EventManager.OnMemberLeft -= OnPartyMemberLeft;
                m_partyManager.EventManager.OnLeaderChanged -= OnPartyLeaderChanged;

                m_partyManager.ClearAll();
                m_partyManager = null;
            }
#endif

#if REFLECTIVE_CLIENT
            m_clientPartyEventManager = null;
#endif
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the party a connection belongs to.
        /// </summary>
        public Party GetPartyByConnection(NetworkConnection conn)
        {
            return m_partyManager?.GetPartyByMember(conn);
        }

        /// <summary>
        /// Gets the party ID for a connection.
        /// </summary>
        public uint? GetPartyIDByConnection(NetworkConnection conn)
        {
            return m_partyManager?.GetPartyIDByMember(conn);
        }

        /// <summary>
        /// Checks if a connection is in a party.
        /// </summary>
        public bool IsInParty(NetworkConnection conn)
        {
            return m_partyManager?.IsInParty(conn) ?? false;
        }

        #endregion
    }
}
