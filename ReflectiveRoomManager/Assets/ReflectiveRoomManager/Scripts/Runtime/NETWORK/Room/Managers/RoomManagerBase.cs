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
            _sceneSynchronizer = new SceneSynchronizer();

            //SERVER SIDE
            NetworkConnectionHandler.OnStartServer(OnStartServer);
            NetworkConnectionHandler.OnStopServer(OnStopServer);
            
            NetworkConnectionHandler.OnStopServer(() => RemoveAllRoom(true));
            
            NetworkConnectionHandler.OnServerConnect(OnServerConnect);
            NetworkConnectionHandler.OnServerDisconnect(OnServerDisconnect);

            RoomConnectionHandler.OnServerCreateRoom(CreateRoom);
            RoomConnectionHandler.OnServerJoinRoom(JoinRoom);
            RoomConnectionHandler.OnServerExitRoom(ExitRoom);
            
            m_eventManager.OnServerExitedRoom += SendClientExitSceneMessage;
            
            //CLIENT SIDE
            NetworkConnectionHandler.OnStartClient(OnStartClient);
            NetworkConnectionHandler.OnStopClient(OnStopClient);

            NetworkConnectionHandler.OnClientConnect(OnClientConnect);
            NetworkConnectionHandler.OnClientDisconnect(OnClientDisconnect);
            
            RoomConnectionHandler.OnClientRoomListAdd(AddRoomList);
            RoomConnectionHandler.OnClientRoomListUpdate(UpdateRoomList);
            RoomConnectionHandler.OnClientRoomListRemove(RemoveRoomList);
            RoomConnectionHandler.OnClientConnectionMessage(GetConnectionMessageForClient);
        }
    }
}