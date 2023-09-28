using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Structs;
    using Manager;

    public abstract class REFLECTIVE_BaseRoomManager : MonoBehaviour
    {
        #region Events

        //Server Side
        public static event Action<REFLECTIVE_RoomInfo> OnServerCreatedRoom;
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
        
        protected static void Invoke_OnServerCreatedRoom(REFLECTIVE_RoomInfo roomInfo) => OnServerCreatedRoom?.Invoke(roomInfo);

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
        
        /// <summary>The one and only NetworkManager</summary>
        public static REFLECTIVE_BaseRoomManager singleton { get; internal set; }

        // public bool AllPlayersReady
        // {
        //     get => _allPlayersReady;
        //     set
        //     {
        //         var wasReady = _allPlayersReady;
        //         var nowReady = value;
        //
        //         if (wasReady == nowReady) return;
        //
        //         _allPlayersReady = value;
        //
        //         if (nowReady)
        //             OnRoomServerPlayersReady();
        //         else
        //             OnRoomServerPlayersNotReady();
        //     }
        // }
        //
        // public bool IsStarted { get; protected set; }

        #endregion
        
        #region Private Variables

        private bool _allPlayersReady;
        
        protected List<REFLECTIVE_Room> m_rooms = new();
        private readonly List<REFLECTIVE_RoomInfo> m_roomListInfos = new();

        #endregion

        #region Initialize Methods

        private bool InitializeSingleton()
        {
            if (singleton != null && singleton == this)
                return true;

            if (!_dontDestroyOnLoad)
            {
                singleton = this;
                return true;
            }
            
            if (singleton != null)
            {
                Debug.LogWarning("Multiple RoomManagers detected in the scene. Only one RoomManager can exist at a time. The duplicate RoomManager will be destroyed.");
                Destroy(gameObject);

                // Return false to not allow collision-destroyed second instance to continue.
                return false;
            }
                
            singleton = this;
            if (!Application.isPlaying) return true;
                
            // Force the object to scene root, in case user made it a child of something
            // in the scene since DDOL is only allowed for scene root objects
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            return true;
        }

        #endregion

        #region Get Room Methods

        public List<REFLECTIVE_Room> GetRooms() => m_rooms;

        /// <summary>
        /// The function return information about the room where the "connection" is located
        /// </summary>
        /// <param name="conn"></param>
        /// <returns>Information about the room where the "connection" is located.</returns>
        public REFLECTIVE_Room GetRoomOfPlayer(NetworkConnection conn)
        {
            print(m_rooms.Count);
            
            return m_rooms.FirstOrDefault(room => room.Connections.Any(connection => connection == conn));
        }

        /// <summary>
        /// The function return information about where the *scene* is located
        /// </summary>
        /// <param name="scene"></param>
        /// <returns>Information about the room where the *scene* is located</returns>
        public REFLECTIVE_Room GetRoomOfScene(Scene scene)
        {
            return m_rooms.FirstOrDefault(room => room.Scene == scene);
        }

        #endregion

        #region Request Methods

        /// <summary>
        /// Sends a request to the server for room creation with the specified information
        /// </summary>
        /// <param name="reflectiveRoomInfo"></param>
        public static void RequestCreateRoom(REFLECTIVE_RoomInfo reflectiveRoomInfo)
        {
            var serverRoomMessage =
                new REFLECTIVE_ServerRoomMessage(REFLECTIVE_ServerRoomState.Create, reflectiveRoomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        /// <summary>
        /// Sends a request to join the specified room
        /// </summary>
        /// <param name="roomName"></param>
        public static void RequestJoinRoom(string roomName)
        {
            var roomInfo = new REFLECTIVE_RoomInfo { Name = roomName };

            var serverRoomMessage = new REFLECTIVE_ServerRoomMessage(REFLECTIVE_ServerRoomState.Join, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        /// <summary>
        /// Sends a request to the server to exit the client's room
        /// </summary>
        /// <param name="isDisconnected"></param>
        public static void RequestExitRoom(bool isDisconnected = false)
        {
            var serverRoomMessage =
                new REFLECTIVE_ServerRoomMessage(REFLECTIVE_ServerRoomState.Exit, default, isDisconnected);

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
            REFLECTIVE_RoomInfo roomInfo = default);

        /// <summary>
        /// Joins the client into the room with the specified room' name
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="roomName"></param>
        public abstract void JoinRoom(NetworkConnectionToClient conn, string roomName);

        /// <summary>
        /// It works on the server side. Deletes all rooms and removes all customers from the rooms.
        /// </summary>
        public abstract void RemoveAllRoom();

        /// <summary>
        /// It works on the server side. It deletes the specified Room and removes all customers from the room.
        /// </summary>
        /// <param name="roomName"></param>
        public abstract void RemoveRoom(string roomName);

        /// <summary>
        /// It works on the server side. It performs the process of a client exiting from the server.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="isDisconnected"></param>
        public abstract void ExitRoom(NetworkConnection conn, bool isDisconnected);

        #endregion

        #region Recieve Message Methods

        /// <summary>
        /// This function is triggered by an event from the "client". It performs various operations based on the incoming event.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void OnReceivedRoomMessageViaServer(NetworkConnectionToClient conn,
            REFLECTIVE_ServerRoomMessage msg)
        {
            switch (msg.ServerRoomState)
            {
                case REFLECTIVE_ServerRoomState.Create:
                    CreateRoom(conn, msg.RoomInfo);
                    break;
                case REFLECTIVE_ServerRoomState.Join:
                    JoinRoom(conn, msg.RoomInfo.Name);
                    break;
                case REFLECTIVE_ServerRoomState.Exit:
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
        private static void OnReceivedRoomMessageViaClient(REFLECTIVE_ClientRoomMessage msg)
        {
            switch (msg.ClientRoomState)
            {
                case REFLECTIVE_ClientRoomState.Created:
                    OnClientCreatedRoom?.Invoke();
                    break;
                case REFLECTIVE_ClientRoomState.Joined:
                    OnClientJoinedRoom?.Invoke();
                    break;
                case REFLECTIVE_ClientRoomState.Removed:
                    OnClientRemovedRoom?.Invoke();
                    break;
                case REFLECTIVE_ClientRoomState.Exited:
                    OnClientExitedRoom?.Invoke();
                    break;
                case REFLECTIVE_ClientRoomState.Fail:
                    OnClientFailedRoom?.Invoke();
                    break;
                default:
                    return;
            }
        }

        #endregion

        #region Callback Methods

        // /// <summary>
        // /// This is called on the server when all the players in the room are ready.
        // /// </summary>
        // protected virtual void OnRoomServerPlayersReady() { }
        //
        // /// <summary>
        // /// This is called on the server when CheckReadyToBegin finds that players are not ready
        // /// </summary>
        // protected virtual void OnRoomServerPlayersNotReady() {}
        
        private void OnRoomListChanged(REFLECTIVE_RoomListChangeMessage msg)
        {
           switch (msg.State)
            {
                case REFLECTIVE_RoomMessageState.Add:
                    m_roomListInfos.Add(msg.RoomInfo);
                    break;
                case REFLECTIVE_RoomMessageState.Update:
                    var room = m_roomListInfos.FirstOrDefault(info => info.Name == msg.RoomInfo.Name);
                    var index = m_roomListInfos.IndexOf(room);
                    if (index < 0) break;
                    m_roomListInfos[index] = msg.RoomInfo;
                    break;
                case REFLECTIVE_RoomMessageState.Remove:
                    m_roomListInfos.Remove(msg.RoomInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Base Server Methods
        
        protected virtual void OnStartServer()
        {
            NetworkServer.RegisterHandler<REFLECTIVE_ServerRoomMessage>(OnReceivedRoomMessageViaServer);
        }
        
        protected virtual void OnStartClient()
        {
            NetworkClient.RegisterHandler<REFLECTIVE_ClientRoomMessage>(OnReceivedRoomMessageViaClient);
            NetworkClient.RegisterHandler<REFLECTIVE_RoomListChangeMessage>(OnRoomListChanged);
        }

        #endregion

        #region Base Methods

        private void Awake()
        {
            if(!InitializeSingleton()) return;

            REFLECTIVE_NetworkManager.OnStartedServer += OnStartServer;
            REFLECTIVE_NetworkManager.OnStartedClient += OnStartClient;
        }

        #endregion
    }
}