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
    using Service;
    using Structs;
    using Utilities;
    using Connection.Manager;

    public abstract class BaseRoomManager : MonoBehaviour
    {
        #region Events

        //Server Side
        public static event Action<RoomInfo> OnServerCreatedRoom;
        public static event Action<NetworkConnectionToClient> OnServerJoinedRoom;
        public static event Action<NetworkConnectionToClient> OnServerExitedRoom;
        public static event Action<NetworkConnection> OnServerDisconnectedRoom;

        #endregion

        #region Event Caller Methods

        protected static void Invoke_OnServerCreatedRoom(RoomInfo roomInfo) =>
            OnServerCreatedRoom?.Invoke(roomInfo);

        protected static void Invoke_OnServerJoinedClient(NetworkConnectionToClient conn) =>
            OnServerJoinedRoom?.Invoke(conn);

        protected static void Invoke_OnServerExitedClient(NetworkConnectionToClient conn) =>
            OnServerExitedRoom?.Invoke(conn);

        protected static void Invoke_OnServerDisconnectedClient(NetworkConnection conn) =>
            OnServerDisconnectedRoom?.Invoke(conn);

        #endregion

        #region Serialize Variables

        [Header("Configuration")] [SerializeField]
        private bool _dontDestroyOnLoad = true;

        [Header("Setup")] [SerializeField] private RoomManagementData_SO _roomManagementData;

        #endregion

        #region Public Variables

        /// <summary>The one and only RoomManager</summary>
        public static BaseRoomManager Singleton
        {
            get
            {
                if (_singleton == null)
                    Debug.LogWarning("There is no Room Manager");

                return _singleton;
            }
        }

        #endregion

        #region Private Variables

        protected List<Room> m_rooms = new();

        private static BaseRoomManager _singleton;
        private readonly List<RoomInfo> m_roomListInfos = new();

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
            var roomData = _roomManagementData.DefaultRoomData;

            _roomLoader = roomData.RoomLoaderType switch
            {
                RoomLoaderType.Empty => new EmptyRoomLoader(),
                RoomLoaderType.AdditiveScene => new AdditiveSceneRoomLoader(),
                RoomLoaderType.SingleScene => new SingleSceneRoomLoader(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion

        #region Get Methods

        public RoomData_SO GetRoomData()
        {
            return _roomManagementData.DefaultRoomData;
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
        public IEnumerable<RoomInfo> GetRoomInfos() => m_roomListInfos;

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
            return m_roomListInfos.FirstOrDefault(room => room.ConnectionIds.Any(id => id == RoomClient.ID));
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
        public abstract void JoinRoom(NetworkConnectionToClient conn, string roomName);

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

        private void AddRoomList(RoomInfo roomInfo)
        {
            m_roomListInfos.Add(roomInfo);
        }

        private void UpdateRoomList(RoomInfo roomInfo)
        {
            var room = m_roomListInfos.FirstOrDefault(info => info.Name == roomInfo.Name);

            var index = m_roomListInfos.IndexOf(room);

            if (index < 0) return;

            m_roomListInfos[index] = roomInfo;
        }

        private void RemoveRoomList(RoomInfo roomInfo)
        {
            m_roomListInfos.RemoveAll(info => info.Name == roomInfo.Name);
        }

        #endregion

        #region Base Server Methods

        protected virtual void OnStartServer()
        {
            ConnectionManager.roomConnections.AddRegistersForServer();
        }

        protected virtual void OnStopServer()
        {
            RoomServer.RemoveAllRoom(forced: true);
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
        }

        protected virtual void OnClientConnect()
        {
        }

        protected virtual void OnClientDisconnect()
        {
            RoomClient.ExitRoom();
        }

        #endregion

        #region Base Methods

        protected virtual void Awake()
        {
            if (!InitializeSingleton()) return;

            InitializeRoomLoader();

            //SERVER SIDE
            ConnectionManager.networkConnections.OnStartedServer += OnStartServer;
            ConnectionManager.networkConnections.OnStoppedServer += OnStopServer;
            ConnectionManager.networkConnections.OnServerConnected += OnServerConnect;
            ConnectionManager.networkConnections.OnServerDisconnected += OnServerDisconnect;

            ConnectionManager.roomConnections.OnServerCreateRoom += CreateRoom;
            ConnectionManager.roomConnections.OnServerJoinRoom += JoinRoom;
            ConnectionManager.roomConnections.OnServerExitRoom += ExitRoom;

            //CLIENT SIDE
            ConnectionManager.networkConnections.OnStartedClient += OnStartClient;
            ConnectionManager.networkConnections.OnStoppedClient += OnStopClient;
            ConnectionManager.networkConnections.OnClientConnected += OnClientConnect;
            ConnectionManager.networkConnections.OnClientDisconnected += OnClientDisconnect;

            ConnectionManager.roomConnections.OnClientRoomListAdd += AddRoomList;
            ConnectionManager.roomConnections.OnClientRoomListUpdate += UpdateRoomList;
            ConnectionManager.roomConnections.OnClientRoomListRemove += RemoveRoomList;
            ConnectionManager.roomConnections.OnClientConnectionMessage += GetConnectionMessageForClient;
        }

        #endregion
    }
}