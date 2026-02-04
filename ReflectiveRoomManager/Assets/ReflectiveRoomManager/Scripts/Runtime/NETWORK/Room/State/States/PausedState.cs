using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.States
{
    /// <summary>
    /// Paused state - game logic suspended.
    /// </summary>
    public class PausedState : IRoomState
    {
        public string StateName => "Paused";
        public byte StateTypeID => 3;

        public bool CanTransitionTo(IRoomState targetState)
        {
            // Paused can transition to Playing or Ended
            return targetState is PlayingState || targetState is EndedState;
        }

        public void OnEnter(Room room, RoomStateContext context)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[PausedState] Room '{room.Name}' game paused");
        }

        public void OnExit(Room room, RoomStateContext context)
        {
            // Cleanup
        }

        public void OnUpdate(Room room, RoomStateContext context, float deltaTime)
        {
            // Check pause timeout
            if (context.EffectiveConfig.PauseTimeout > 0 &&
                context.StateElapsedTime >= context.EffectiveConfig.PauseTimeout)
            {
                if (context.EffectiveConfig.EnableDebugLogs)
                    Debug.Log($"[PausedState] Pause timeout reached, resuming game");
                room.StateMachine.TransitionTo<PlayingState>();
            }
        }

        public void OnPlayerJoined(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[PausedState] Player joined during pause");
        }

        public void OnPlayerLeft(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[PausedState] Player left during pause");

            // End game if too few players
            if (room.CurrentPlayers < context.EffectiveConfig.MinPlayersToStart)
            {
                if (context.EffectiveConfig.EnableDebugLogs)
                    Debug.LogWarning($"[PausedState] Not enough players, ending game");
                room.StateMachine.TransitionTo<EndedState>();
            }
        }

        public RoomStateData GetStateData(RoomStateContext context)
        {
            var timeRemaining = 0f;
            if (context.EffectiveConfig.PauseTimeout > 0)
            {
                timeRemaining = Mathf.Max(0f, context.EffectiveConfig.PauseTimeout - context.StateElapsedTime);
            }

            var data = new Dictionary<string, string>
            {
                ["PauseTimeRemaining"] = timeRemaining.ToString("F2")
            };

            return new RoomStateData(StateTypeID, context.StateElapsedTime, data);
        }

        public bool CanPlayerJoin(Room room, NetworkConnection conn, out string reason)
        {
            // Allow joining during pause (same as during play)
            if (room.StateMachine != null && !room.StateMachine.Context.EffectiveConfig.AllowJoinDuringPlay)
            {
                reason = "Cannot join during paused game";
                return false;
            }

            reason = null;
            return true;
        }

        public bool CanPlayerLeave(Room room, NetworkConnection conn, out string reason)
        {
            reason = null;
            return true;
        }
    }
}
