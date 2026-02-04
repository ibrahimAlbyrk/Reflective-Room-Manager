using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Team.Messages
{
    /// <summary>
    /// Server-to-Client message notifying a player of their team assignment.
    /// </summary>
    public struct TeamAssignmentMessage : NetworkMessage
    {
        /// <summary>
        /// Team ID the player is assigned to.
        /// </summary>
        public uint TeamID;

        /// <summary>
        /// Team name.
        /// </summary>
        public string TeamName;

        /// <summary>
        /// Team color.
        /// </summary>
        public Color TeamColor;

        /// <summary>
        /// Current team members.
        /// </summary>
        public TeamMemberData[] Members;

        public TeamAssignmentMessage(uint teamID, string teamName, Color teamColor, TeamMemberData[] members)
        {
            TeamID = teamID;
            TeamName = teamName;
            TeamColor = teamColor;
            Members = members;
        }
    }

    /// <summary>
    /// Serializable team member data for network transmission.
    /// </summary>
    public struct TeamMemberData
    {
        public int ConnectionID;
        public string PlayerName;
        public int Kills;
        public int Deaths;
        public int Assists;
        public int Score;

        public TeamMemberData(int connectionID, string playerName, int kills, int deaths, int assists, int score)
        {
            ConnectionID = connectionID;
            PlayerName = playerName;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Score = score;
        }
    }
}
