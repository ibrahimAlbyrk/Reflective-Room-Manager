using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Chat
{
    using Structs;
    using Messages;
    using Filters;
    using History;
    using Room;

    /// <summary>
    /// Main chat manager handling server-side message routing and client-side API.
    /// Attach to a NetworkManager or persistent game object.
    /// </summary>
    [DisallowMultipleComponent]
    public partial class ChatManager : MonoBehaviour
    {
        private static ChatManager _instance;
        public static ChatManager Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private ChatSettings _settings;

#if REFLECTIVE_SERVER
        // Server-side components
        private MessageHistory _serverHistory;
        private MuteManager _muteManager;
        private ChatRateLimiter _rateLimiter;
        private List<IChatFilter> _filters;
        private Dictionary<uint, string> _playerNames;
#endif

#if REFLECTIVE_CLIENT
        // Client-side components
        private Dictionary<ChatChannel, List<ChatMessage>> _clientHistory;
#endif

        // Events
        public event Action<ChatMessage> OnMessageReceived;
        public event Action<string, ChatErrorCode> OnChatError;

        public ChatSettings Settings => _settings;

        /// <summary>
        /// Sets the chat settings. Use when creating ChatManager at runtime.
        /// </summary>
        public void SetSettings(ChatSettings settings)
        {
            _settings = settings;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[ChatManager] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            UnregisterHandlers();
        }

        /// <summary>
        /// Initializes the chat system. Call after network starts.
        /// </summary>
        public void Initialize()
        {
            if (_settings == null)
            {
                Debug.LogError("[ChatManager] ChatSettings not assigned! Call SetSettings() first.");
                return;
            }

#if REFLECTIVE_SERVER
            if (NetworkServer.active)
                InitializeServer();
#endif

#if REFLECTIVE_CLIENT
            if (NetworkClient.active)
                InitializeClient();
#endif
        }

#if REFLECTIVE_SERVER
        private void InitializeServer()
        {
            _serverHistory = new MessageHistory(_settings.MaxHistoryPerChannel);
            _muteManager = new MuteManager();
            _rateLimiter = new ChatRateLimiter(_settings.MaxMessagesPerWindow, _settings.RateLimitWindowSeconds);
            _playerNames = new Dictionary<uint, string>();

            InitializeFilters();
            RegisterServerHandlers();

            Debug.Log("[ChatManager] Server initialized.");
        }

        private void InitializeFilters()
        {
            _filters = new List<IChatFilter>();

            if (_settings.EnableWordFilter && _settings.BannedWords.Count > 0)
                _filters.Add(new WordFilter(_settings.BannedWords, _settings.WordFilterMode));

            if (_settings.EnableLinkFilter)
                _filters.Add(new LinkFilter(_settings.AllowedDomains));

            if (_settings.EnableSpamFilter)
                _filters.Add(new SpamFilter(_settings.MaxRepeatedChars));
        }

        private void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<ChatRequestMessage>(OnServerChatRequest);
            NetworkServer.RegisterHandler<ChatHistoryRequestMessage>(OnServerHistoryRequest);
        }
#endif

#if REFLECTIVE_CLIENT
        private void InitializeClient()
        {
            _clientHistory = new Dictionary<ChatChannel, List<ChatMessage>>();
            RegisterClientHandlers();

            Debug.Log("[ChatManager] Client initialized.");
        }

        private void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<ChatBroadcastMessage>(OnClientChatBroadcast);
            NetworkClient.RegisterHandler<ChatErrorMessage>(OnClientChatError);
            NetworkClient.RegisterHandler<ChatHistoryResponseMessage>(OnClientHistoryResponse);
        }
#endif

        private void UnregisterHandlers()
        {
#if REFLECTIVE_SERVER
            if (NetworkServer.active)
            {
                NetworkServer.UnregisterHandler<ChatRequestMessage>();
                NetworkServer.UnregisterHandler<ChatHistoryRequestMessage>();
            }
#endif

#if REFLECTIVE_CLIENT
            if (NetworkClient.active)
            {
                NetworkClient.UnregisterHandler<ChatBroadcastMessage>();
                NetworkClient.UnregisterHandler<ChatErrorMessage>();
                NetworkClient.UnregisterHandler<ChatHistoryResponseMessage>();
            }
#endif
        }

#if REFLECTIVE_SERVER
        /// <summary>
        /// Cleans up server resources. Call when server stops.
        /// </summary>
        public void CleanupServer()
        {
            _serverHistory?.ClearAll();
            _muteManager?.Clear();
            _rateLimiter?.Clear();
            _playerNames?.Clear();
            _filters?.Clear();

            Debug.Log("[ChatManager] Server cleanup complete.");
        }
#endif

#if REFLECTIVE_CLIENT
        /// <summary>
        /// Cleans up client resources. Call when client disconnects.
        /// </summary>
        public void CleanupClient()
        {
            _clientHistory?.Clear();

            Debug.Log("[ChatManager] Client cleanup complete.");
        }
#endif
    }
}
