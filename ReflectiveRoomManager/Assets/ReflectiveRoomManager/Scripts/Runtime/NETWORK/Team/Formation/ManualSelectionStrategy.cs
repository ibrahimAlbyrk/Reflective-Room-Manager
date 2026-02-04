using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Team.Formation
{
    /// <summary>
    /// Strategy where players choose their own team.
    /// Falls back to least populated team if preferred team is full.
    /// </summary>
    public class ManualSelectionStrategy : ITeamFormationStrategy
    {
        public string StrategyName => "Manual Selection";

        public Team AssignPlayer(IReadOnlyList<Team> teams, NetworkConnection conn, TeamContext context)
        {
            if (teams == null || teams.Count == 0)
            {
                Debug.LogWarning("[ManualSelectionStrategy] No teams available");
                return null;
            }

            // If player has a preference, try to honor it
            if (context?.PreferredTeamID.HasValue == true)
            {
                var preferred = teams.FirstOrDefault(t => t.ID == context.PreferredTeamID.Value);
                if (preferred != null && !preferred.IsFull)
                    return preferred;

                Debug.Log($"[ManualSelectionStrategy] Preferred team {context.PreferredTeamID} unavailable, using fallback");
            }

            // Try to join party members' team if applicable
            if (context?.HasPartyMembersOnTeams == true)
            {
                var partyTeam = TryGetPartyMembersTeam(teams, context.PartyMembers);
                if (partyTeam != null && !partyTeam.IsFull)
                    return partyTeam;
            }

            // Fallback: assign to least populated team
            return GetLeastPopulatedTeam(teams);
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

        private Team GetLeastPopulatedTeam(IReadOnlyList<Team> teams)
        {
            return teams
                .Where(t => !t.IsFull)
                .OrderBy(t => t.MemberCount)
                .FirstOrDefault();
        }
    }
}
