using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.MonoBehavior;
using REFLECTIVE.Runtime.SceneManagement.Manager;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Scenes;
    using Events;
    using Loader;
    using Identifier;
    using Utilities;
    using Reconnection;
    using Reconnection.Messages;
    using Connection.Manager;

    [DisallowMultipleComponent]
    public abstract partial class RoomManagerBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            //INITIALIZE
            if (!InitializeSingleton()) return;

            _connectionManager = ReflectiveConnectionManager.Instance;

            if (_enableRateLimiting)
            {
                _rateLimiter = new RateLimiter(_maxRequestsPerWindow, _rateLimitWindowSeconds);
                _connectionManager.RoomConnections.SetRateLimiter(_rateLimiter);
            }

            InitializeRoomLoader();

            ReflectiveSceneManager.Init(RoomLoaderType);

            m_eventManager = new RoomEventManager();

            m_uniqueIdentifier = new UniqueIdentifier();

            //SERVER SIDE
            _connectionManager.NetworkConnections.OnServerStarted.AddListener(OnStartServer);
            _connectionManager.NetworkConnections.OnServerStopped.AddListener(OnStopServer);
            _onServerStoppedRemoveAllRoom = () => RemoveAllRoom(true);
            _connectionManager.NetworkConnections.OnServerStopped.AddListener(_onServerStoppedRemoveAllRoom);
            
            _connectionManager.NetworkConnections.OnServerConnected.AddListener(OnServerConnect);
            _connectionManager.NetworkConnections.OnServerDisconnected.AddListener(OnServerDisconnect);

            _connectionManager.RoomConnections.OnServerCreateRoom.AddListener(CreateRoom);
            _onServerJoinRoomHandler = (conn, roomName, accessToken) => JoinRoom(conn, roomName, accessToken);
            _connectionManager.RoomConnections.OnServerJoinRoom.AddListener(_onServerJoinRoomHandler);
            _connectionManager.RoomConnections.OnServerExitRoom.AddListener(ExitRoom);

            if (RoomLoaderType != RoomLoaderType.NoneScene)
            {
                _sceneSynchronizer = new RoomSceneSynchronizer(this);
                m_eventManager.OnServerJoinedRoom += _sceneSynchronizer.DoSyncScene;
            }
            
            m_eventManager.OnServerJoinedRoom += SendRoomIDToClient;
            m_eventManager.OnServerExitedRoom += SendClientExitSceneMessage;
            m_eventManager.OnServerExitedRoom += SendRoomIDToClientForReset;

            //CLIENT SIDE
            _connectionManager.NetworkConnections.OnClientStarted.AddListener(OnStartClient);
            _connectionManager.NetworkConnections.OnClientStopped.AddListener(OnStopClient);
            
            _connectionManager.NetworkConnections.OnClientConnected.AddListener(OnClientConnect);
            _connectionManager.NetworkConnections.OnClientDisconnected.AddListener(OnClientDisconnect);
            _connectionManager.NetworkConnections.OnClientDisconnected.AddListener(RemoveAllRoomList);
            
            _connectionManager.RoomConnections.OnClientRoomListAdd.AddListener(AddRoomList);
            _connectionManager.RoomConnections.OnClientRoomListUpdate.AddListener(UpdateRoomList);
            _connectionManager.RoomConnections.OnClientRoomListRemove.AddListener(RemoveRoomList);

            _connectionManager.RoomConnections.OnClientRoomIDMessage.AddListener(GetRoomIDForClient);

            // Room cleanup initialization
            if (_enableAutoCleanup)
                _cleanupService = new RoomCleanupService(_emptyRoomTimeoutSeconds, _cleanupCheckIntervalSeconds);

            // Reconnection initialization
            if (_enableReconnection)
                InitializeReconnection();
        }

        protected virtual void Update()
        {
            _cleanupService?.Update(m_rooms, room => RemoveRoom(room, forced: true));
        }

        private void InitializeReconnection()
        {
            var identityProvider = _playerIdentityProviderComponent != null
                ? _playerIdentityProviderComponent as IPlayerIdentityProvider
                : gameObject.AddComponent<GuidPlayerIdentityProvider>();

            IReconnectionHandler reconnectionHandler;
            if (_reconnectionHandlerComponent != null)
            {
                reconnectionHandler = _reconnectionHandlerComponent as IReconnectionHandler;
            }
            else
            {
                var defaultHandler = gameObject.AddComponent<DefaultReconnectionHandler>();
                defaultHandler.SetGracePeriod(_reconnectionGracePeriod);
                reconnectionHandler = defaultHandler;
            }

            var playerHandler = _disconnectedPlayerHandlerComponent != null
                ? _disconnectedPlayerHandlerComponent as IDisconnectedPlayerHandler
                : gameObject.AddComponent<DefaultDisconnectedPlayerHandler>();

            var stateSerializer = _playerStateSerializerComponent != null
                ? _playerStateSerializerComponent as IPlayerStateSerializer
                : null;

            _reconnectionService = gameObject.AddComponent<ReconnectionService>();
            _reconnectionService.Initialize(reconnectionHandler, stateSerializer, playerHandler, identityProvider, m_eventManager);
        }

        private void OnPlayerIdentityMessageReceived(NetworkConnectionToClient conn, PlayerIdentityMessage msg)
        {
            if (_reconnectionService == null) return;

            var identityProvider = _playerIdentityProviderComponent != null
                ? _playerIdentityProviderComponent as IPlayerIdentityProvider
                : GetComponent<GuidPlayerIdentityProvider>() as IPlayerIdentityProvider;

            if (identityProvider == null) return;

            // Check if this is a reconnection attempt
            if (!string.IsNullOrEmpty(msg.PlayerId) && _reconnectionService.HasPendingReconnection(msg.PlayerId))
            {
                var playerId = identityProvider.GetOrAssignPlayerId(conn, msg.PlayerId);

                if (_reconnectionService.TryReconnect(playerId, conn))
                    return;
            }

            // Normal flow: assign new or existing ID
            var assignedId = identityProvider.GetOrAssignPlayerId(conn, msg.PlayerId);
            conn.Send(new PlayerIdentityResponseMessage { PlayerId = assignedId });
        }

        protected virtual void OnDestroy()
        {
            // SERVER SIDE
            _connectionManager.NetworkConnections.OnServerStarted.RemoveListener(OnStartServer);
            _connectionManager.NetworkConnections.OnServerStopped.RemoveListener(OnStopServer);
            _connectionManager.NetworkConnections.OnServerStopped.RemoveListener(_onServerStoppedRemoveAllRoom);

            _connectionManager.NetworkConnections.OnServerConnected.RemoveListener(OnServerConnect);
            _connectionManager.NetworkConnections.OnServerDisconnected.RemoveListener(OnServerDisconnect);

            _connectionManager.RoomConnections.OnServerCreateRoom.RemoveListener(CreateRoom);
            _connectionManager.RoomConnections.OnServerJoinRoom.RemoveListener(_onServerJoinRoomHandler);
            _connectionManager.RoomConnections.OnServerExitRoom.RemoveListener(ExitRoom);

            if (m_eventManager != null)
            {
                if (_sceneSynchronizer != null)
                    m_eventManager.OnServerJoinedRoom -= _sceneSynchronizer.DoSyncScene;
                m_eventManager.OnServerJoinedRoom -= SendRoomIDToClient;
                m_eventManager.OnServerExitedRoom -= SendClientExitSceneMessage;
                m_eventManager.OnServerExitedRoom -= SendRoomIDToClientForReset;
            }

            // CLIENT SIDE
            _connectionManager.NetworkConnections.OnClientStarted.RemoveListener(OnStartClient);
            _connectionManager.NetworkConnections.OnClientStopped.RemoveListener(OnStopClient);

            _connectionManager.NetworkConnections.OnClientConnected.RemoveListener(OnClientConnect);
            _connectionManager.NetworkConnections.OnClientDisconnected.RemoveListener(OnClientDisconnect);
            _connectionManager.NetworkConnections.OnClientDisconnected.RemoveListener(RemoveAllRoomList);

            _connectionManager.RoomConnections.OnClientRoomListAdd.RemoveListener(AddRoomList);
            _connectionManager.RoomConnections.OnClientRoomListUpdate.RemoveListener(UpdateRoomList);
            _connectionManager.RoomConnections.OnClientRoomListRemove.RemoveListener(RemoveRoomList);

            _connectionManager.RoomConnections.OnClientRoomIDMessage.RemoveListener(GetRoomIDForClient);

            CoroutineRunner.Cleanup();
        }
    }
}
