using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.States
{
    /// <summary>
    /// Lobby state - players gathering, ready check, countdown.
    /// </summary>
    public class LobbyState : IRoomState
    {
        public string StateName => "Lobby";
        public byte StateTypeID => 0;

        private const string KEY_READY_PLAYERS = "ReadyPlayers";
        private const string KEY_COUNTDOWN_ACTIVE = "CountdownActive";
        private const string KEY_COUNTDOWN_START_TIME = "CountdownStartTime";

        // Cache for ready player connections to avoid allocations
        private readonly HashSet<int> _readyConnectionIds = new();

        public bool CanTransitionTo(IRoomState targetState)
        {
            // Lobby can transition to Starting or Ended
            return targetState is StartingState || targetState is EndedState;
        }

        public void OnEnter(Room room, RoomStateContext context)
        {
            _readyConnectionIds.Clear();
            context.SetTransientData(KEY_READY_PLAYERS, 0);
            context.SetTransientData(KEY_COUNTDOWN_ACTIVE, false);
            context.SetTransientData(KEY_COUNTDOWN_START_TIME, 0f);

            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[LobbyState] Room '{room.Name}' entered Lobby state");
        }

        public void OnExit(Room room, RoomStateContext context)
        {
            _readyConnectionIds.Clear();
        }

        public void OnUpdate(Room room, RoomStateContext context, float deltaTime)
        {
            var isCountdownActive = context.GetTransientData(KEY_COUNTDOWN_ACTIVE, false);

            if (!isCountdownActive)
            {
                if (CanStartCountdown(room, context))
                {
                    context.SetTransientData(KEY_COUNTDOWN_ACTIVE, true);
                    context.SetTransientData(KEY_COUNTDOWN_START_TIME, context.StateElapsedTime);

                    if (context.EffectiveConfig.EnableDebugLogs)
                        Debug.Log($"[LobbyState] Starting countdown for room '{room.Name}'");
                }
            }
            else
            {
                var countdownStartTime = context.GetTransientData(KEY_COUNTDOWN_START_TIME, 0f);
                var countdownElapsed = context.StateElapsedTime - countdownStartTime;

                if (countdownElapsed >= context.EffectiveConfig.LobbyCountdownDuration)
                {
                    room.StateMachine.TransitionTo<StartingState>();
                }
                else if (!CanStartCountdown(room, context))
                {
                    context.SetTransientData(KEY_COUNTDOWN_ACTIVE, false);

                    if (context.EffectiveConfig.EnableDebugLogs)
                        Debug.Log($"[LobbyState] Countdown cancelled for room '{room.Name}'");
                }
            }
        }

        public void OnPlayerJoined(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[LobbyState] Player joined room '{room.Name}' in Lobby");
        }

        public void OnPlayerLeft(Room room, RoomStateContext context, NetworkConnection conn)
        {
            // Remove from ready list if player was ready
            var connId = (conn as NetworkConnectionToClient)?.connectionId ?? conn.GetHashCode();
            if (_readyConnectionIds.Remove(connId))
            {
                var readyCount = context.GetTransientData(KEY_READY_PLAYERS, 0);
                context.SetTransientData(KEY_READY_PLAYERS, Mathf.Max(0, readyCount - 1));
            }

            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[LobbyState] Player left room '{room.Name}' in Lobby");
        }

        public RoomStateData GetStateData(RoomStateContext context)
        {
            var isCountdownActive = context.GetTransientData(KEY_COUNTDOWN_ACTIVE, false);
            var countdownStartTime = context.GetTransientData(KEY_COUNTDOWN_START_TIME, 0f);
            var countdownRemaining = 0f;

            if (isCountdownActive)
            {
                var elapsed = context.StateElapsedTime - countdownStartTime;
                countdownRemaining = Mathf.Max(0f, context.EffectiveConfig.LobbyCountdownDuration - elapsed);
            }

            var data = new Dictionary<string, string>
            {
                [KEY_READY_PLAYERS] = context.GetTransientData(KEY_READY_PLAYERS, 0).ToString(),
                [KEY_COUNTDOWN_ACTIVE] = isCountdownActive.ToString(),
                ["CountdownRemaining"] = countdownRemaining.ToString("F2")
            };

            return new RoomStateData(StateTypeID, context.StateElapsedTime, data);
        }

        public bool CanPlayerJoin(Room room, NetworkConnection conn, out string reason)
        {
            reason = null;
            return true; // Lobby always allows joins
        }

        public bool CanPlayerLeave(Room room, NetworkConnection conn, out string reason)
        {
            reason = null;
            return true; // Lobby always allows leaves
        }

        /// <summary>
        /// Checks if countdown should start (min players + all ready if required).
        /// </summary>
        private bool CanStartCountdown(Room room, RoomStateContext context)
        {
            // Check minimum players
            if (room.CurrentPlayers < context.EffectiveConfig.MinPlayersToStart)
                return false;

            // If require all ready, check ready count
            if (context.EffectiveConfig.RequireAllPlayersReady)
            {
                var readyCount = context.GetTransientData(KEY_READY_PLAYERS, 0);
                return readyCount >= room.CurrentPlayers;
            }

            return true;
        }

        /// <summary>
        /// External method to mark player as ready (called by game logic).
        /// </summary>
        public void MarkPlayerReady(RoomStateContext context, NetworkConnection conn)
        {
            var connId = (conn as NetworkConnectionToClient)?.connectionId ?? conn.GetHashCode();
            if (_readyConnectionIds.Contains(connId))
                return;

            _readyConnectionIds.Add(connId);
            var readyCount = context.GetTransientData(KEY_READY_PLAYERS, 0);
            context.SetTransientData(KEY_READY_PLAYERS, readyCount + 1);
        }

        /// <summary>
        /// External method to unmark player as ready.
        /// </summary>
        public void UnmarkPlayerReady(RoomStateContext context, NetworkConnection conn)
        {
            var connId = (conn as NetworkConnectionToClient)?.connectionId ?? conn.GetHashCode();
            if (!_readyConnectionIds.Remove(connId))
                return;

            var readyCount = context.GetTransientData(KEY_READY_PLAYERS, 0);
            context.SetTransientData(KEY_READY_PLAYERS, Mathf.Max(0, readyCount - 1));
        }

        /// <summary>
        /// Checks if a player is ready.
        /// </summary>
        public bool IsPlayerReady(NetworkConnection conn)
        {
            var connId = (conn as NetworkConnectionToClient)?.connectionId ?? conn.GetHashCode();
            return _readyConnectionIds.Contains(connId);
        }

        /// <summary>
        /// Gets the current ready player count.
        /// </summary>
        public int GetReadyPlayerCount(RoomStateContext context)
        {
            return context.GetTransientData(KEY_READY_PLAYERS, 0);
        }
    }
}
