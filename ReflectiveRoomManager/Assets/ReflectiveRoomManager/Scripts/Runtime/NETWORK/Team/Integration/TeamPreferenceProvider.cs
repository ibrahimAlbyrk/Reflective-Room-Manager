using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Team.Integration
{
    /// <summary>
    /// Stores and retrieves team preferences set by party leaders.
    /// Allows party leaders to specify which team their party should join.
    /// </summary>
    public class TeamPreferenceProvider
    {
        private readonly Dictionary<uint, uint> _partyTeamPreferences;

        public TeamPreferenceProvider()
        {
            _partyTeamPreferences = new Dictionary<uint, uint>();
        }

        /// <summary>
        /// Gets the preferred team ID for a party.
        /// </summary>
        /// <param name="partyID">The party ID.</param>
        /// <returns>The preferred team ID, or null if no preference set.</returns>
        public uint? GetPreferredTeamID(uint partyID)
        {
            return _partyTeamPreferences.TryGetValue(partyID, out var teamID) ? teamID : null;
        }

        /// <summary>
        /// Sets the preferred team ID for a party.
        /// </summary>
        /// <param name="partyID">The party ID.</param>
        /// <param name="teamID">The preferred team ID.</param>
        public void SetPreferredTeamID(uint partyID, uint teamID)
        {
            _partyTeamPreferences[partyID] = teamID;
        }

        /// <summary>
        /// Clears the team preference for a party.
        /// </summary>
        /// <param name="partyID">The party ID.</param>
        public void ClearPreference(uint partyID)
        {
            _partyTeamPreferences.Remove(partyID);
        }

        /// <summary>
        /// Checks if a party has a team preference set.
        /// </summary>
        /// <param name="partyID">The party ID.</param>
        /// <returns>True if a preference is set.</returns>
        public bool HasPreference(uint partyID)
        {
            return _partyTeamPreferences.ContainsKey(partyID);
        }

        /// <summary>
        /// Clears all preferences.
        /// </summary>
        public void ClearAll()
        {
            _partyTeamPreferences.Clear();
        }
    }
}
