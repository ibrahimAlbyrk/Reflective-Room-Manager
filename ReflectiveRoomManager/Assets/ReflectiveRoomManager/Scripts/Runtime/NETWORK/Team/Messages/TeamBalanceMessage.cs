using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Team.Messages
{
    /// <summary>
    /// Server-to-Client message notifying of team balance changes.
    /// </summary>
    public struct TeamBalanceMessage : NetworkMessage
    {
        /// <summary>
        /// All assignment changes made during balancing.
        /// </summary>
        public TeamAssignmentChange[] Changes;

        public TeamBalanceMessage(TeamAssignmentChange[] changes)
        {
            Changes = changes;
        }
    }

    /// <summary>
    /// Represents a single team assignment change.
    /// </summary>
    public struct TeamAssignmentChange
    {
        /// <summary>
        /// Connection ID of the player being moved.
        /// </summary>
        public int ConnectionID;

        /// <summary>
        /// Team ID the player is moving from.
        /// </summary>
        public uint FromTeamID;

        /// <summary>
        /// Team ID the player is moving to.
        /// </summary>
        public uint ToTeamID;

        public TeamAssignmentChange(int connectionID, uint fromTeamID, uint toTeamID)
        {
            ConnectionID = connectionID;
            FromTeamID = fromTeamID;
            ToTeamID = toTeamID;
        }
    }
}
