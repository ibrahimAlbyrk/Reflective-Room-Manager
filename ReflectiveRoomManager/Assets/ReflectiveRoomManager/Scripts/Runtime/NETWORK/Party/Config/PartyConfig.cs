using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Party.Config
{
    /// <summary>
    /// ScriptableObject configuration for the party system.
    /// Designer-friendly settings for party behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "PartyConfig", menuName = "REFLECTIVE/Party Config")]
    public class PartyConfig : ScriptableObject
    {
        [Header("Party Size")]
        [Tooltip("Default maximum party size when creating a new party")]
        [Range(2, 20)]
        public int DefaultMaxSize = 4;

        [Tooltip("Absolute maximum party size allowed")]
        [Range(2, 20)]
        public int MaxPartySize = 10;

        [Header("Invite Settings")]
        [Tooltip("Time in seconds before an invite expires")]
        [Range(10, 300)]
        public int InviteTimeoutSeconds = 60;

        [Header("Leadership")]
        [Tooltip("Automatically transfer leadership when leader leaves/disconnects")]
        public bool EnableAutoLeaderTransfer = true;

        [Header("Custom Data")]
        [Tooltip("Allow parties to store custom key-value data")]
        public bool EnableCustomData = true;

        [Tooltip("Maximum number of custom data entries per party")]
        [Range(0, 50)]
        public int MaxCustomDataEntries = 20;

        [Header("Advanced")]
        [Tooltip("Enable debug logging for party operations")]
        public bool EnableDebugLogs;
    }
}
