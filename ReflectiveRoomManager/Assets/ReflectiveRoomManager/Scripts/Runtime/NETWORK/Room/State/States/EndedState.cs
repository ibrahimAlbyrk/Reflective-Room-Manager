using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.States
{
    /// <summary>
    /// Ended state - game over, results shown.
    /// </summary>
    public class EndedState : IRoomState
    {
        public string StateName => "Ended";
        public byte StateTypeID => 4;

        private const string KEY_SHOULD_CLOSE_ROOM = "ShouldCloseRoom";

        public bool CanTransitionTo(IRoomState targetState)
        {
            // Ended can transition to Lobby (restart) or be removed
            return targetState is LobbyState;
        }

        public void OnEnter(Room room, RoomStateContext context)
        {
            context.SetTransientData(KEY_SHOULD_CLOSE_ROOM, false);

            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[EndedState] Room '{room.Name}' game ended");
        }

        public void OnExit(Room room, RoomStateContext context)
        {
            // Cleanup
        }

        public void OnUpdate(Room room, RoomStateContext context, float deltaTime)
        {
            // Check end screen duration
            if (context.StateElapsedTime >= context.EffectiveConfig.EndScreenDuration)
            {
                if (context.EffectiveConfig.AutoReturnToLobby)
                {
                    if (context.EffectiveConfig.EnableDebugLogs)
                        Debug.Log($"[EndedState] Returning to lobby");
                    room.StateMachine.TransitionTo<LobbyState>();
                }
                else if (context.EffectiveConfig.AutoCloseRoomOnEnd)
                {
                    if (context.EffectiveConfig.EnableDebugLogs)
                        Debug.Log($"[EndedState] Auto-closing room");
                    // Signal that room should be closed (handled by RoomManager)
                    context.SetTransientData(KEY_SHOULD_CLOSE_ROOM, true);
                }
            }
        }

        public void OnPlayerJoined(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[EndedState] Player joined after game ended");
        }

        public void OnPlayerLeft(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[EndedState] Player left after game ended");
        }

        public RoomStateData GetStateData(RoomStateContext context)
        {
            var timeRemaining = Mathf.Max(0f, context.EffectiveConfig.EndScreenDuration - context.StateElapsedTime);

            var data = new Dictionary<string, string>
            {
                ["EndScreenTimeRemaining"] = timeRemaining.ToString("F2"),
                ["WillReturnToLobby"] = context.EffectiveConfig.AutoReturnToLobby.ToString()
            };

            return new RoomStateData(StateTypeID, context.StateElapsedTime, data);
        }

        public bool CanPlayerJoin(Room room, NetworkConnection conn, out string reason)
        {
            // Generally allow joining during end screen (new players can see results)
            reason = null;
            return true;
        }

        public bool CanPlayerLeave(Room room, NetworkConnection conn, out string reason)
        {
            reason = null;
            return true;
        }

        /// <summary>
        /// Checks if room should be closed (called by RoomManager).
        /// </summary>
        public bool ShouldCloseRoom(RoomStateContext context)
        {
            return context.GetTransientData(KEY_SHOULD_CLOSE_ROOM, false);
        }
    }
}
