using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Chat
{
    /// <summary>
    /// Configuration settings for the chat system.
    /// </summary>
    [CreateAssetMenu(fileName = "ChatSettings", menuName = "ReflectiveRM/Chat Settings")]
    public class ChatSettings : ScriptableObject
    {
        [Header("Rate Limiting")]
        [Tooltip("Maximum messages allowed within the time window")]
        [SerializeField] private int _maxMessagesPerWindow = 5;
        [Tooltip("Time window in seconds for rate limiting")]
        [SerializeField] private float _rateLimitWindowSeconds = 10f;

        [Header("History")]
        [Tooltip("Maximum messages stored per channel")]
        [SerializeField] private int _maxHistoryPerChannel = 100;

        [Header("Content Limits")]
        [Tooltip("Maximum message content length")]
        [SerializeField] private int _maxContentLength = 512;
        [Tooltip("Maximum player name length")]
        [SerializeField] private int _maxNameLength = 32;

        [Header("Word Filter")]
        [SerializeField] private bool _enableWordFilter = true;
        [SerializeField] private WordFilterMode _wordFilterMode = WordFilterMode.Censor;
        [SerializeField] private List<string> _bannedWords = new();

        [Header("Link Filter")]
        [SerializeField] private bool _enableLinkFilter = true;
        [SerializeField] private List<string> _allowedDomains = new();

        [Header("Spam Filter")]
        [SerializeField] private bool _enableSpamFilter = true;
        [Tooltip("Maximum consecutive repeated characters allowed")]
        [SerializeField] private int _maxRepeatedChars = 5;

        [Header("Duplicate Detection")]
        [Tooltip("Time window for duplicate message detection")]
        [SerializeField] private float _duplicateTimeWindowSeconds = 5f;

        [Header("Global Chat Scope")]
        [SerializeField] private GlobalChatScope _globalChatScope = GlobalChatScope.AllConnected;

        // Public accessors
        public int MaxMessagesPerWindow => _maxMessagesPerWindow;
        public float RateLimitWindowSeconds => _rateLimitWindowSeconds;
        public int MaxHistoryPerChannel => _maxHistoryPerChannel;
        public int MaxContentLength => _maxContentLength;
        public int MaxNameLength => _maxNameLength;

        public bool EnableWordFilter => _enableWordFilter;
        public WordFilterMode WordFilterMode => _wordFilterMode;
        public IReadOnlyList<string> BannedWords => _bannedWords;

        public bool EnableLinkFilter => _enableLinkFilter;
        public IReadOnlyList<string> AllowedDomains => _allowedDomains;

        public bool EnableSpamFilter => _enableSpamFilter;
        public int MaxRepeatedChars => _maxRepeatedChars;

        public float DuplicateTimeWindowSeconds => _duplicateTimeWindowSeconds;
        public GlobalChatScope GlobalChatScope => _globalChatScope;
    }

    /// <summary>
    /// Mode for handling banned words.
    /// </summary>
    public enum WordFilterMode
    {
        Block,   // Reject entire message
        Censor,  // Replace with asterisks
        Remove   // Remove only the word
    }

    /// <summary>
    /// Scope for global chat channel.
    /// </summary>
    public enum GlobalChatScope
    {
        AllConnected, // All connected players
        LobbyOnly,    // Only players not in a room
        InRoomOnly    // Only players in a room
    }
}
