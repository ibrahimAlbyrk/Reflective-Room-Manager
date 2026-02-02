using UnityEngine;
using REFLECTIVE.Runtime.MonoBehavior;
using REFLECTIVE.Runtime.SceneManagement.Manager;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Scenes;
    using Events;
    using Loader;
    using Identifier;
    using Connection.Manager;

    [DisallowMultipleComponent]
    public abstract partial class RoomManagerBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            //INITIALIZE
            if (!InitializeSingleton()) return;

            InitializeRoomLoader();

            ReflectiveSceneManager.Init(RoomLoaderType);

            m_eventManager = new RoomEventManager();

            m_uniqueIdentifier = new UniqueIdentifier();

            //SERVER SIDE
            ReflectiveConnectionManager.networkConnections.OnServerStarted.AddListener(OnStartServer);
            ReflectiveConnectionManager.networkConnections.OnServerStopped.AddListener(OnStopServer);
            _onServerStoppedRemoveAllRoom = () => RemoveAllRoom(true);
            ReflectiveConnectionManager.networkConnections.OnServerStopped.AddListener(_onServerStoppedRemoveAllRoom);
            
            ReflectiveConnectionManager.networkConnections.OnServerConnected.AddListener(OnServerConnect);
            ReflectiveConnectionManager.networkConnections.OnServerDisconnected.AddListener(OnServerDisconnect);

            ReflectiveConnectionManager.roomConnections.OnServerCreateRoom.AddListener(CreateRoom);
            ReflectiveConnectionManager.roomConnections.OnServerJoinRoom.AddListener(JoinRoom);
            ReflectiveConnectionManager.roomConnections.OnServerExitRoom.AddListener(ExitRoom);

            if (RoomLoaderType != RoomLoaderType.NoneScene)
            {
                _sceneSynchronizer = new RoomSceneSynchronizer(this);
                m_eventManager.OnServerJoinedRoom += _sceneSynchronizer.DoSyncScene;
            }
            
            m_eventManager.OnServerJoinedRoom += SendRoomIDToClient;
            m_eventManager.OnServerExitedRoom += SendClientExitSceneMessage;
            m_eventManager.OnServerExitedRoom += SendRoomIDToClientForReset;

            //CLIENT SIDE
            ReflectiveConnectionManager.networkConnections.OnClientStarted.AddListener(OnStartClient);
            ReflectiveConnectionManager.networkConnections.OnClientStopped.AddListener(OnStopClient);
            
            ReflectiveConnectionManager.networkConnections.OnClientConnected.AddListener(OnClientConnect);
            ReflectiveConnectionManager.networkConnections.OnClientDisconnected.AddListener(OnClientDisconnect);
            ReflectiveConnectionManager.networkConnections.OnClientDisconnected.AddListener(RemoveAllRoomList);
            
            ReflectiveConnectionManager.roomConnections.OnClientRoomListAdd.AddListener(AddRoomList);
            ReflectiveConnectionManager.roomConnections.OnClientRoomListUpdate.AddListener(UpdateRoomList);
            ReflectiveConnectionManager.roomConnections.OnClientRoomListRemove.AddListener(RemoveRoomList);

            ReflectiveConnectionManager.roomConnections.OnClientRoomIDMessage.AddListener(GetRoomIDForClient);
        }

        protected virtual void OnDestroy()
        {
            // SERVER SIDE
            ReflectiveConnectionManager.networkConnections.OnServerStarted.RemoveListener(OnStartServer);
            ReflectiveConnectionManager.networkConnections.OnServerStopped.RemoveListener(OnStopServer);
            ReflectiveConnectionManager.networkConnections.OnServerStopped.RemoveListener(_onServerStoppedRemoveAllRoom);

            ReflectiveConnectionManager.networkConnections.OnServerConnected.RemoveListener(OnServerConnect);
            ReflectiveConnectionManager.networkConnections.OnServerDisconnected.RemoveListener(OnServerDisconnect);

            ReflectiveConnectionManager.roomConnections.OnServerCreateRoom.RemoveListener(CreateRoom);
            ReflectiveConnectionManager.roomConnections.OnServerJoinRoom.RemoveListener(JoinRoom);
            ReflectiveConnectionManager.roomConnections.OnServerExitRoom.RemoveListener(ExitRoom);

            if (m_eventManager != null)
            {
                if (_sceneSynchronizer != null)
                    m_eventManager.OnServerJoinedRoom -= _sceneSynchronizer.DoSyncScene;
                m_eventManager.OnServerJoinedRoom -= SendRoomIDToClient;
                m_eventManager.OnServerExitedRoom -= SendClientExitSceneMessage;
                m_eventManager.OnServerExitedRoom -= SendRoomIDToClientForReset;
            }

            // CLIENT SIDE
            ReflectiveConnectionManager.networkConnections.OnClientStarted.RemoveListener(OnStartClient);
            ReflectiveConnectionManager.networkConnections.OnClientStopped.RemoveListener(OnStopClient);

            ReflectiveConnectionManager.networkConnections.OnClientConnected.RemoveListener(OnClientConnect);
            ReflectiveConnectionManager.networkConnections.OnClientDisconnected.RemoveListener(OnClientDisconnect);
            ReflectiveConnectionManager.networkConnections.OnClientDisconnected.RemoveListener(RemoveAllRoomList);

            ReflectiveConnectionManager.roomConnections.OnClientRoomListAdd.RemoveListener(AddRoomList);
            ReflectiveConnectionManager.roomConnections.OnClientRoomListUpdate.RemoveListener(UpdateRoomList);
            ReflectiveConnectionManager.roomConnections.OnClientRoomListRemove.RemoveListener(RemoveRoomList);

            ReflectiveConnectionManager.roomConnections.OnClientRoomIDMessage.RemoveListener(GetRoomIDForClient);

            CoroutineRunner.Cleanup();
        }
    }
}
