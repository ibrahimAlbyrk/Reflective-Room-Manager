using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Service;
    using Structs;
    using Manager;
    using Utilities;

    public abstract class BaseRoomManager : MonoBehaviour
    {
        #region Events

        //Server Side
        public static event Action<RoomInfo> OnServerCreatedRoom;
        public static event Action<NetworkConnectionToClient> OnServerJoinedRoom;
        public static event Action<NetworkConnectionToClient> OnServerExitedRoom;
        public static event Action<NetworkConnection> OnServerDisconnectedRoom;

        //Client Side
        public static event Action OnClientCreatedRoom;
        public static event Action OnClientJoinedRoom;
        public static event Action OnClientRemovedRoom;
        public static event Action OnClientExitedRoom;
        public static event Action OnClientFailedRoom;

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

        [Header("Configuration")]
        [SerializeField] private bool _dontDestroyOnLoad = true;
        
        #endregion
        
        #region Public Variables
        
        /// <summary>The one and only RoomManager</summary>
        public static BaseRoomManager Singleton {
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
                Debug.LogWarning("Multiple RoomManagers detected in the scene. Only one RoomManager can exist at a time.The duplicate RoomManager will be destroyed.");
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
        /// <param name="roomInfo"></param>
        public static void RequestCreateRoom(RoomInfo roomInfo)
        {
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
            var serverRoomMessage =
                new ServerRoomMessage(ServerRoomState.Exit, default, isDisconnected);

            NetworkClient.Send(serverRoomMessage);
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

        #region Recieve Message Methods

        /// <summary>
        /// Gets the connection information of the client connecting to the server
        /// </summary>
        /// <param name="msg"></param>
        private static void OnReceivedConnectionMessageViaClient(ClientConnectionMessage msg)
        {
            RoomClient.ID = msg.ConnectionID;
        }
        
        /// <summary>
        /// This function is triggered by an event from the "client". It performs various operations based on the incoming event.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void OnReceivedRoomMessageViaServer(NetworkConnectionToClient conn,
            ServerRoomMessage msg)
        {
            switch (msg.ServerRoomState)
            {
                case ServerRoomState.Create:
                    CreateRoom(conn, msg.RoomInfo);
                    break;
                case ServerRoomState.Join:
                    JoinRoom(conn, msg.RoomInfo.Name);
                    break;
                case ServerRoomState.Exit:
                    ExitRoom(conn, msg.IsDisconnected);
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// This function is triggered by an event from the "server". It performs various operations based on the incoming event.
        /// </summary>
        /// <param name="msg"></param>
        private static void OnReceivedRoomMessageViaClient(ClientRoomMessage msg)
        {
            switch (msg.ClientRoomState)
            {
                case ClientRoomState.Created:
                    OnClientCreatedRoom?.Invoke();
                    break;
                case ClientRoomState.Joined:
                    OnClientJoinedRoom?.Invoke();
                    break;
                case ClientRoomState.Removed:
                    OnClientRemovedRoom?.Invoke();
                    break;
                case ClientRoomState.Exited:
                    OnClientExitedRoom?.Invoke();
                    break;
                case ClientRoomState.Fail:
                    OnClientFailedRoom?.Invoke();
                    break;
                default:
                    return;
            }
        }

        #endregion

        #region Callback Methods

        private static void SendConnectionMessageToClient(NetworkConnection conn)
        {
            conn.Send(new ClientConnectionMessage
            {
                ConnectionID = conn.connectionId
            });
        }
        
        private void UpdateRoomListForClient(NetworkConnection conn)
        {
            foreach (var message in m_rooms.Select(room => 
                         new RoomListChangeMessage(
                             RoomListUtility.ConvertToRoomList(room),
                             RoomMessageState.Add)))
            {
                conn.Send(message);
            }
        }
        
        private void OnRoomListChangeForClient(RoomListChangeMessage msg)
        {
           switch (msg.State)
            {
                case RoomMessageState.Add:
                    m_roomListInfos.Add(msg.RoomInfo);
                    break;
                case RoomMessageState.Update:
                    var room = m_roomListInfos.FirstOrDefault(info => info.Name == msg.RoomInfo.Name);
                    var index = m_roomListInfos.IndexOf(room);
                    if (index < 0) break;
                    m_roomListInfos[index] = msg.RoomInfo;
                    break;
                case RoomMessageState.Remove:
                    m_roomListInfos.RemoveAll(roomInfo => roomInfo.Name == msg.RoomInfo.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Base Server Methods
        
        protected virtual void OnStartServer()
        {
            NetworkServer.RegisterHandler<ServerRoomMessage>(OnReceivedRoomMessageViaServer);
        }
        
        protected virtual void OnStopServer()
        {
            RoomServer.RemoveAllRoom(forced:true);
        }
        
        protected virtual void OnStartClient()
        {
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnReceivedRoomMessageViaClient);
            NetworkClient.RegisterHandler<RoomListChangeMessage>(OnRoomListChangeForClient);
            NetworkClient.RegisterHandler<ClientConnectionMessage>(OnReceivedConnectionMessageViaClient);
        }

        protected virtual void OnStopClient()
        {
        }
        
        protected virtual void OnServerConnect(NetworkConnection conn)
        {
            UpdateRoomListForClient(conn);

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

        private void Awake()
        {
            if(!InitializeSingleton()) return;

            //Server Side
            NetworkManager.OnStartedServer += OnStartServer;
            NetworkManager.OnStoppedServer += OnStopServer;
            NetworkManager.OnServerConnected += OnServerConnect;
            NetworkManager.OnServerDisconnected += OnServerDisconnect;
            
            //Client Side
            NetworkManager.OnStartedClient += OnStartClient;
            NetworkManager.OnStoppedClient += OnStopClient;
            NetworkManager.OnClientConnected += OnClientConnect;
            NetworkManager.OnClientDisconnected += OnClientDisconnect;
        }

        #endregion
    }
}