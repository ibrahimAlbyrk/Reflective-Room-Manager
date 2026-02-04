using System;
using System.Collections.Generic;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Team.Events
{
    using Messages;

    /// <summary>
    /// Manages events for the team system.
    /// Provides both server-side and client-side event hooks.
    /// </summary>
    public class TeamEventManager
    {
        #region Server-Side Events

        /// <summary>
        /// Called on server when a player is assigned to a team.
        /// Parameters: Team, NetworkConnection
        /// </summary>
        public event Action<Team, NetworkConnection> OnPlayerAssigned;

        /// <summary>
        /// Called on server when a player is removed from a team.
        /// Parameters: Team, NetworkConnection
        /// </summary>
        public event Action<Team, NetworkConnection> OnPlayerRemoved;

        /// <summary>
        /// Called on server when a player swaps teams.
        /// Parameters: NetworkConnection, Team (from), Team (to)
        /// </summary>
        public event Action<NetworkConnection, Team, Team> OnPlayerSwapped;

        /// <summary>
        /// Called on server when teams are balanced.
        /// Parameters: List of assignment changes
        /// </summary>
        public event Action<List<TeamAssignmentChange>> OnTeamsBalanced;

        /// <summary>
        /// Called on server when a team's score changes.
        /// Parameters: Team, int (new score)
        /// </summary>
        public event Action<Team, int> OnTeamScoreChanged;

        /// <summary>
        /// Called on server when a player gets a kill.
        /// Parameters: Team, NetworkConnection (killer)
        /// </summary>
        public event Action<Team, NetworkConnection> OnPlayerKill;

        /// <summary>
        /// Called on server when a player dies.
        /// Parameters: Team, NetworkConnection (deceased)
        /// </summary>
        public event Action<Team, NetworkConnection> OnPlayerDeath;

        #endregion

        #region Client-Side Events

        /// <summary>
        /// Called on client when they are assigned to a team.
        /// Parameters: uint (team ID), string (team name)
        /// </summary>
        public event Action<uint, string> OnClientTeamAssigned;

        /// <summary>
        /// Called on client when they leave a team.
        /// Parameters: uint (team ID)
        /// </summary>
        public event Action<uint> OnClientTeamLeft;

        /// <summary>
        /// Called on client when team data is updated.
        /// Parameters: TeamData array
        /// </summary>
        public event Action<TeamData[]> OnClientTeamsUpdated;

        /// <summary>
        /// Called on client when their team swap request completes.
        /// Parameters: bool (success), string (error message if failed)
        /// </summary>
        public event Action<bool, string> OnClientSwapResult;

        /// <summary>
        /// Called on client when teams are rebalanced.
        /// Parameters: TeamAssignmentChange array
        /// </summary>
        public event Action<TeamAssignmentChange[]> OnClientTeamsBalanced;

        #endregion

        #region Server Event Invokers

        internal void Invoke_OnPlayerAssigned(Team team, NetworkConnection conn)
        {
            OnPlayerAssigned?.Invoke(team, conn);
        }

        internal void Invoke_OnPlayerRemoved(Team team, NetworkConnection conn)
        {
            OnPlayerRemoved?.Invoke(team, conn);
        }

        internal void Invoke_OnPlayerSwapped(NetworkConnection conn, Team fromTeam, Team toTeam)
        {
            OnPlayerSwapped?.Invoke(conn, fromTeam, toTeam);
        }

        internal void Invoke_OnTeamsBalanced(List<TeamAssignmentChange> changes)
        {
            OnTeamsBalanced?.Invoke(changes);
        }

        internal void Invoke_OnTeamScoreChanged(Team team, int newScore)
        {
            OnTeamScoreChanged?.Invoke(team, newScore);
        }

        internal void Invoke_OnPlayerKill(Team team, NetworkConnection killer)
        {
            OnPlayerKill?.Invoke(team, killer);
        }

        internal void Invoke_OnPlayerDeath(Team team, NetworkConnection deceased)
        {
            OnPlayerDeath?.Invoke(team, deceased);
        }

        #endregion

        #region Client Event Invokers

        internal void Invoke_OnClientTeamAssigned(uint teamID, string teamName)
        {
            OnClientTeamAssigned?.Invoke(teamID, teamName);
        }

        internal void Invoke_OnClientTeamLeft(uint teamID)
        {
            OnClientTeamLeft?.Invoke(teamID);
        }

        internal void Invoke_OnClientTeamsUpdated(TeamData[] teams)
        {
            OnClientTeamsUpdated?.Invoke(teams);
        }

        internal void Invoke_OnClientSwapResult(bool success, string errorMessage)
        {
            OnClientSwapResult?.Invoke(success, errorMessage);
        }

        internal void Invoke_OnClientTeamsBalanced(TeamAssignmentChange[] changes)
        {
            OnClientTeamsBalanced?.Invoke(changes);
        }

        #endregion
    }
}
