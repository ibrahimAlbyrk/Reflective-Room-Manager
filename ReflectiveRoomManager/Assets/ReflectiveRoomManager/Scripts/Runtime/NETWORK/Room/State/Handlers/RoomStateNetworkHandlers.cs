using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.Handlers
{
    using Messages;
    using States;

    /// <summary>
    /// Network message handlers for room state synchronization.
    /// </summary>
    public static class RoomStateNetworkHandlers
    {
#if REFLECTIVE_SERVER
        private static bool _serverHandlersRegistered;

        public static void RegisterServerHandlers()
        {
            if (_serverHandlersRegistered)
            {
                Debug.LogWarning("[RoomStateNetworkHandlers] Server handlers already registered");
                return;
            }

            NetworkServer.RegisterHandler<RoomStateActionMessage>(OnServerStateAction);
            _serverHandlersRegistered = true;
        }

        public static void UnregisterServerHandlers()
        {
            if (!_serverHandlersRegistered) return;

            NetworkServer.UnregisterHandler<RoomStateActionMessage>();
            _serverHandlersRegistered = false;
        }

        private static void OnServerStateAction(NetworkConnectionToClient conn, RoomStateActionMessage msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
            {
                Debug.LogWarning("[RoomStateNetworkHandlers] RoomManagerBase instance not found");
                return;
            }

            var room = roomManager.GetRoom(msg.RoomID);
            if (room == null)
            {
                Debug.LogWarning($"[RoomStateNetworkHandlers] Room ID {msg.RoomID} not found");
                return;
            }

            if (room.StateMachine == null)
            {
                Debug.LogWarning($"[RoomStateNetworkHandlers] Room '{room.Name}' has no state machine");
                return;
            }

            // Verify connection is in the room
            if (!room.Connections.Contains(conn))
            {
                Debug.LogWarning($"[RoomStateNetworkHandlers] Connection not in room '{room.Name}'");
                return;
            }

            var handled = HandleStateAction(room, conn, msg);
            if (handled)
            {
                BroadcastStateUpdate(room);
            }
        }

        private static bool HandleStateAction(Room room, NetworkConnectionToClient conn, RoomStateActionMessage msg)
        {
            var stateMachine = room.StateMachine;
            var currentState = stateMachine.CurrentState;
            var context = stateMachine.Context;

            switch (msg.Action)
            {
                case RoomStateAction.MarkReady:
                    if (currentState is LobbyState lobbyState)
                    {
                        lobbyState.MarkPlayerReady(context, conn);
                        return true;
                    }
                    break;

                case RoomStateAction.UnmarkReady:
                    if (currentState is LobbyState unreadyLobbyState)
                    {
                        unreadyLobbyState.UnmarkPlayerReady(context, conn);
                        return true;
                    }
                    break;

                case RoomStateAction.PauseGame:
                    if (currentState is PlayingState && context.EffectiveConfig.AllowPausing)
                    {
                        // Check pause permission
                        if (CanPlayerPause(room, conn, context))
                        {
                            return stateMachine.TransitionTo<PausedState>();
                        }
                        Debug.LogWarning("[RoomStateNetworkHandlers] Player lacks permission to pause");
                    }
                    break;

                case RoomStateAction.ResumeGame:
                    if (currentState is PausedState)
                    {
                        return stateMachine.TransitionTo<PlayingState>();
                    }
                    break;

                case RoomStateAction.EndGame:
                    if (currentState is PlayingState || currentState is PausedState)
                    {
                        return stateMachine.TransitionTo<EndedState>();
                    }
                    break;

                case RoomStateAction.RestartGame:
                    if (currentState is EndedState)
                    {
                        return stateMachine.TransitionTo<LobbyState>();
                    }
                    break;
            }

            return false;
        }

        private static bool CanPlayerPause(Room room, NetworkConnection conn, RoomStateContext context)
        {
            switch (context.Config.PausePermission)
            {
                case PausePermission.Anyone:
                    return true;

                case PausePermission.OwnerOnly:
                    // First connection in the room is considered owner
                    return room.Connections.Count > 0 && room.Connections[0] == conn;

                case PausePermission.AdminsOnly:
                    // Would require admin system - default to owner only
                    return room.Connections.Count > 0 && room.Connections[0] == conn;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Broadcasts state update to all connections in the room.
        /// </summary>
        public static void BroadcastStateUpdate(Room room)
        {
            if (room?.StateMachine == null) return;

            var stateData = room.StateMachine.GetCurrentStateData();
            var message = new RoomStateChangeMessage(room.ID, stateData);

            foreach (var conn in room.Connections)
            {
                conn.Send(message);
            }
        }

        /// <summary>
        /// Broadcasts periodic state sync to all connections in the room.
        /// </summary>
        public static void BroadcastStateSync(Room room)
        {
            if (room?.StateMachine == null) return;

            var stateData = room.StateMachine.GetCurrentStateData();
            var message = new RoomStateSyncMessage(
                room.ID,
                stateData.StateTypeID,
                stateData.ElapsedTime,
                stateData.Data
            );

            foreach (var conn in room.Connections)
            {
                conn.Send(message);
            }
        }

        /// <summary>
        /// Sends state update to a specific connection (e.g., late joiner).
        /// </summary>
        public static void SendStateToConnection(Room room, NetworkConnection conn)
        {
            if (room?.StateMachine == null) return;

            var stateData = room.StateMachine.GetCurrentStateData();
            var message = new RoomStateChangeMessage(room.ID, stateData);
            conn.Send(message);
        }
#endif

#if REFLECTIVE_CLIENT
        private static bool _clientHandlersRegistered;

        public static void RegisterClientHandlers()
        {
            if (_clientHandlersRegistered)
            {
                Debug.LogWarning("[RoomStateNetworkHandlers] Client handlers already registered");
                return;
            }

            NetworkClient.RegisterHandler<RoomStateChangeMessage>(OnClientStateChange);
            NetworkClient.RegisterHandler<RoomStateSyncMessage>(OnClientStateSync);
            _clientHandlersRegistered = true;
        }

        public static void UnregisterClientHandlers()
        {
            if (!_clientHandlersRegistered) return;

            NetworkClient.UnregisterHandler<RoomStateChangeMessage>();
            NetworkClient.UnregisterHandler<RoomStateSyncMessage>();
            _clientHandlersRegistered = false;
        }

        private static void OnClientStateChange(RoomStateChangeMessage msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null) return;

            // Trigger client-side event
            OnClientRoomStateChanged?.Invoke(msg.RoomID, msg.StateData);
        }

        private static void OnClientStateSync(RoomStateSyncMessage msg)
        {
            // Trigger client-side sync event
            OnClientRoomStateSync?.Invoke(msg.RoomID, msg.StateTypeID, msg.StateElapsedTime, msg.StateData);
        }

        // Client-side events
        public delegate void ClientStateChangedHandler(uint roomID, RoomStateData stateData);
        public delegate void ClientStateSyncHandler(uint roomID, byte stateTypeID, float elapsedTime, System.Collections.Generic.Dictionary<string, string> stateData);

        public static event ClientStateChangedHandler OnClientRoomStateChanged;
        public static event ClientStateSyncHandler OnClientRoomStateSync;

        public static void ClearClientEvents()
        {
            OnClientRoomStateChanged = null;
            OnClientRoomStateSync = null;
        }
#endif
    }
}
