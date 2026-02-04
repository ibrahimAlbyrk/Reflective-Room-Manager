using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Team.Formation
{
    /// <summary>
    /// Strategy that automatically distributes players across teams.
    /// Supports multiple distribution modes.
    /// </summary>
    public class AutoDistributionStrategy : ITeamFormationStrategy
    {
        public string StrategyName => $"Auto Distribution ({_mode})";

        private readonly DistributionMode _mode;
        private int _roundRobinIndex;
        private readonly System.Random _random;

        public AutoDistributionStrategy(DistributionMode mode = DistributionMode.RoundRobin)
        {
            _mode = mode;
            _roundRobinIndex = 0;
            _random = new System.Random();
        }

        public Team AssignPlayer(IReadOnlyList<Team> teams, NetworkConnection conn, TeamContext context)
        {
            if (teams == null || teams.Count == 0)
            {
                Debug.LogWarning("[AutoDistributionStrategy] No teams available");
                return null;
            }

            // Priority 1: Try to join party members' team
            if (context?.HasPartyMembersOnTeams == true)
            {
                var partyTeam = TryGetPartyMembersTeam(teams, context.PartyMembers);
                if (partyTeam != null && !partyTeam.IsFull)
                    return partyTeam;
            }

            // Priority 2: Honor team preference if set by party leader
            if (context?.PreferredTeamID.HasValue == true)
            {
                var preferred = teams.FirstOrDefault(t => t.ID == context.PreferredTeamID.Value);
                if (preferred != null && !preferred.IsFull)
                    return preferred;
            }

            // Priority 3: Use distribution mode
            return _mode switch
            {
                DistributionMode.RoundRobin => AssignRoundRobin(teams),
                DistributionMode.Random => AssignRandom(teams),
                DistributionMode.LeastPopulated => AssignLeastPopulated(teams),
                _ => AssignLeastPopulated(teams)
            };
        }

        private Team TryGetPartyMembersTeam(IReadOnlyList<Team> teams, List<NetworkConnection> partyMembers)
        {
            foreach (var team in teams)
            {
                if (partyMembers.Any(pm => team.IsMember(pm)))
                    return team;
            }
            return null;
        }

        private Team AssignRoundRobin(IReadOnlyList<Team> teams)
        {
            var availableTeams = teams.Where(t => !t.IsFull).ToList();
            if (availableTeams.Count == 0)
                return null;

            // Find the next available team in round-robin order
            for (var i = 0; i < teams.Count; i++)
            {
                var index = (_roundRobinIndex + i) % teams.Count;
                var team = teams[index];
                if (!team.IsFull)
                {
                    _roundRobinIndex = (index + 1) % teams.Count;
                    return team;
                }
            }

            return null;
        }

        private Team AssignRandom(IReadOnlyList<Team> teams)
        {
            var availableTeams = teams.Where(t => !t.IsFull).ToList();
            if (availableTeams.Count == 0)
                return null;

            var index = _random.Next(availableTeams.Count);
            return availableTeams[index];
        }

        private Team AssignLeastPopulated(IReadOnlyList<Team> teams)
        {
            return teams
                .Where(t => !t.IsFull)
                .OrderBy(t => t.MemberCount)
                .FirstOrDefault();
        }

        /// <summary>
        /// Resets the round-robin counter.
        /// </summary>
        public void ResetRoundRobin()
        {
            _roundRobinIndex = 0;
        }
    }

    /// <summary>
    /// Distribution modes for automatic team assignment.
    /// </summary>
    public enum DistributionMode
    {
        /// <summary>
        /// Assigns players in order: Team 1, Team 2, Team 1, Team 2, etc.
        /// </summary>
        RoundRobin,

        /// <summary>
        /// Randomly assigns players to available teams.
        /// </summary>
        Random,

        /// <summary>
        /// Always assigns to the team with the fewest members.
        /// </summary>
        LeastPopulated
    }
}
