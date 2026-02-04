using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Loader;
    using Events;
    using Roles;
    using Scenes;
    using State;
    using Structs;
    using Identifier;
    using Utilities;
    using Validation;
    using Reconnection;
    using Discovery;
    using Connection.Manager;
    
    public abstract partial class RoomManagerBase
    {
        #region Serialize Variables

        [Header("Configuration")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        [Header("Scene Management")]
        [SerializeField] private LocalPhysicsMode _physicsMode = LocalPhysicsMode.Physics3D;
        [SerializeField] [Scene] private string _lobbyScene;
        [SerializeField] [Scene] private string _roomScene;
        [SerializeField] [Scene] private string _clientContainerScene;
        
        [Header("Room Info")]
        [Tooltip("Maximum number of rooms that can be on the server")]
        [SerializeField] private int _maxRoomCount = 10;
        
        [Tooltip("Maximum number of players a client can specify for a room")]
        [SerializeField] private int _maxPlayerCountPerRoom = 5;

        [Tooltip("determines what type of loading the room will have")]
        [SerializeField] private RoomLoaderType _RoomLoaderType = RoomLoaderType.AdditiveScene;

        [Header("Validation")]
        [Tooltip("Optional custom room validator (must implement IRoomValidator)")]
        [SerializeField] private MonoBehaviour _roomValidatorComponent;
        [Tooltip("Optional custom room access validator (must implement IRoomAccessValidator)")]
        [SerializeField] private MonoBehaviour _roomAccessValidatorComponent;

        [Header("Rate Limiting")]
        [SerializeField] private bool _enableRateLimiting;
        [SerializeField] private int _maxRequestsPerWindow = 5;
        [SerializeField] private float _rateLimitWindowSeconds = 10f;

        [Header("Room Cleanup")]
        [SerializeField] private bool _enableAutoCleanup;
        [SerializeField] private float _emptyRoomTimeoutSeconds = 60f;
        [SerializeField] private float _cleanupCheckIntervalSeconds = 5f;

        [Header("Reconnection")]
        [SerializeField] private bool _enableReconnection;
        [SerializeField] private float _reconnectionGracePeriod = 30f;
        [SerializeField] private MonoBehaviour _playerIdentityProviderComponent;
        [SerializeField] private MonoBehaviour _reconnectionHandlerComponent;
        [SerializeField] private MonoBehaviour _disconnectedPlayerHandlerComponent;
        [SerializeField] private MonoBehaviour _playerStateSerializerComponent;

        [Header("State Machine")]
        [Tooltip("Enable state machine for rooms")]
        [SerializeField] protected bool _enableStateMachine;
        [Tooltip("State machine configuration")]
        [SerializeField] protected RoomStateConfig _stateConfig;
        [Tooltip("State sync frequency in Hz (0 = disabled)")]
        [SerializeField] protected float _stateSyncFrequency = 1f;

        [Header("Role System")]
        [Tooltip("Enable role-based permissions for rooms")]
        [SerializeField] protected bool _enableRoleSystem;
        [Tooltip("Role system configuration")]
        [SerializeField] protected RoomRoleConfig _roleConfig;

        [Header("Room Discovery")]
        [Tooltip("Enable advanced room discovery and filtering")]
        [SerializeField] protected bool _enableRoomDiscovery;
        [Tooltip("Cache TTL in seconds for discovery queries")]
        [SerializeField] protected float _discoveryCacheTTL = 5f;

        #endregion

        #region Public Variables

        /// <summary>The one and only RoomManager</summary>
        public static RoomManagerBase Instance
        {
            get
            {
                if (_singleton == null)
                    Debug.LogWarning("There is no Room Manager Instance");

                return _singleton;
            }
        }

        public RoomEventManager Events => m_eventManager;
        
        public LocalPhysicsMode PhysicsMode => _physicsMode;
        public int MaxRoomCount => _maxRoomCount;
        public int MaxPlayerCountPerRoom => _maxPlayerCountPerRoom;

        public RoomLoaderType RoomLoaderType => _RoomLoaderType;
        
        public string LobbyScene => _lobbyScene;
        public string RoomScene => _roomScene;
        public string ClientContainerScene => _clientContainerScene;
        public bool UseRuntimeContainer => string.IsNullOrEmpty(_clientContainerScene);

        public IRoomValidator RoomValidator
        {
            get
            {
                if (_roomValidatorComponent is IRoomValidator component)
                    return component;
                return _roomValidator;
            }
            set => _roomValidator = value ?? new DefaultRoomValidator();
        }

        public IRoomAccessValidator RoomAccessValidator
        {
            get
            {
                if (_roomAccessValidatorComponent is IRoomAccessValidator component)
                    return component;
                return _roomAccessValidator;
            }
            set => _roomAccessValidator = value ?? new DefaultRoomAccessValidator();
        }

        #endregion

        #region Private Variables
        
        protected RoomEventManager m_eventManager;

        protected readonly List<Room> m_rooms = new();
        
        protected UniqueIdentifier m_uniqueIdentifier;

        private static RoomManagerBase _singleton;
        private readonly List<RoomInfo> _roomListInfos = new();

        private IRoomLoader _roomLoader;

        private RoomSceneSynchronizer _sceneSynchronizer;

        private System.Action _onServerStoppedRemoveAllRoom;

        private System.Action<Mirror.NetworkConnectionToClient, string, string> _onServerJoinRoomHandler;

        private IRoomValidator _roomValidator = new DefaultRoomValidator();

        private IRoomAccessValidator _roomAccessValidator = new DefaultRoomAccessValidator();

        private IConnectionManager _connectionManager;

        private RateLimiter _rateLimiter;

        private RoomCleanupService _cleanupService;

        protected ReconnectionService _reconnectionService;
        public bool EnableReconnection => _enableReconnection;

        protected bool _isShuttingDown;
        private Coroutine _shutdownCoroutine;

        public bool IsShuttingDown => _isShuttingDown;

        // State machine
        public bool EnableStateMachine => _enableStateMachine;
        public RoomStateConfig StateConfig => _stateConfig;
        private float _stateSyncTimer;

        // Role system
        public bool EnableRoleSystem => _enableRoleSystem;
        public RoomRoleConfig RoleConfig => _roleConfig;

        // Room discovery
        public bool EnableRoomDiscovery => _enableRoomDiscovery;
        protected RoomDiscoveryService _discoveryService;
        public RoomDiscoveryService DiscoveryService => _discoveryService;

        #endregion
    }
}