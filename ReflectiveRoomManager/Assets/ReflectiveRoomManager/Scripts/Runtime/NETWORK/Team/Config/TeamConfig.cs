using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Team.Config
{
    /// <summary>
    /// ScriptableObject configuration for the team system.
    /// Designer-friendly settings for team behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "TeamConfig", menuName = "REFLECTIVE/Team Config")]
    public class TeamConfig : ScriptableObject
    {
        [Header("Team Setup")]
        [Tooltip("Default number of teams per room")]
        [Range(2, 8)]
        public int TeamCount = 2;

        [Tooltip("Default maximum size per team")]
        [Range(1, 32)]
        public int DefaultMaxTeamSize = 10;

        [Tooltip("Default team formation strategy")]
        public TeamFormationMode DefaultFormationMode = TeamFormationMode.AutoDistribution;

        [Header("Team Management")]
        [Tooltip("Allow players to manually request team swaps")]
        public bool AllowManualSwap = true;

        [Tooltip("Enable automatic team balancing")]
        public bool EnableAutoBalance;

        [Tooltip("Score difference threshold to trigger auto-balance")]
        [Range(0, 1000)]
        public int AutoBalanceScoreDiffThreshold = 500;

        [Tooltip("Minimum players needed before balancing applies")]
        [Range(2, 20)]
        public int AutoBalanceMinPlayers = 4;

        [Header("Team Colors")]
        [Tooltip("Default colors for teams (index matches team index)")]
        public Color[] TeamColors = new Color[]
        {
            new Color(1f, 0.2f, 0.2f),    // Red
            new Color(0.2f, 0.4f, 1f),    // Blue
            new Color(0.2f, 0.8f, 0.2f),  // Green
            new Color(1f, 0.8f, 0.2f),    // Yellow
            new Color(0.8f, 0.2f, 0.8f),  // Purple
            new Color(0.2f, 0.8f, 0.8f),  // Cyan
            new Color(1f, 0.5f, 0.2f),    // Orange
            new Color(0.5f, 0.5f, 0.5f)   // Gray
        };

        [Header("Team Names")]
        [Tooltip("Default names for teams (index matches team index)")]
        public string[] TeamNames = new string[]
        {
            "Red Team",
            "Blue Team",
            "Green Team",
            "Yellow Team",
            "Purple Team",
            "Cyan Team",
            "Orange Team",
            "Gray Team"
        };

        [Header("Advanced")]
        [Tooltip("Enable debug logging for team operations")]
        public bool EnableDebugLogs;

        /// <summary>
        /// Gets the color for a team index.
        /// </summary>
        public Color GetTeamColor(int index)
        {
            if (TeamColors == null || TeamColors.Length == 0)
                return Color.white;
            return TeamColors[index % TeamColors.Length];
        }

        /// <summary>
        /// Gets the name for a team index.
        /// </summary>
        public string GetTeamName(int index)
        {
            if (TeamNames == null || TeamNames.Length == 0)
                return $"Team {index + 1}";
            return TeamNames[index % TeamNames.Length];
        }
    }

    /// <summary>
    /// Available team formation strategies.
    /// </summary>
    public enum TeamFormationMode
    {
        /// <summary>
        /// Players choose their own team.
        /// </summary>
        ManualSelection,

        /// <summary>
        /// System automatically distributes players.
        /// </summary>
        AutoDistribution,

        /// <summary>
        /// Team captains draft players.
        /// </summary>
        CaptainPick,

        /// <summary>
        /// Players are assigned based on skill rating.
        /// </summary>
        SkillBased
    }
}
