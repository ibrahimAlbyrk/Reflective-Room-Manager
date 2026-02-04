using System.Collections.Generic;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Team.Formation
{
    /// <summary>
    /// Context object providing information for team assignment decisions.
    /// Used by formation strategies to make informed assignments.
    /// </summary>
    public class TeamContext
    {
        /// <summary>
        /// Preferred team ID if any (set by party leader or player preference).
        /// </summary>
        public uint? PreferredTeamID { get; set; }

        /// <summary>
        /// Party ID if the player is in a party.
        /// </summary>
        public uint? PartyID { get; set; }

        /// <summary>
        /// Other party members already assigned to teams.
        /// </summary>
        public List<NetworkConnection> PartyMembers { get; set; }

        /// <summary>
        /// Player's skill rating (for skill-based assignment).
        /// </summary>
        public int PlayerSkillRating { get; set; }

        /// <summary>
        /// Player's display name.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Additional custom data for strategy decisions.
        /// </summary>
        public Dictionary<string, object> CustomData { get; set; }

        public TeamContext()
        {
            PreferredTeamID = null;
            PartyID = null;
            PartyMembers = new List<NetworkConnection>();
            PlayerSkillRating = 0;
            PlayerName = null;
            CustomData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Whether this player is part of a party.
        /// </summary>
        public bool IsInParty => PartyID.HasValue;

        /// <summary>
        /// Whether this player has party members already on teams.
        /// </summary>
        public bool HasPartyMembersOnTeams => PartyMembers != null && PartyMembers.Count > 0;
    }
}
