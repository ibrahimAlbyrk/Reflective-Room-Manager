using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Scenes;
    using Events;
    using Handlers;

    [DisallowMultipleComponent]
    public abstract partial class RoomManagerBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            //INITIALIZE
            if (!InitializeSingleton()) return;

            InitializeRoomLoader();

            m_eventManager = new RoomEventManager();
            
            _networkConnectionHandler = new NetworkConnectionHandler();
            _roomConnectionHandler = new RoomConnectionHandler();

            _sceneSynchronizer = new SceneSynchronizer();

            //SERVER SIDE
            _networkConnectionHandler.OnStartServer(OnStartServer);
            _networkConnectionHandler.OnStopServer(OnStopServer);
            _networkConnectionHandler.OnServerConnect(OnServerConnect);
            _networkConnectionHandler.OnServerDisconnect(OnServerDisconnect);

            _roomConnectionHandler.OnServerCreateRoom(CreateRoom);
            _roomConnectionHandler.OnServerJoinRoom(JoinRoom);
            _roomConnectionHandler.OnServerExitRoom(ExitRoom);

            m_eventManager.OnServerJoinedRoom += SendClientJoinSceneMessage;
            m_eventManager.OnServerExitedRoom += SendClientExitSceneMessage;
            
            //CLIENT SIDE
            _networkConnectionHandler.OnStartClient(OnStartClient);
            _networkConnectionHandler.OnStopClient(OnStopClient);
            _networkConnectionHandler.OnClientConnect(OnClientConnect);
            _networkConnectionHandler.OnClientDisconnect(OnClientDisconnect);
            
            _roomConnectionHandler.OnClientRoomListAdd(AddRoomList);
            _roomConnectionHandler.OnClientRoomListUpdate(UpdateRoomList);
            _roomConnectionHandler.OnClientRoomListRemove(RemoveRoomList);
            _roomConnectionHandler.OnClientConnectionMessage(GetConnectionMessageForClient);
        }
    }
}