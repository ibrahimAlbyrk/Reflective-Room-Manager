using Mirror;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Enums;
    using Structs;
    using Behaviour;

    public abstract class REFLECTIVE_BaseRoomManager : REFLECTIVE_NetBehaviour
    {
        //TODO: To be revised
        #region Singleton

        private static readonly object padlock = new();

        private static REFLECTIVE_BaseRoomManager instance;

        public static REFLECTIVE_BaseRoomManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<REFLECTIVE_BaseRoomManager>();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Events

        //Server Side
        public static event System.Action<REFLECTIVE_RoomListInfo, REFLECTIVE_RoomListInfo> OnServerCreatedRoom;
        public static event System.Action<NetworkConnectionToClient> OnServerJoinedRoom;
        public static event System.Action<NetworkConnectionToClient> OnServerExitedRoom;
        public static event System.Action<NetworkConnection> OnServerDisconnectedRoom;

        //Client Side
        public static event System.Action OnClientCreatedRoom;
        public static event System.Action OnClientJoinedRoom;
        public static event System.Action OnClientRemovedRoom;
        public static event System.Action OnClientExitedRoom;
        public static event System.Action OnClientFailedRoom;

        #endregion

        #region Event Caller Methods
        
        protected static void Invoke_OnServerCreatedRoom(REFLECTIVE_RoomListInfo oldRoomInfo,
            REFLECTIVE_RoomListInfo newRoomInfo) => OnServerCreatedRoom?.Invoke(oldRoomInfo, newRoomInfo);

        protected static void Invoke_OnServerJoinedClient(NetworkConnectionToClient conn) =>
            OnServerJoinedRoom?.Invoke(conn);

        protected static void Invoke_OnServerExitedClient(NetworkConnectionToClient conn) =>
            OnServerExitedRoom?.Invoke(conn);

        protected static void Invoke_OnServerDisconnectedClient(NetworkConnection conn) =>
            OnServerDisconnectedRoom?.Invoke(conn);

        #endregion

        #region Private Variables

        protected readonly List<REFLECTIVE_Room> m_rooms = new();

        protected readonly SyncList<REFLECTIVE_RoomListInfo> m_roomInfos = new();

        #endregion

        #region Get Room Methods

        public List<REFLECTIVE_RoomListInfo> GetRooms() => m_roomInfos.ToList();

        /// <summary>
        /// The function return information about the room where the "connection" is located
        /// </summary>
        /// <param name="conn"></param>
        /// <returns>Information about the room where the "connection" is located.</returns>
        public REFLECTIVE_Room GetRoomOfPlayer(NetworkConnection conn)
        {
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

        [ClientCallback]
        public static void RequestCreateRoom(REFLECTIVE_RoomInfo reflectiveRoomInfo)
        {
            var serverRoomMessage =
                new REFLECTIVE_ServerRoomMessage(REFLECTIVE_ServerRoomState.Create, reflectiveRoomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        [ClientCallback]
        public static void RequestJoinRoom(string roomName)
        {
            var roomInfo = new REFLECTIVE_RoomInfo { Name = roomName };

            var serverRoomMessage = new REFLECTIVE_ServerRoomMessage(REFLECTIVE_ServerRoomState.Join, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        [ClientCallback]
        public static void RequestExitRoom(bool isDisconnected = false)
        {
            var serverRoomMessage =
                new REFLECTIVE_ServerRoomMessage(REFLECTIVE_ServerRoomState.Exit, default, isDisconnected);

            NetworkClient.Send(serverRoomMessage);
        }

        #endregion

        #region Room Methods

        [ServerCallback]
        public abstract void CreateRoom(NetworkConnection conn = null,
            REFLECTIVE_RoomInfo reflectiveRoomInfo = default);

        [ServerCallback]
        public abstract void JoinRoom(NetworkConnectionToClient conn, string roomName);

        [ServerCallback]
        public abstract void RemoveAllRoom();

        [ServerCallback]
        public abstract void RemoveRoom(string roomName);

        [ServerCallback]
        public abstract void ExitRoom(NetworkConnection conn, bool isDisconnected);

        #endregion

        #region Recieve Message Methods

        [ServerCallback]
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

        [ClientCallback]
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

        private static void OnRoomListChanged(SyncList<REFLECTIVE_RoomListInfo>.Operation operation,int index,
            REFLECTIVE_RoomListInfo oldInfo,
            REFLECTIVE_RoomListInfo newInfo)
        {
            OnServerCreatedRoom?.Invoke(oldInfo, newInfo);
        }

        #endregion

        #region Base Server Methods

        [ServerCallback]
        public virtual void OnStartedServer()
        {
            NetworkServer.RegisterHandler<REFLECTIVE_ServerRoomMessage>(OnReceivedRoomMessageViaServer);

            m_roomInfos.Callback += OnRoomListChanged;
        }

        [ClientCallback]
        public virtual void OnStartedClient()
        {
            NetworkClient.RegisterHandler<REFLECTIVE_ClientRoomMessage>(OnReceivedRoomMessageViaClient);
        }

        #endregion

        #region Utilities

        [ServerCallback]
        protected void UpdateRoomInfo(REFLECTIVE_Room reflectiveRoom)
        {
            var index = m_roomInfos.FindIndex(info => info.Name == reflectiveRoom.RoomName);

            var roomInfo = new REFLECTIVE_RoomListInfo
            (
                reflectiveRoom.RoomName,
                reflectiveRoom.MaxPlayers,
                reflectiveRoom.CurrentPlayers
            );
            
            m_roomInfos[index] = roomInfo;
        }
        
        [ServerCallback]
        protected void AddToList(REFLECTIVE_Room reflectiveRoom)
        {
            m_rooms.Add(reflectiveRoom);

            var roomListInfo = new REFLECTIVE_RoomListInfo
            (
                reflectiveRoom.RoomName,
                reflectiveRoom.MaxPlayers,
                reflectiveRoom.CurrentPlayers
            );
            
            m_roomInfos.Add(roomListInfo);
        }

        [ServerCallback]
        protected void RemoveToList(REFLECTIVE_Room reflectiveRoom)
        {
            m_rooms.Remove(reflectiveRoom);
            m_roomInfos.RemoveAll(info => info.Name == reflectiveRoom.RoomName);
        }

        [ServerCallback]
        protected static void SendRoomMessage(NetworkConnection conn, REFLECTIVE_ClientRoomState state)
        {
            var roomMessage = new REFLECTIVE_ClientRoomMessage(state, conn.connectionId);

            conn.Send(roomMessage);
        }

        #endregion
    }
}