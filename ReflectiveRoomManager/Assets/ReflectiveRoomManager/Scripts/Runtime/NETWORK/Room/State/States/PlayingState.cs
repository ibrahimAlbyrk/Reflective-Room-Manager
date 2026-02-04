using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.States
{
    /// <summary>
    /// Playing state - active gameplay.
    /// </summary>
    public class PlayingState : IRoomState
    {
        public string StateName => "Playing";
        public byte StateTypeID => 2;

        public bool CanTransitionTo(IRoomState targetState)
        {
            // Playing can transition to Paused or Ended
            return targetState is PausedState || targetState is EndedState;
        }

        public void OnEnter(Room room, RoomStateContext context)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[PlayingState] Room '{room.Name}' game started");
        }

        public void OnExit(Room room, RoomStateContext context)
        {
            // Cleanup
        }

        public void OnUpdate(Room room, RoomStateContext context, float deltaTime)
        {
            // Check max game duration
            if (context.EffectiveConfig.MaxGameDuration > 0 &&
                context.StateElapsedTime >= context.EffectiveConfig.MaxGameDuration)
            {
                if (context.EffectiveConfig.EnableDebugLogs)
                    Debug.Log($"[PlayingState] Max game duration reached, ending game");
                room.StateMachine.TransitionTo<EndedState>();
            }
        }

        public void OnPlayerJoined(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[PlayingState] Player joined during game");
        }

        public void OnPlayerLeft(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[PlayingState] Player left during game");

            // End game if too few players
            if (room.CurrentPlayers < context.EffectiveConfig.MinPlayersToStart)
            {
                if (context.EffectiveConfig.EnableDebugLogs)
                    Debug.LogWarning($"[PlayingState] Not enough players, ending game");
                room.StateMachine.TransitionTo<EndedState>();
            }
        }

        public RoomStateData GetStateData(RoomStateContext context)
        {
            var timeRemaining = 0f;
            if (context.EffectiveConfig.MaxGameDuration > 0)
            {
                timeRemaining = Mathf.Max(0f, context.EffectiveConfig.MaxGameDuration - context.StateElapsedTime);
            }

            var data = new Dictionary<string, string>
            {
                ["TimeRemaining"] = timeRemaining.ToString("F2")
            };

            return new RoomStateData(StateTypeID, context.StateElapsedTime, data);
        }

        public bool CanPlayerJoin(Room room, NetworkConnection conn, out string reason)
        {
            // Access config through room's state machine context
            if (room.StateMachine != null && !room.StateMachine.Context.EffectiveConfig.AllowJoinDuringPlay)
            {
                reason = "Cannot join during active gameplay";
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
