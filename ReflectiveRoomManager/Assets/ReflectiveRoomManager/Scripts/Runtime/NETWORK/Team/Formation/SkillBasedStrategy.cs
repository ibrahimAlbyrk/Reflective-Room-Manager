using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Team.Formation
{
    /// <summary>
    /// Strategy that assigns players based on skill rating for balanced teams.
    /// Uses an external skill provider interface for flexibility.
    /// </summary>
    public class SkillBasedStrategy : ITeamFormationStrategy
    {
        public string StrategyName => "Skill-Based";

        private readonly ISkillProvider _skillProvider;

        public SkillBasedStrategy(ISkillProvider skillProvider = null)
        {
            _skillProvider = skillProvider;
        }

        public Team AssignPlayer(IReadOnlyList<Team> teams, NetworkConnection conn, TeamContext context)
        {
            if (teams == null || teams.Count == 0)
            {
                Debug.LogWarning("[SkillBasedStrategy] No teams available");
                return null;
            }

            // Priority 1: Try to join party members' team
            if (context?.HasPartyMembersOnTeams == true)
            {
                var partyTeam = TryGetPartyMembersTeam(teams, context.PartyMembers);
                if (partyTeam != null && !partyTeam.IsFull)
                    return partyTeam;
            }

            // Get player skill rating
            var playerSkill = context?.PlayerSkillRating ?? GetPlayerSkill(conn);

            // Find the team that would result in the most balanced average skill
            return GetMostBalancedTeam(teams, playerSkill);
        }

        private int GetPlayerSkill(NetworkConnection conn)
        {
            if (_skillProvider == null) return 1000; // Default skill
            return _skillProvider.GetSkillRating(conn);
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

        private Team GetMostBalancedTeam(IReadOnlyList<Team> teams, int playerSkill)
        {
            var availableTeams = teams.Where(t => !t.IsFull).ToList();
            if (availableTeams.Count == 0) return null;
            if (availableTeams.Count == 1) return availableTeams[0];

            // Calculate current team skill averages
            var teamSkills = new Dictionary<Team, float>();
            foreach (var team in availableTeams)
            {
                var avgSkill = CalculateTeamAverageSkill(team);
                teamSkills[team] = avgSkill;
            }

            // Find the overall average skill
            var overallAvg = teamSkills.Values.Count > 0
                ? teamSkills.Values.Average()
                : 1000f;

            // Find the team that, after adding this player, would be closest to the overall average
            Team bestTeam = null;
            var bestDiff = float.MaxValue;

            foreach (var team in availableTeams)
            {
                var currentAvg = teamSkills[team];
                var newMemberCount = team.MemberCount + 1;
                var newAvg = (currentAvg * team.MemberCount + playerSkill) / newMemberCount;
                var diff = Mathf.Abs(newAvg - overallAvg);

                // Prefer teams with fewer members when skill difference is similar
                if (diff < bestDiff || (Mathf.Approximately(diff, bestDiff) && team.MemberCount < bestTeam?.MemberCount))
                {
                    bestDiff = diff;
                    bestTeam = team;
                }
            }

            return bestTeam ?? availableTeams.OrderBy(t => t.MemberCount).First();
        }

        private float CalculateTeamAverageSkill(Team team)
        {
            if (team.MemberCount == 0) return 1000f; // Neutral average

            var totalSkill = 0f;
            foreach (var member in team.Members)
            {
                totalSkill += _skillProvider?.GetSkillRating(member.Connection) ?? 1000;
            }
            return totalSkill / team.MemberCount;
        }
    }

    /// <summary>
    /// Interface for providing player skill ratings.
    /// Implement this to integrate with your game's ranking/MMR system.
    /// </summary>
    public interface ISkillProvider
    {
        /// <summary>
        /// Gets the skill rating for a player.
        /// Higher values indicate higher skill.
        /// </summary>
        /// <param name="conn">The player's connection.</param>
        /// <returns>Skill rating (default baseline is 1000).</returns>
        int GetSkillRating(NetworkConnection conn);
    }
}
