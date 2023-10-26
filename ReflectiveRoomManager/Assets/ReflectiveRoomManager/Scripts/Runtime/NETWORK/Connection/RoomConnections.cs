using System;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Connection
{
    using Room.Enums;
    using Room.Structs;
    
    public class RoomConnections
    {
        #region Events

        //Server Side
        public event Action<NetworkConnectionToClient, RoomInfo> OnServerCreateRoom;
        public event Action<NetworkConnectionToClient, string> OnServerJoinRoom;
        public event Action<NetworkConnectionToClient, bool> OnServerExitRoom;

        //Client Side
        public event Action<RoomInfo> OnClientRoomListAdd; 
        public event Action<RoomInfo> OnClientRoomListUpdate; 
        public event Action<RoomInfo> OnClientRoomListRemove; 
        
        public event Action<int> OnClientConnectionMessage;
        
        public event Action OnClientCreatedRoom;
        public event Action OnClientJoinedRoom;
        public event Action OnClientRemovedRoom;
        public event Action OnClientExitedRoom;
        public event Action OnClientFailedRoom;

        #endregion

        public void AddRegistersForServer()
        {
            NetworkServer.RegisterHandler<ServerRoomMessage>(OnReceivedRoomMessageViaServer);
            
        }
        
        public void AddRegistersForClient()
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
                    OnClientRoomListAdd?.Invoke(msg.RoomInfo);
                    break;
                case RoomMessageState.Update:
                    OnClientRoomListUpdate?.Invoke(msg.RoomInfo);
                    break;
                case RoomMessageState.Remove:
                    OnClientRoomListRemove?.Invoke(msg.RoomInfo);
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
            OnClientConnectionMessage?.Invoke(msg.ConnectionID);
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
                    OnServerCreateRoom?.Invoke(conn, msg.RoomInfo);
                    break;
                case ServerRoomState.Join:
                    OnServerJoinRoom?.Invoke(conn, msg.RoomInfo.Name);
                    break;
                case ServerRoomState.Exit:
                    OnServerExitRoom?.Invoke(conn, msg.IsDisconnected);
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
                    throw new ArgumentException("Invalid ClientRoomState", nameof(msg.ClientRoomState));
            }
        }
    }
}