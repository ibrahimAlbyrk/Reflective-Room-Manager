using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Data;
    using Enums;
    using Loader;
    using Events;
    using Service;
    using Structs;
    using Handlers;
    using Utilities;
    using Connection.Manager;

    [DisallowMultipleComponent]
    public abstract class RoomManagerBase : MonoBehaviour
    {
        #region Serialize Variables

        [Header("Configuration")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        [Header("Scene Management")]
        [SerializeField] private LocalPhysicsMode _physicsMode = LocalPhysicsMode.Physics3D;
        [SerializeField] [Scene] private string _lobbyScene;
        [SerializeField] [Scene] private string _roomScene;
        
        [Header("Setup")]
        [SerializeField] private RoomData_SO _defaultRoomData = new (10, 10, RoomLoaderType.AdditiveScene);

        #endregion

        #region Public Variables

        /// <summary>The one and only RoomManager</summary>
        public static RoomManagerBase Singleton
        {
            get
            {
                if (_singleton == null)
                    Debug.LogWarning("There is no Room Manager");

                return _singleton;
            }
        }

        public LocalPhysicsMode PhysicsMode => _physicsMode;
        
        public RoomData_SO RoomData => _defaultRoomData;
        
        public string LobbyScene => _lobbyScene;
        public string RoomScene => _roomScene;
        
        #endregion

        #region Private Variables
        
        protected RoomEventManager m_eventManager;
        private NetworkConnectionHandler _networkConnectionHandler;
        private RoomConnectionHandler _roomConnectionHandler;

        protected List<Room> m_rooms = new();

        private static RoomManagerBase _singleton;
        private readonly List<RoomInfo> _roomListInfos = new();

        private IRoomLoader _roomLoader;

        #endregion

        #region Initialize Methods

        private bool InitializeSingleton()
        {
            if (_singleton != null && _singleton == this)
                return true;

            if (!_dontDestroyOnLoad)
            {
                _singleton = this;
                return true;
            }

            if (_singleton != null)
            {
                Debug.LogWarning(
                    "Multiple RoomManagers detected in the scene. Only one RoomManager can exist at a time.The duplicate RoomManager will be destroyed.");
                Destroy(gameObject);

                // Return false to not allow collision-destroyed second instance to continue.
                return false;
            }

            _singleton = this;
            if (!Application.isPlaying) return true;

            // Force the object to scene root, in case user made it a child of something
            // in the scene since DDOL is only allowed for scene root objects
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            return true;
        }

        private void InitializeRoomLoader()
        {
            _roomLoader = _defaultRoomData.RoomLoaderType switch
            {
                RoomLoaderType.NoneScene => new NoneSceneRoomLoader(),
                RoomLoaderType.AdditiveScene => new SceneRoomLoader(),
                RoomLoaderType.SingleScene => new SceneRoomLoader(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion

        #region Get Room Methods

        /// <summary>
        /// Returns a list of all rooms
        /// </summary>
        /// <remarks>Only works on server</remarks>
        /// <returns></returns>
        public IEnumerable<Room> GetRooms() => m_rooms;

        /// <summary>
        /// Returns a list of all room infos
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <returns></returns>
        public IEnumerable<RoomInfo> GetRoomInfos() => _roomListInfos;

        /// <summary>
        /// The function return information about the room where the "connection" is located
        /// </summary>
        /// <remarks>Only works on server</remarks>
        /// <param name="conn"></param>
        /// <returns>Information about the room where the "connection" is located.</returns>
        public Room GetRoomOfPlayer(NetworkConnection conn)
        {
            return m_rooms.FirstOrDefault(room => room.Connections.Any(connection => connection == conn));
        }

        /// <summary>
        /// The function return information about the room where the "connection ID" is located
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <returns>Information about the room where the "connection ID" is located.</returns>
        public RoomInfo GetRoomOfClient()
        {
            return _roomListInfos.FirstOrDefault(room => room.ConnectionIds.Any(id => id == RoomClient.ID));
        }

        /// <summary>
        /// The function return information about where the scene* is located
        /// </summary>
        /// <remarks>Only works on server</remarks>
        /// <param name="scene"></param>
        /// <returns>Information about the room where the scene* is located</returns>
        public Room GetRoomOfScene(Scene scene)
        {
            return m_rooms.FirstOrDefault(room => room.Scene == scene);
        }

        #endregion

        #region Request Methods

        /// <summary>
        /// Sends a request to the server for room creation with the specified information
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <param name="roomInfo">The <see cref="RoomInfo"/> instance that contains the room's </param>
        public static void RequestCreateRoom(RoomInfo roomInfo)
        {
            if (NetworkClient.connection == null) return;

            var serverRoomMessage =
                new ServerRoomMessage(ServerRoomState.Create, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        /// <summary>
        /// Sends a request to join the specified room
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <param name="roomName"></param>
        public static void RequestJoinRoom(string roomName)
        {
            if (NetworkClient.connection == null) return;

            var roomInfo = new RoomInfo { Name = roomName };

            var serverRoomMessage = new ServerRoomMessage(ServerRoomState.Join, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        /// <summary>
        /// Sends a request to the server to exit the client's room
        /// </summary>
        /// <remarks>Only works on client</remarks>
        /// <param name="isDisconnected"></param>
        public static void RequestExitRoom(bool isDisconnected = false)
        {
            if (NetworkClient.connection == null) return;

            var serverRoomMessage =
                new ServerRoomMessage(ServerRoomState.Exit, default, isDisconnected);

            NetworkClient.Send(serverRoomMessage);
        }

        #endregion

        #region Room Loader Mehods

        protected void LoadRoom(Room room, RoomInfo roomInfo, Action onLoaded = null)
        {
            if (_roomLoader == null)
                throw new NullReferenceException("Room Loader is null");

            _roomLoader.LoadRoom(room, roomInfo, onLoaded);
        }

        protected void UnLoadRoom(Room room)
        {
            if (_roomLoader == null)
                throw new NullReferenceException("Room Loader is null");

            _roomLoader.UnLoadRoom(room);
        }

        #endregion

        #region Room Methods

        /// <summary>
        /// Performs room creation with the room information sent.
        /// If the client's connection information is null, it creates the room as belonging to the server.
        /// If the connection is not null, it creates it as belonging to the client.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="roomInfo"></param>
        public abstract void CreateRoom(NetworkConnection conn = null,
            RoomInfo roomInfo = default);

        /// <summary>
        /// Joins the client into the room with the specified room' name
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="roomName"></param>
        public abstract void JoinRoom(NetworkConnection conn, string roomName);

        /// <summary>
        /// It works on the server side. Deletes all rooms and removes all customers from the rooms.
        /// </summary>
        public abstract void RemoveAllRoom(bool forced = false);

        /// <summary>
        /// It works on the server side. It deletes the specified Room and removes all customers from the room.
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="forced"></param>
        public abstract void RemoveRoom(string roomName, bool forced = false);

        /// <summary>
        /// It works on the server side. It performs the process of a client exiting from the server.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="isDisconnected"></param>
        public abstract void ExitRoom(NetworkConnection conn, bool isDisconnected);

        #endregion

        #region Callback Methods

        private static void GetConnectionMessageForClient(int connectionID) => RoomClient.ID = connectionID;

        private static void SendConnectionMessageToClient(NetworkConnection conn)
        {
            conn.Send(new ClientConnectionMessage
            {
                ConnectionID = conn.connectionId
            });
        }

        private void SendUpdateRoomListForClient(NetworkConnection conn)
        {
            foreach (var message in m_rooms.Select(room =>
                         new RoomListChangeMessage(
                             RoomListUtility.ConvertToRoomList(room),
                             RoomMessageState.Add)))
            {
                conn.Send(message);
            }
        }
        
        private void SendClientJoinSceneMessage(NetworkConnection conn)
        {
            conn.Send(new SceneMessage{sceneName = _roomScene, sceneOperation = SceneOperation.Normal});
        }
        
        private void SendClientExitSceneMessage(NetworkConnection conn)
        {
            conn.Send(new SceneMessage{sceneName = _lobbyScene, sceneOperation = SceneOperation.Normal});
        }

        private void AddRoomList(RoomInfo roomInfo)
        {
            _roomListInfos.Add(roomInfo);
        }

        private void UpdateRoomList(RoomInfo roomInfo)
        {
            var room = _roomListInfos.FirstOrDefault(info => info.Name == roomInfo.Name);

            var index = _roomListInfos.IndexOf(room);

            if (index < 0) return;

            _roomListInfos[index] = roomInfo;
        }

        private void RemoveRoomList(RoomInfo roomInfo)
        {
            _roomListInfos.RemoveAll(info => info.Name == roomInfo.Name);
        }

        #endregion

        #region Base Server Methods

        protected virtual void OnStartServer()
        {
            ConnectionManager.roomConnections.AddRegistersForServer();
        }

        protected virtual void OnStopServer()
        {
            RemoveAllRoom(forced:true);
        }

        protected virtual void OnStartClient()
        {
            ConnectionManager.roomConnections.AddRegistersForClient();
        }

        protected virtual void OnStopClient()
        {
        }

        protected virtual void OnServerConnect(NetworkConnection conn)
        {
            SendUpdateRoomListForClient(conn);

            SendConnectionMessageToClient(conn);
        }

        protected virtual void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            ExitRoom(conn, true);
        }

        protected virtual void OnClientConnect()
        {
        }

        protected virtual void OnClientDisconnect()
        {
        }

        #endregion

        #region Base Methods

        protected virtual void Awake()
        {
            if (!InitializeSingleton()) return;

            InitializeRoomLoader();

            m_eventManager = new RoomEventManager();
            _networkConnectionHandler = new NetworkConnectionHandler();
            _roomConnectionHandler = new RoomConnectionHandler();

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

        #endregion
    }
}