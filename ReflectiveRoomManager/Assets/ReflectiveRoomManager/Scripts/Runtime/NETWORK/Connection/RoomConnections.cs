using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Connection
{
    using Data;
    using Room.Enums;
    using Room.Scenes;
    using Room.Structs;
    using Room.Utilities;

    public class RoomConnections
    {
        private RateLimiter _rateLimiter;

        internal void SetRateLimiter(RateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter;
        }

        //SERVER SIDE
        public readonly ConnectionEvent<RoomInfo, NetworkConnectionToClient> OnServerCreateRoom = new(false);
        public readonly ConnectionEvent<NetworkConnectionToClient, string, string> OnServerJoinRoom = new(false);
        public readonly ConnectionEvent<NetworkConnectionToClient, bool> OnServerExitRoom = new(false);

        public readonly ConnectionEvent<Scene> OnServerRoomSceneLoaded = new(false);
        public readonly ConnectionEvent<Scene> OnServerRoomSceneChanged = new(false);

        //CLIENT SIDE
        public readonly ConnectionEvent<RoomInfo> OnClientRoomListAdd = new(false);
        public readonly ConnectionEvent<RoomInfo> OnClientRoomListUpdate = new(false);
        public readonly ConnectionEvent<RoomInfo> OnClientRoomListRemove = new(false);
        
        public readonly ConnectionEvent<uint> OnClientRoomIDMessage = new(false);
        
        public readonly ConnectionEvent OnClientCreatedRoom = new(false);
        public readonly ConnectionEvent OnClientJoinedRoom = new(false);
        public readonly ConnectionEvent OnClientRemovedRoom = new(false);
        public readonly ConnectionEvent OnClientExitedRoom = new(false);
        public readonly ConnectionEvent OnClientFailedRoom = new(false);
        
        public readonly ConnectionEvent<SceneLoadMessage> OnClientSceneLoaded = new(false);

        internal void AddRegistersForServer()
        {
            NetworkServer.RegisterHandler<ServerRoomMessage>(OnReceivedRoomMessageViaServer);
        }
        
        internal void AddRegistersForClient()
        {
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnReceivedRoomMessageViaClient);
            NetworkClient.RegisterHandler<RoomListChangeMessage>(OnRoomListChangeForClient);
            NetworkClient.RegisterHandler<ClientRoomIDMessage>(OnReceivedRoomIDViaClient);
            NetworkClient.RegisterHandler<SceneLoadMessage>(OnReceivedSceneLoadMessage);
        }

        private void OnRoomListChangeForClient(RoomListChangeMessage msg)
        {
            switch (msg.State)
            {
                case RoomMessageState.Add:
                    OnClientRoomListAdd.Call(msg.RoomInfo);
                    break;
                case RoomMessageState.Update:
                    OnClientRoomListUpdate.Call(msg.RoomInfo);
                    break;
                case RoomMessageState.Remove:
                    OnClientRoomListRemove.Call(msg.RoomInfo);
                    break;
                default:
                    throw new ArgumentException("Invalid RoomMessageState", nameof(msg.State));
            }
        }
        
        /// <summary>
        /// Gets the connection information of the client connecting to the server
        /// </summary>
        /// <param name="msg"></param>
        private void OnReceivedRoomIDViaClient(ClientRoomIDMessage msg)
        {
            OnClientRoomIDMessage.Call(msg.RoomID);
        }

        private void OnReceivedSceneLoadMessage(SceneLoadMessage msg)
        {
            OnClientSceneLoaded?.Call(msg);
        }
        
        /// <summary>
        /// This function is triggered by an event from the "client". It performs various operations based on the incoming event.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void OnReceivedRoomMessageViaServer(NetworkConnectionToClient conn, ServerRoomMessage msg)
        {
            if (_rateLimiter != null && !_rateLimiter.IsAllowed(conn))
            {
                Debug.LogWarning($"[RateLimiter] Connection {conn.connectionId} exceeded rate limit");
                conn.Send(new ClientRoomMessage(ClientRoomState.Fail));
                return;
            }

            switch (msg.ServerRoomState)
            {
                case ServerRoomState.Create:
                    OnServerCreateRoom.Call(msg.RoomInfo, conn);
                    break;
                case ServerRoomState.Join:
                    OnServerJoinRoom.Call(conn, msg.RoomInfo.RoomName, msg.AccessToken ?? string.Empty);
                    break;
                case ServerRoomState.Exit:
                    OnServerExitRoom.Call(conn, msg.IsDisconnected);
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
                    OnClientCreatedRoom.Call();
                    break;
                case ClientRoomState.Joined:
                    OnClientJoinedRoom.Call();
                    break;
                case ClientRoomState.Removed:
                    OnClientRemovedRoom.Call();
                    break;
                case ClientRoomState.Exited:
                    OnClientExitedRoom.Call();
                    break;
                case ClientRoomState.Fail:
                    OnClientFailedRoom.Call();
                    break;
                default:
                    throw new ArgumentException("Invalid ClientRoomState", nameof(msg.ClientRoomState));
            }
        }
    }
}