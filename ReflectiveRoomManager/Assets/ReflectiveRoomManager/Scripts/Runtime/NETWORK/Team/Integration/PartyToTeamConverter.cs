using System.Linq;
using System.Collections.Generic;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Team.Integration
{
    using Formation;
    using REFLECTIVE.Runtime.NETWORK.Party;

    /// <summary>
    /// Converts party information into team context for assignment.
    /// Bridges the Party system (pre-room) with the Team system (in-room).
    /// </summary>
    public static class PartyToTeamConverter
    {
        /// <summary>
        /// Builds a TeamContext from party information.
        /// </summary>
        /// <param name="partyManager">The party manager to query.</param>
        /// <param name="conn">The connection being assigned.</param>
        /// <param name="teamManager">The team manager (to check existing assignments).</param>
        /// <param name="preferenceProvider">Optional preference provider for team preferences.</param>
        /// <returns>TeamContext with party-aware information.</returns>
        public static TeamContext BuildTeamContext(
            PartyManager partyManager,
            NetworkConnection conn,
            TeamManager teamManager,
            TeamPreferenceProvider preferenceProvider = null)
        {
            var context = new TeamContext();

            if (partyManager == null || conn == null)
                return context;

            // Get player's party
            var party = partyManager.GetPartyByMember(conn);
            if (party == null)
                return context;

            context.PartyID = party.ID;

            // Get party leader's team preference
            if (preferenceProvider != null)
            {
                context.PreferredTeamID = preferenceProvider.GetPreferredTeamID(party.ID);
            }

            // Get party members already assigned to teams
            if (teamManager != null)
            {
                context.PartyMembers = party.Members
                    .Where(m => m.Connection != conn && teamManager.IsOnTeam(m.Connection))
                    .Select(m => m.Connection)
                    .ToList();
            }

            // Get player name from party member
            var partyMember = party.GetMember(conn);
            if (partyMember != null)
            {
                context.PlayerName = partyMember.PlayerName;
            }

            return context;
        }

        /// <summary>
        /// Gets all connections from a party.
        /// </summary>
        /// <param name="partyManager">The party manager to query.</param>
        /// <param name="partyID">The party ID.</param>
        /// <returns>List of connections in the party.</returns>
        public static List<NetworkConnection> GetPartyMembers(PartyManager partyManager, uint partyID)
        {
            if (partyManager == null)
                return new List<NetworkConnection>();

            var party = partyManager.GetParty(partyID);
            return party?.Members.Select(m => m.Connection).ToList() ?? new List<NetworkConnection>();
        }

        /// <summary>
        /// Checks if two connections are in the same party.
        /// </summary>
        public static bool AreInSameParty(PartyManager partyManager, NetworkConnection a, NetworkConnection b)
        {
            if (partyManager == null || a == null || b == null)
                return false;

            var partyA = partyManager.GetPartyByMember(a);
            var partyB = partyManager.GetPartyByMember(b);

            if (partyA == null || partyB == null)
                return false;

            return partyA.ID == partyB.ID;
        }

        /// <summary>
        /// Gets the party ID for a connection, if any.
        /// </summary>
        public static uint? GetPartyID(PartyManager partyManager, NetworkConnection conn)
        {
            return partyManager?.GetPartyIDByMember(conn);
        }
    }
}
