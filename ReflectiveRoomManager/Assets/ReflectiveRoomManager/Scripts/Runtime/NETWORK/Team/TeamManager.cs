using System.Linq;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Team
{
    using Config;
    using Events;
    using Messages;
    using Formation;
    using Utilities;
    using REFLECTIVE.Runtime.Identifier;
    using REFLECTIVE.Runtime.NETWORK.Room;

    /// <summary>
    /// Manages teams within a room.
    /// Each room has its own TeamManager instance.
    /// </summary>
    public class TeamManager
    {
        #region Properties

        /// <summary>
        /// Team event manager for subscribing to team events.
        /// </summary>
        public TeamEventManager EventManager { get; }

        /// <summary>
        /// Current team formation strategy.
        /// </summary>
        public ITeamFormationStrategy FormationStrategy { get; private set; }

        /// <summary>
        /// Team configuration.
        /// </summary>
        public TeamConfig Config { get; private set; }

        /// <summary>
        /// The room this TeamManager belongs to.
        /// </summary>
        public Room Room { get; private set; }

        /// <summary>
        /// All teams in this room.
        /// </summary>
        public IReadOnlyList<Team> Teams => _teams;

        /// <summary>
        /// Whether the team system is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        #endregion

        #region Private Fields

        private readonly List<Team> _teams;
        private readonly Dictionary<NetworkConnection, Team> _connToTeamMap;
        private readonly UniqueIdentifier _idGenerator;

        #endregion

        #region Constructor

        public TeamManager()
        {
            EventManager = new TeamEventManager();
            _teams = new List<Team>();
            _connToTeamMap = new Dictionary<NetworkConnection, Team>();
            _idGenerator = new UniqueIdentifier();
            IsInitialized = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the team system for a room.
        /// </summary>
        public void Initialize(Room room, TeamConfig config)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[TeamManager] Already initialized");
                return;
            }

            Room = room;
            Config = config;

            // Create teams based on config
            CreateTeams(config?.TeamCount ?? 2, config?.DefaultMaxTeamSize ?? 10);

            // Set default formation strategy
            SetFormationStrategy(CreateDefaultStrategy(config?.DefaultFormationMode ?? TeamFormationMode.AutoDistribution));

            IsInitialized = true;

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[TeamManager] Initialized for room {room?.ID} with {_teams.Count} teams");
        }

        /// <summary>
        /// Creates teams based on configuration.
        /// </summary>
        private void CreateTeams(int teamCount, int maxTeamSize)
        {
            _teams.Clear();

            for (var i = 0; i < teamCount; i++)
            {
                var id = _idGenerator.CreateID();
                var name = Config?.GetTeamName(i) ?? $"Team {i + 1}";
                var color = Config?.GetTeamColor(i) ?? Color.white;

                var team = new Team(id, name, color, maxTeamSize);
                _teams.Add(team);
            }
        }

        /// <summary>
        /// Creates a formation strategy based on the mode.
        /// </summary>
        private ITeamFormationStrategy CreateDefaultStrategy(TeamFormationMode mode)
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

        /// <summary>
        /// Sets the formation strategy.
        /// </summary>
        public void SetFormationStrategy(ITeamFormationStrategy strategy)
        {
            FormationStrategy = strategy;

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[TeamManager] Strategy set to: {strategy?.StrategyName}");
        }

        #endregion

        #region Team Assignment

        /// <summary>
        /// Assigns a player to a team using the current formation strategy.
        /// </summary>
        public Team AssignPlayerToTeam(NetworkConnection conn, TeamContext context = null)
        {
            if (conn == null)
            {
                Debug.LogWarning("[TeamManager] Cannot assign null connection");
                return null;
            }

            // Check if already on a team
            if (_connToTeamMap.ContainsKey(conn))
            {
                Debug.LogWarning($"[TeamManager] Connection {conn.GetConnectionId()} already on a team");
                return _connToTeamMap[conn];
            }

            if (FormationStrategy == null)
            {
                Debug.LogWarning("[TeamManager] No formation strategy set");
                return null;
            }

            var team = FormationStrategy.AssignPlayer(_teams, conn, context);
            if (team == null)
            {
                Debug.LogWarning($"[TeamManager] Strategy failed to assign connection {conn.GetConnectionId()}");
                return null;
            }

            // Add to team
            var playerName = context?.PlayerName;
            if (!team.AddMember(conn, playerName))
            {
                Debug.LogWarning($"[TeamManager] Failed to add connection {conn.GetConnectionId()} to team {team.ID}");
                return null;
            }

            _connToTeamMap[conn] = team;

            EventManager.Invoke_OnPlayerAssigned(team, conn);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[TeamManager] Assigned {conn.GetConnectionId()} to team {team.ID} ({team.Name})");

            // Send assignment message to player
            SendTeamAssignment(conn, team);

            // Sync to all players in room
            BroadcastTeamSync();

            return team;
        }

        /// <summary>
        /// Assigns a player to a specific team by ID.
        /// </summary>
        public Team AssignPlayerToTeam(NetworkConnection conn, uint teamID, string playerName = null)
        {
            var team = GetTeam(teamID);
            if (team == null)
            {
                Debug.LogWarning($"[TeamManager] Team {teamID} not found");
                return null;
            }

            if (team.IsFull)
            {
                Debug.LogWarning($"[TeamManager] Team {teamID} is full");
                return null;
            }

            if (_connToTeamMap.ContainsKey(conn))
            {
                Debug.LogWarning($"[TeamManager] Connection {conn.GetConnectionId()} already on a team");
                return _connToTeamMap[conn];
            }

            if (!team.AddMember(conn, playerName))
                return null;

            _connToTeamMap[conn] = team;

            EventManager.Invoke_OnPlayerAssigned(team, conn);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[TeamManager] Assigned {conn.GetConnectionId()} to team {team.ID}");

            SendTeamAssignment(conn, team);
            BroadcastTeamSync();

            return team;
        }

        #endregion

        #region Team Operations

        /// <summary>
        /// Swaps a player to a different team.
        /// </summary>
        public bool SwapPlayer(NetworkConnection conn, uint targetTeamID)
        {
            if (Config != null && !Config.AllowManualSwap)
            {
                Debug.LogWarning("[TeamManager] Manual swapping is disabled");
                SendSwapResult(conn, false, "Team swapping is disabled");
                return false;
            }

            if (!_connToTeamMap.TryGetValue(conn, out var currentTeam))
            {
                Debug.LogWarning($"[TeamManager] Connection {conn.GetConnectionId()} is not on a team");
                SendSwapResult(conn, false, "You are not on a team");
                return false;
            }

            var targetTeam = GetTeam(targetTeamID);
            if (targetTeam == null)
            {
                Debug.LogWarning($"[TeamManager] Target team {targetTeamID} not found");
                SendSwapResult(conn, false, "Target team not found");
                return false;
            }

            if (targetTeam == currentTeam)
            {
                Debug.LogWarning($"[TeamManager] Connection {conn.GetConnectionId()} already on target team");
                SendSwapResult(conn, false, "Already on this team");
                return false;
            }

            if (targetTeam.IsFull)
            {
                Debug.LogWarning($"[TeamManager] Target team {targetTeamID} is full");
                SendSwapResult(conn, false, "Target team is full");
                return false;
            }

            // Get member data before removal
            var member = currentTeam.GetMember(conn);
            var playerName = member?.PlayerName;

            // Remove from current team
            currentTeam.RemoveMember(conn);

            // Add to new team
            targetTeam.AddMember(conn, playerName);
            _connToTeamMap[conn] = targetTeam;

            EventManager.Invoke_OnPlayerSwapped(conn, currentTeam, targetTeam);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[TeamManager] Swapped {conn.GetConnectionId()} from team {currentTeam.ID} to team {targetTeam.ID}");

            SendSwapResult(conn, true, null);
            SendTeamAssignment(conn, targetTeam);
            BroadcastTeamSync();

            return true;
        }

        /// <summary>
        /// Removes a player from their team.
        /// </summary>
        public bool RemovePlayer(NetworkConnection conn)
        {
            if (!_connToTeamMap.TryGetValue(conn, out var team))
            {
                Debug.LogWarning($"[TeamManager] Connection {conn.GetConnectionId()} is not on a team");
                return false;
            }

            team.RemoveMember(conn);
            _connToTeamMap.Remove(conn);

            EventManager.Invoke_OnPlayerRemoved(team, conn);

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[TeamManager] Removed {conn.GetConnectionId()} from team {team.ID}");

            BroadcastTeamSync();

            return true;
        }

        /// <summary>
        /// Auto-balances teams by member count.
        /// </summary>
        public void AutoBalance()
        {
            if (_teams.Count < 2) return;

            var changes = new List<TeamAssignmentChange>();
            var avgSize = _connToTeamMap.Count / (float)_teams.Count;

            // Sort teams by member count
            var sortedTeams = _teams.OrderByDescending(t => t.MemberCount).ToList();

            // Move players from larger teams to smaller teams
            for (var i = 0; i < sortedTeams.Count - 1; i++)
            {
                var largerTeam = sortedTeams[i];
                var smallerTeam = sortedTeams[sortedTeams.Count - 1 - i];

                while (largerTeam.MemberCount > avgSize + 0.5f && smallerTeam.MemberCount < avgSize - 0.5f)
                {
                    // Get the last member to move (newest)
                    var memberToMove = largerTeam.Members.LastOrDefault();
                    if (memberToMove == null) break;

                    var conn = memberToMove.Connection;
                    var playerName = memberToMove.PlayerName;

                    largerTeam.RemoveMember(conn);
                    smallerTeam.AddMember(conn, playerName);
                    _connToTeamMap[conn] = smallerTeam;

                    changes.Add(new TeamAssignmentChange(conn.GetConnectionId(), largerTeam.ID, smallerTeam.ID));
                }
            }

            if (changes.Count > 0)
            {
                EventManager.Invoke_OnTeamsBalanced(changes);

                if (Config != null && Config.EnableDebugLogs)
                    Debug.Log($"[TeamManager] Auto-balanced {changes.Count} players");

                // Notify clients
                BroadcastBalanceChanges(changes);
                BroadcastTeamSync();
            }
        }

        /// <summary>
        /// Shuffles all players randomly across teams.
        /// </summary>
        public void Shuffle()
        {
            var allPlayers = _connToTeamMap.Keys.ToList();
            var random = new System.Random();

            // Clear all teams
            foreach (var team in _teams)
            {
                foreach (var member in team.Members.ToList())
                    team.RemoveMember(member.Connection);
            }
            _connToTeamMap.Clear();

            // Shuffle players
            for (var i = allPlayers.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (allPlayers[i], allPlayers[j]) = (allPlayers[j], allPlayers[i]);
            }

            // Reassign using round-robin
            var teamIndex = 0;
            foreach (var conn in allPlayers)
            {
                var team = _teams[teamIndex % _teams.Count];
                team.AddMember(conn);
                _connToTeamMap[conn] = team;
                teamIndex++;
            }

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log($"[TeamManager] Shuffled {allPlayers.Count} players");

            BroadcastTeamSync();
        }

        #endregion

        #region Stats

        /// <summary>
        /// Adds score to a team.
        /// </summary>
        public void AddTeamScore(uint teamID, int points)
        {
            var team = GetTeam(teamID);
            if (team == null) return;

            team.AddScore(points);
            EventManager.Invoke_OnTeamScoreChanged(team, team.Stats.TotalScore);
        }

        /// <summary>
        /// Records a kill for a player.
        /// </summary>
        public void RecordKill(NetworkConnection killer)
        {
            if (!_connToTeamMap.TryGetValue(killer, out var team)) return;
            team.AddKill(killer);
            EventManager.Invoke_OnPlayerKill(team, killer);
        }

        /// <summary>
        /// Records a death for a player.
        /// </summary>
        public void RecordDeath(NetworkConnection deceased)
        {
            if (!_connToTeamMap.TryGetValue(deceased, out var team)) return;
            team.AddDeath(deceased);
            EventManager.Invoke_OnPlayerDeath(team, deceased);
        }

        /// <summary>
        /// Gets team statistics.
        /// </summary>
        public TeamStats GetTeamStats(uint teamID)
        {
            return GetTeam(teamID)?.Stats;
        }

        /// <summary>
        /// Resets all team statistics.
        /// </summary>
        public void ResetAllStats()
        {
            foreach (var team in _teams)
                team.ResetStats();

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log("[TeamManager] All stats reset");

            BroadcastTeamSync();
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Gets a team by ID.
        /// </summary>
        public Team GetTeam(uint teamID)
        {
            return _teams.FirstOrDefault(t => t.ID == teamID);
        }

        /// <summary>
        /// Gets the team a player is on.
        /// </summary>
        public Team GetPlayerTeam(NetworkConnection conn)
        {
            return _connToTeamMap.TryGetValue(conn, out var team) ? team : null;
        }

        /// <summary>
        /// Gets all teams.
        /// </summary>
        public IReadOnlyList<Team> GetAllTeams()
        {
            return _teams;
        }

        /// <summary>
        /// Checks if a player is on any team.
        /// </summary>
        public bool IsOnTeam(NetworkConnection conn)
        {
            return _connToTeamMap.ContainsKey(conn);
        }

        #endregion

        #region Network Messages

        private void SendTeamAssignment(NetworkConnection conn, Team team)
        {
            var memberData = team.Members.Select(m => new TeamMemberData(
                m.ConnectionId,
                m.PlayerName,
                m.Kills,
                m.Deaths,
                m.Assists,
                m.Score
            )).ToArray();

            var msg = new TeamAssignmentMessage(team.ID, team.Name, team.TeamColor, memberData);
            conn.Send(msg);
        }

        private void SendSwapResult(NetworkConnection conn, bool success, string errorMessage)
        {
            var msg = success
                ? TeamSwapResultMessage.CreateSuccess(_connToTeamMap.TryGetValue(conn, out var t) ? t.ID : 0)
                : TeamSwapResultMessage.CreateFailure(errorMessage);
            conn.Send(msg);
        }

        private void BroadcastTeamSync()
        {
            if (Room == null) return;

            var teamData = _teams.Select(CreateTeamData).ToArray();
            var msg = new TeamSyncMessage(teamData);

            foreach (var conn in Room.Connections)
            {
                conn?.Send(msg);
            }
        }

        private void BroadcastBalanceChanges(List<TeamAssignmentChange> changes)
        {
            if (Room == null) return;

            var msg = new TeamBalanceMessage(changes.ToArray());
            foreach (var conn in Room.Connections)
            {
                conn?.Send(msg);
            }
        }

        private TeamData CreateTeamData(Team team)
        {
            var memberData = team.Members.Select(m => new TeamMemberData(
                m.ConnectionId,
                m.PlayerName,
                m.Kills,
                m.Deaths,
                m.Assists,
                m.Score
            )).ToArray();

            var statsData = new TeamStatsData(
                team.Stats.TotalScore,
                team.Stats.TotalKills,
                team.Stats.TotalDeaths,
                team.Stats.TotalAssists
            );

            return new TeamData(team.ID, team.Name, team.TeamColor, team.MaxSize, memberData, statsData);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clears all team data (for room cleanup).
        /// </summary>
        public void Clear()
        {
            _teams.Clear();
            _connToTeamMap.Clear();
            IsInitialized = false;

            if (Config != null && Config.EnableDebugLogs)
                Debug.Log("[TeamManager] Cleared");
        }

        #endregion
    }
}
