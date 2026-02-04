using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Team.Messages
{
    /// <summary>
    /// Server-to-Client message synchronizing all team state.
    /// </summary>
    public struct TeamSyncMessage : NetworkMessage
    {
        /// <summary>
        /// All teams data.
        /// </summary>
        public TeamData[] Teams;

        public TeamSyncMessage(TeamData[] teams)
        {
            Teams = teams;
        }
    }

    /// <summary>
    /// Serializable team data for network transmission.
    /// </summary>
    public struct TeamData
    {
        public uint TeamID;
        public string TeamName;
        public Color TeamColor;
        public int MaxSize;
        public TeamMemberData[] Members;
        public TeamStatsData Stats;

        public TeamData(uint teamID, string teamName, Color teamColor, int maxSize,
            TeamMemberData[] members, TeamStatsData stats)
        {
            TeamID = teamID;
            TeamName = teamName;
            TeamColor = teamColor;
            MaxSize = maxSize;
            Members = members;
            Stats = stats;
        }
    }

    /// <summary>
    /// Serializable team statistics for network transmission.
    /// </summary>
    public struct TeamStatsData
    {
        public int TotalScore;
        public int TotalKills;
        public int TotalDeaths;
        public int TotalAssists;

        public TeamStatsData(int totalScore, int totalKills, int totalDeaths, int totalAssists)
        {
            TotalScore = totalScore;
            TotalKills = totalKills;
            TotalDeaths = totalDeaths;
            TotalAssists = totalAssists;
        }
    }
}
