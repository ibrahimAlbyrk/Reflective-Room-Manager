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
#if REFLECTIVE_SERVER
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

        internal void AddRegistersForServer()
        {
            NetworkServer.RegisterHandler<ServerRoomMessage>(OnReceivedRoomMessageViaServer);
        }

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
#endif

#if REFLECTIVE_CLIENT
        private RuntimeContainerHandler _runtimeContainerHandler;

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

        public readonly ConnectionEvent<float> OnClientShutdownWarning = new(false);

        internal void AddRegistersForClient(bool initializeContainerHandler = true)
        {
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnReceivedRoomMessageViaClient);
            NetworkClient.RegisterHandler<RoomListChangeMessage>(OnRoomListChangeForClient);
            NetworkClient.RegisterHandler<ClientRoomIDMessage>(OnReceivedRoomIDViaClient);
            NetworkClient.RegisterHandler<SceneLoadMessage>(OnReceivedSceneLoadMessage);
            NetworkClient.RegisterHandler<ServerShutdownWarningMessage>(OnReceivedShutdownWarning);

            if (initializeContainerHandler)
            {
                _runtimeContainerHandler = new RuntimeContainerHandler();
                _runtimeContainerHandler.RegisterClientHandlers();
            }
        }

        internal void CleanupClientHandlers()
        {
            _runtimeContainerHandler?.UnregisterClientHandlers();
            _runtimeContainerHandler = null;
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

        private void OnReceivedRoomIDViaClient(ClientRoomIDMessage msg)
        {
            OnClientRoomIDMessage.Call(msg.RoomID);
        }

        private void OnReceivedSceneLoadMessage(SceneLoadMessage msg)
        {
            OnClientSceneLoaded?.Call(msg);
        }

        private void OnReceivedShutdownWarning(ServerShutdownWarningMessage msg)
        {
            OnClientShutdownWarning.Call(msg.SecondsRemaining);
        }

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
#endif
    }
}