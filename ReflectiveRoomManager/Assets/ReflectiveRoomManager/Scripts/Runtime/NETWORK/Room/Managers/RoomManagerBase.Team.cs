using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using REFLECTIVE.Runtime.NETWORK.Team;
    using REFLECTIVE.Runtime.NETWORK.Team.Config;
    using REFLECTIVE.Runtime.NETWORK.Team.Events;
    using REFLECTIVE.Runtime.NETWORK.Team.Formation;
    using REFLECTIVE.Runtime.NETWORK.Team.Integration;
    using REFLECTIVE.Runtime.NETWORK.Team.Messages;
    using REFLECTIVE.Runtime.NETWORK.Utilities;

    /// <summary>
    /// Partial class for Team system integration in RoomManagerBase.
    /// </summary>
    public abstract partial class RoomManagerBase
    {
        #region Serialize Variables

        [Header("Team System")]
        [Tooltip("Enable the team system")]
        [SerializeField] protected bool _enableTeamSystem;

        [Tooltip("Team system configuration")]
        [SerializeField] protected TeamConfig _teamConfig;

        #endregion

        #region Properties

        /// <summary>
        /// Whether the team system is enabled.
        /// </summary>
        public bool EnableTeamSystem => _enableTeamSystem;

        /// <summary>
        /// Team configuration.
        /// </summary>
        public TeamConfig TeamConfig => _teamConfig;

        /// <summary>
        /// Team preference provider for party-team integration.
        /// </summary>
        public TeamPreferenceProvider TeamPreferences => m_teamPreferences;

        /// <summary>
        /// Client-side team event manager.
        /// </summary>
        public TeamEventManager ClientTeamEvents => m_clientTeamEventManager;

        #endregion

        #region Private Fields

        protected TeamPreferenceProvider m_teamPreferences;
        private bool _teamServerHandlersRegistered;
        private bool _teamClientHandlersRegistered;

        /// <summary>
        /// Client-side event manager for team events.
        /// </summary>
        protected TeamEventManager m_clientTeamEventManager;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the team system infrastructure.
        /// Called during Awake if team system is enabled.
        /// </summary>
        protected virtual void InitializeTeamSystemInfrastructure()
        {
            if (!_enableTeamSystem) return;

            if (_teamConfig == null)
            {
                Debug.LogWarning("[RoomManagerBase] Team system enabled but no config assigned. Using defaults.");
            }

            m_teamPreferences = new TeamPreferenceProvider();
            m_clientTeamEventManager = new TeamEventManager();

            Debug.Log("[RoomManagerBase] Team system infrastructure initialized");
        }

        /// <summary>
        /// Initializes the team system for a specific room.
        /// Called when a room is created.
        /// </summary>
        protected virtual void InitializeTeamSystem(Room room)
        {
            if (!_enableTeamSystem || room == null) return;

            var teamManager = new TeamManager();
            teamManager.Initialize(room, _teamConfig);

            // Subscribe to team events
            teamManager.EventManager.OnPlayerAssigned += (team, conn) => OnPlayerAssignedToTeam(room, team, conn);
            teamManager.EventManager.OnPlayerRemoved += (team, conn) => OnPlayerRemovedFromTeam(room, team, conn);
            teamManager.EventManager.OnPlayerSwapped += (conn, fromTeam, toTeam) => OnPlayerSwappedTeam(room, conn, fromTeam, toTeam);
            teamManager.EventManager.OnTeamScoreChanged += (team, score) => OnTeamScoreChanged(room, team, score);

            // Store in room (requires Room class modification or extension)
            SetRoomTeamManager(room, teamManager);

            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Team system initialized for room {room.ID}");
        }

        /// <summary>
        /// Creates a formation strategy based on the mode.
        /// </summary>
        protected virtual ITeamFormationStrategy CreateFormationStrategy(TeamFormationMode mode)
        {
            return mode switch
            {
                TeamFormationMode.ManualSelection => new ManualSelectionStrategy(),
                TeamFormationMode.AutoDistribution => new AutoDistributionStrategy(DistributionMode.RoundRobin),
                TeamFormationMode.CaptainPick => new CaptainPickStrategy(),
                TeamFormationMode.SkillBased => new SkillBasedStrategy(),
                _ => new AutoDistributionStrategy(DistributionMode.RoundRobin)
            };
        }

#if REFLECTIVE_SERVER
        /// <summary>
        /// Registers team system server network handlers.
        /// Called when server starts.
        /// </summary>
        protected virtual void RegisterTeamServerHandlers()
        {
            if (!_enableTeamSystem || _teamServerHandlersRegistered) return;

            NetworkServer.RegisterHandler<TeamSwapRequestMessage>(OnServerTeamSwapRequest);

            _teamServerHandlersRegistered = true;
        }

        /// <summary>
        /// Unregisters team system server network handlers.
        /// Called when server stops.
        /// </summary>
        protected virtual void UnregisterTeamServerHandlers()
        {
            if (!_teamServerHandlersRegistered) return;

            NetworkServer.UnregisterHandler<TeamSwapRequestMessage>();

            _teamServerHandlersRegistered = false;
        }
#endif

#if REFLECTIVE_CLIENT
        /// <summary>
        /// Registers team system client network handlers.
        /// Called when client starts.
        /// </summary>
        protected virtual void RegisterTeamClientHandlers()
        {
            if (!_enableTeamSystem || _teamClientHandlersRegistered) return;

            NetworkClient.RegisterHandler<TeamAssignmentMessage>(OnClientTeamAssignment);
            NetworkClient.RegisterHandler<TeamSwapResultMessage>(OnClientTeamSwapResult);
            NetworkClient.RegisterHandler<TeamSyncMessage>(OnClientTeamSync);
            NetworkClient.RegisterHandler<TeamBalanceMessage>(OnClientTeamBalance);

            _teamClientHandlersRegistered = true;

            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log("[RoomManagerBase] Team client handlers registered");
        }

        /// <summary>
        /// Unregisters team system client network handlers.
        /// Called when client stops.
        /// </summary>
        protected virtual void UnregisterTeamClientHandlers()
        {
            if (!_teamClientHandlersRegistered) return;

            NetworkClient.UnregisterHandler<TeamAssignmentMessage>();
            NetworkClient.UnregisterHandler<TeamSwapResultMessage>();
            NetworkClient.UnregisterHandler<TeamSyncMessage>();
            NetworkClient.UnregisterHandler<TeamBalanceMessage>();

            _teamClientHandlersRegistered = false;

            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log("[RoomManagerBase] Team client handlers unregistered");
        }
#endif

        #endregion

        #region Room Lifecycle Hooks

        /// <summary>
        /// Assigns a player to a team when they join a room.
        /// Override to customize team assignment logic.
        /// </summary>
        protected virtual void AssignPlayerToTeam(NetworkConnection conn, Room room)
        {
            if (!_enableTeamSystem || room == null) return;

            var teamManager = GetRoomTeamManager(room);
            if (teamManager == null) return;

            // Build context with party information
            TeamContext context = null;
            if (_enablePartySystem && m_partyManager != null)
            {
                context = PartyToTeamConverter.BuildTeamContext(
                    m_partyManager,
                    conn,
                    teamManager,
                    m_teamPreferences
                );
            }

            teamManager.AssignPlayerToTeam(conn, context);
        }

        /// <summary>
        /// Removes a player from their team when they exit a room.
        /// </summary>
        protected virtual void RemovePlayerFromTeam(NetworkConnection conn, Room room)
        {
            if (!_enableTeamSystem || room == null) return;

            var teamManager = GetRoomTeamManager(room);
            teamManager?.RemovePlayer(conn);
        }

        /// <summary>
        /// Cleans up the team system for a room.
        /// Called when a room is removed.
        /// </summary>
        protected virtual void CleanupRoomTeamSystem(Room room)
        {
            if (!_enableTeamSystem || room == null) return;

            var teamManager = GetRoomTeamManager(room);
            if (teamManager != null)
            {
                teamManager.Clear();
                SetRoomTeamManager(room, null);
            }

            // Clear team preferences for any parties that were in this room
            // (Optional: could track which parties were in which room)
        }

        #endregion

        #region Team Event Handlers

        /// <summary>
        /// Called when a player is assigned to a team.
        /// </summary>
        protected virtual void OnPlayerAssignedToTeam(Room room, Team team, NetworkConnection conn)
        {
            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Player {conn.GetConnectionId()} assigned to team {team.ID} in room {room.ID}");
        }

        /// <summary>
        /// Called when a player is removed from a team.
        /// </summary>
        protected virtual void OnPlayerRemovedFromTeam(Room room, Team team, NetworkConnection conn)
        {
            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Player {conn.GetConnectionId()} removed from team {team.ID} in room {room.ID}");
        }

        /// <summary>
        /// Called when a player swaps teams.
        /// </summary>
        protected virtual void OnPlayerSwappedTeam(Room room, NetworkConnection conn, Team fromTeam, Team toTeam)
        {
            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Player {conn.GetConnectionId()} swapped from team {fromTeam.ID} to team {toTeam.ID}");
        }

        /// <summary>
        /// Called when a team's score changes.
        /// </summary>
        protected virtual void OnTeamScoreChanged(Room room, Team team, int newScore)
        {
            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Team {team.ID} score changed to {newScore} in room {room.ID}");
        }

        #endregion

#if REFLECTIVE_SERVER
        #region Network Message Handlers (Server)

        private void OnServerTeamSwapRequest(NetworkConnectionToClient conn, TeamSwapRequestMessage msg)
        {
            var room = GetRoomByConnection(conn);
            if (room == null)
            {
                Debug.LogWarning($"[RoomManagerBase] Swap request from {conn.GetConnectionId()} but not in a room");
                return;
            }

            var teamManager = GetRoomTeamManager(room);
            if (teamManager == null)
            {
                Debug.LogWarning($"[RoomManagerBase] No TeamManager for room {room.ID}");
                return;
            }

            teamManager.SwapPlayer(conn, msg.TargetTeamID);
        }

        #endregion
#endif

#if REFLECTIVE_CLIENT
        #region Network Message Handlers (Client)

        private void OnClientTeamAssignment(TeamAssignmentMessage msg)
        {
            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Assigned to team: {msg.TeamName} (ID: {msg.TeamID})");

            m_clientTeamEventManager?.Invoke_OnClientTeamAssigned(msg.TeamID, msg.TeamName);
        }

        private void OnClientTeamSwapResult(TeamSwapResultMessage msg)
        {
            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Team swap result: {(msg.Success ? "Success" : "Failed - " + msg.ErrorMessage)}");

            m_clientTeamEventManager?.Invoke_OnClientSwapResult(msg.Success, msg.ErrorMessage);
        }

        private void OnClientTeamSync(TeamSyncMessage msg)
        {
            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Team sync received with {msg.Teams?.Length ?? 0} teams");

            m_clientTeamEventManager?.Invoke_OnClientTeamsUpdated(msg.Teams);
        }

        private void OnClientTeamBalance(TeamBalanceMessage msg)
        {
            if (_teamConfig != null && _teamConfig.EnableDebugLogs)
                Debug.Log($"[RoomManagerBase] Team balance received with {msg.Changes?.Length ?? 0} changes");

            m_clientTeamEventManager?.Invoke_OnClientTeamsBalanced(msg.Changes);
        }

        #endregion
#endif

        #region Room-TeamManager Association

        // These methods provide a way to associate TeamManagers with Rooms.
        // In a production implementation, you might want to extend the Room class directly.

        private static readonly System.Collections.Generic.Dictionary<uint, TeamManager> _roomTeamManagers = new();

        /// <summary>
        /// Gets the TeamManager for a room.
        /// </summary>
        public TeamManager GetRoomTeamManager(Room room)
        {
            if (room == null) return null;
            return _roomTeamManagers.TryGetValue(room.ID, out var manager) ? manager : null;
        }

        /// <summary>
        /// Sets the TeamManager for a room.
        /// </summary>
        protected void SetRoomTeamManager(Room room, TeamManager manager)
        {
            if (room == null) return;

            if (manager == null)
                _roomTeamManagers.Remove(room.ID);
            else
                _roomTeamManagers[room.ID] = manager;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleans up the team system infrastructure.
        /// Called during OnDestroy.
        /// </summary>
        protected virtual void CleanupTeamSystemInfrastructure()
        {
            m_teamPreferences?.ClearAll();
            m_teamPreferences = null;
            m_clientTeamEventManager = null;
            _roomTeamManagers.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the team a player is on in their current room.
        /// </summary>
        public Team GetPlayerTeam(NetworkConnection conn)
        {
            var room = GetRoomByConnection(conn);
            if (room == null) return null;

            var teamManager = GetRoomTeamManager(room);
            return teamManager?.GetPlayerTeam(conn);
        }

        /// <summary>
        /// Gets all teams in a room.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<Team> GetRoomTeams(Room room)
        {
            var teamManager = GetRoomTeamManager(room);
            return teamManager?.Teams;
        }

        /// <summary>
        /// Gets all teams in a room by room ID.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<Team> GetRoomTeams(uint roomID)
        {
            var room = GetRoom(roomID);
            return GetRoomTeams(room);
        }

        /// <summary>
        /// Sets the team preference for a party.
        /// </summary>
        public void SetPartyTeamPreference(uint partyID, uint teamID)
        {
            m_teamPreferences?.SetPreferredTeamID(partyID, teamID);
        }

        /// <summary>
        /// Clears the team preference for a party.
        /// </summary>
        public void ClearPartyTeamPreference(uint partyID)
        {
            m_teamPreferences?.ClearPreference(partyID);
        }

        #endregion
    }
}
