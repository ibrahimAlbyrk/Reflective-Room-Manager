using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting
{
    /// <summary>
    /// ScriptableObject configuration for the voting system.
    /// Designer-friendly defaults.
    /// </summary>
    [CreateAssetMenu(fileName = "VoteConfig", menuName = "REFLECTIVE/Vote Config")]
    public class VoteConfig : ScriptableObject
    {
        [Header("Default Vote Settings")]
        [Tooltip("Default vote duration in seconds")]
        public float DefaultDuration = 30f;

        [Tooltip("Default minimum participation rate (0-1)")]
        [Range(0f, 1f)]
        public float DefaultMinParticipation = 0.5f;

        [Tooltip("Default winning threshold (0-1)")]
        [Range(0f, 1f)]
        public float DefaultWinningThreshold = 0.51f;

        [Tooltip("Default tie resolution mode")]
        public TieResolutionMode DefaultTieResolution = TieResolutionMode.Fail;

        [Tooltip("Default allow vote change")]
        public bool DefaultAllowVoteChange = true;

        [Header("Cooldown Settings")]
        [Tooltip("Multiplier for cooldown when vote fails (e.g., 1.5 = 50% longer)")]
        public float FailedVoteCooldownMultiplier = 1.5f;

        [Header("Debug")]
        [Tooltip("Enable debug logging for vote operations")]
        public bool EnableDebugLogs;
    }
}
