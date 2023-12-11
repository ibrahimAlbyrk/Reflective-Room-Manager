using System;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Connection
{
    using Room.Enums;
    using Room.Structs;
    
    public class RoomConnections
    {
        #region Events

        //SERVER SIDE
        private event Action<RoomInfo, NetworkConnectionToClient> _onServerCreateRoom;
        private event Action<NetworkConnectionToClient, string> _onServerJoinRoom;
        private event Action<NetworkConnectionToClient, bool> _onServerExitRoom;

        //CLIENT SIDE
        private event Action<RoomInfo> _onClientRoomListAdd;
        private event Action<RoomInfo> _onClientRoomListUpdate;
        private event Action<RoomInfo> _onClientRoomListRemove;
        
        private event Action<int> _onClientConnectionMessage;
        
        private event Action _onClientCreatedRoom;
        private event Action _onClientJoinedRoom;
        private event Action _onClientRemovedRoom;
        private event Action _onClientExitedRoom;
        private event Action _onClientFailedRoom;

        #endregion

        #region Add/Remove methods

        //SERVER SIDE
        public void OnServerCreateRoom_AddListener(Action<RoomInfo, NetworkConnectionToClient> action) => _onServerCreateRoom += action;
        public void OnServerJoinRoom_AddListener(Action<NetworkConnectionToClient, string> action) => _onServerJoinRoom += action;
        public void OnServerExitRoom_AddListener(Action<NetworkConnectionToClient, bool> action) => _onServerExitRoom += action;
        
        public void OnServerCreateRoom_RemoveListener(Action<RoomInfo, NetworkConnectionToClient> action) => _onServerCreateRoom -= action;
        public void OnServerJoinRoom_RemoveListener(Action<NetworkConnectionToClient, string> action) => _onServerJoinRoom -= action;
        public void OnServerExitRoom_RemoveListener(Action<NetworkConnectionToClient, bool> action) => _onServerExitRoom -= action;

        //CLIENT SIDE
        public void OnClientRoomListAdd_AddListener(Action<RoomInfo> action) => _onClientRoomListAdd += action;
        public void OnClientRoomListUpdate_AddListener(Action<RoomInfo> action) => _onClientRoomListUpdate += action;
        public void OnClientRoomListRemove_AddListener(Action<RoomInfo> action) => _onClientRoomListRemove += action;
        public void OnClientConnectionMessage_AddListener(Action<int> action) => _onClientConnectionMessage += action;
        public void OnClientCreatedRoom_AddListener(Action action) => _onClientCreatedRoom += action;
        public void OnClientJoinedRoom_AddListener(Action action) => _onClientJoinedRoom += action;
        public void OnClientRemovedRoom_AddListener(Action action) => _onClientRemovedRoom += action;
        public void OnClientExitedRoom_AddListener(Action action) => _onClientExitedRoom += action;
        public void OnClientFailedRoom_AddListener(Action action) => _onClientFailedRoom += action;
        
        public void OnClientRoomListAdd_RemoveListener(Action<RoomInfo> action) => _onClientRoomListAdd -= action;
        public void OnClientRoomListUpdate_RemoveListener(Action<RoomInfo> action) => _onClientRoomListUpdate -= action;
        public void OnClientRoomListRemove_RemoveListener(Action<RoomInfo> action) => _onClientRoomListRemove -= action;
        public void OnClientConnectionMessage_RemoveListener(Action<int> action) => _onClientConnectionMessage -= action;
        public void OnClientCreatedRoom_RemoveListener(Action action) => _onClientCreatedRoom -= action;
        public void OnClientJoinedRoom_RemoveListener(Action action) => _onClientJoinedRoom -= action;
        public void OnClientRemovedRoom_RemoveListener(Action action) => _onClientRemovedRoom -= action;
        public void OnClientExitedRoom_RemoveListener(Action action) => _onClientExitedRoom -= action;
        public void OnClientFailedRoom_RemoveListener(Action action) => _onClientFailedRoom -= action;

        #endregion

        internal void AddRegistersForServer()
        {
            NetworkServer.RegisterHandler<ServerRoomMessage>(OnReceivedRoomMessageViaServer);
            
        }
        
        internal void AddRegistersForClient()
        {
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnReceivedRoomMessageViaClient);
            NetworkClient.RegisterHandler<RoomListChangeMessage>(OnRoomListChangeForClient);
            NetworkClient.RegisterHandler<ClientConnectionMessage>(OnReceivedConnectionMessageViaClient);
        }
        
        private void OnRoomListChangeForClient(RoomListChangeMessage msg)
        {
            switch (msg.State)
            {
                case RoomMessageState.Add:
                    // Debug.Log("OnClientRoomListAdd");
                    _onClientRoomListAdd?.Invoke(msg.RoomInfo);
                    break;
                case RoomMessageState.Update:
                    // Debug.Log("OnClientRoomListUpdate");
                    _onClientRoomListUpdate?.Invoke(msg.RoomInfo);
                    break;
                case RoomMessageState.Remove:
                    // Debug.Log("OnClientRoomListRemove");
                    _onClientRoomListRemove?.Invoke(msg.RoomInfo);
                    break;
                default:
                    throw new ArgumentException("Invalid RoomMessageState", nameof(msg.State));
            }
        }
        
        /// <summary>
        /// Gets the connection information of the client connecting to the server
        /// </summary>
        /// <param name="msg"></param>
        private void OnReceivedConnectionMessageViaClient(ClientConnectionMessage msg)
        {
            // Debug.Log($"OnClientConnectionMessage, connection ID: {msg.ConnectionID}");
            
            _onClientConnectionMessage?.Invoke(msg.ConnectionID);
        }
        
        /// <summary>
        /// This function is triggered by an event from the "client". It performs various operations based on the incoming event.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void OnReceivedRoomMessageViaServer(NetworkConnectionToClient conn, ServerRoomMessage msg)
        {
            switch (msg.ServerRoomState)
            {
                case ServerRoomState.Create:
                    // Debug.Log("OnServerCreateRoom");
                    _onServerCreateRoom?.Invoke(msg.RoomInfo, conn);
                    break;
                case ServerRoomState.Join:
                    // Debug.Log("OnServerJoinRoom");
                    _onServerJoinRoom?.Invoke(conn, msg.RoomInfo.RoomName);
                    break;
                case ServerRoomState.Exit:
                    // Debug.Log("OnServerExitRoom");
                    _onServerExitRoom?.Invoke(conn, msg.IsDisconnected);
                    break;
                default:
                    throw new ArgumentException("Invalid ServerRoomState", nameof(msg.ServerRoomState));
            }
        }

        /// <summary>
        /// This function is triggered by an event from the "server". It performs various operations based on the incoming event.
        /// </summary>
        /// <param name="msg"></param>
        private void OnReceivedRoomMessageViaClient(ClientRoomMessage msg)
        {
            switch (msg.ClientRoomState)
            {
                case ClientRoomState.Created:
                    Debug.Log("OnClientCreatedRoom");
                    _onClientCreatedRoom?.Invoke();
                    break;
                case ClientRoomState.Joined:
                    Debug.Log("OnClientJoinedRoom");
                    _onClientJoinedRoom?.Invoke();
                    break;
                case ClientRoomState.Removed:
                    Debug.Log("OnClientRemovedRoom");
                    _onClientRemovedRoom?.Invoke();
                    break;
                case ClientRoomState.Exited:
                    Debug.Log("OnClientExitedRoom");
                    _onClientExitedRoom?.Invoke();
                    break;
                case ClientRoomState.Fail:
                    Debug.Log("OnClientFailRoom");
                    _onClientFailedRoom?.Invoke();
                    break;
                default:
                    throw new ArgumentException("Invalid ClientRoomState", nameof(msg.ClientRoomState));
            }
        }
    }
}