using UnityEngine;
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
            ReflectiveConnectionManager.networkConnections.OnServerStopped.AddListener(() => RemoveAllRoom(true));
            
            ReflectiveConnectionManager.networkConnections.OnServerConnected.AddListener(OnServerConnect);
            ReflectiveConnectionManager.networkConnections.OnServerDisconnected.AddListener(OnServerDisconnect);

            ReflectiveConnectionManager.roomConnections.OnServerCreateRoom.AddListener(CreateRoom);
            ReflectiveConnectionManager.roomConnections.OnServerJoinRoom.AddListener(JoinRoom);
            ReflectiveConnectionManager.roomConnections.OnServerExitRoom.AddListener(ExitRoom);

            if (RoomLoaderType != RoomLoaderType.NoneScene)
                m_eventManager.OnServerJoinedRoom += RoomSceneSynchronizer.DoSyncScene;
            
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
    }
}
