using System;

namespace REFLECTIVE.Runtime.NETWORK.Team
{
    /// <summary>
    /// Tracks aggregated statistics for a team.
    /// </summary>
    [Serializable]
    public class TeamStats
    {
        /// <summary>
        /// Total score accumulated by the team.
        /// </summary>
        public int TotalScore { get; private set; }

        /// <summary>
        /// Total kills by team members.
        /// </summary>
        public int TotalKills { get; private set; }

        /// <summary>
        /// Total deaths of team members.
        /// </summary>
        public int TotalDeaths { get; private set; }

        /// <summary>
        /// Total assists by team members.
        /// </summary>
        public int TotalAssists { get; private set; }

        /// <summary>
        /// Average score per team member.
        /// </summary>
        public float AverageScore(int memberCount)
        {
            return memberCount > 0 ? (float)TotalScore / memberCount : 0f;
        }

        /// <summary>
        /// Kill/Death ratio for the team.
        /// </summary>
        public float KDRatio => TotalDeaths > 0 ? (float)TotalKills / TotalDeaths : TotalKills;

        /// <summary>
        /// Adds a kill to the team stats.
        /// </summary>
        public void AddKill()
        {
            TotalKills++;
        }

        /// <summary>
        /// Adds a death to the team stats.
        /// </summary>
        public void AddDeath()
        {
            TotalDeaths++;
        }

        /// <summary>
        /// Adds an assist to the team stats.
        /// </summary>
        public void AddAssist()
        {
            TotalAssists++;
        }

        /// <summary>
        /// Adds points to the team score.
        /// </summary>
        public void AddScore(int points)
        {
            TotalScore += points;
        }

        /// <summary>
        /// Sets the total score directly.
        /// </summary>
        public void SetScore(int score)
        {
            TotalScore = score;
        }

        /// <summary>
        /// Resets all statistics to zero.
        /// </summary>
        public void Reset()
        {
            TotalScore = 0;
            TotalKills = 0;
            TotalDeaths = 0;
            TotalAssists = 0;
        }

        /// <summary>
        /// Creates a copy of these stats.
        /// </summary>
        public TeamStats Clone()
        {
            return new TeamStats
            {
                TotalScore = TotalScore,
                TotalKills = TotalKills,
                TotalDeaths = TotalDeaths,
                TotalAssists = TotalAssists
            };
        }
    }
}
