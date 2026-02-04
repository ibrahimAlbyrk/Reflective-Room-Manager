using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.States
{
    /// <summary>
    /// Starting state - short countdown (3-2-1), resource loading.
    /// </summary>
    public class StartingState : IRoomState
    {
        public string StateName => "Starting";
        public byte StateTypeID => 1;

        public bool CanTransitionTo(IRoomState targetState)
        {
            // Starting can transition to Playing or Lobby (if player left)
            return targetState is PlayingState || targetState is LobbyState;
        }

        public void OnEnter(Room room, RoomStateContext context)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[StartingState] Room '{room.Name}' starting in {context.EffectiveConfig.StartingCountdownDuration}s");
        }

        public void OnExit(Room room, RoomStateContext context)
        {
            // Cleanup
        }

        public void OnUpdate(Room room, RoomStateContext context, float deltaTime)
        {
            // Check if countdown complete
            if (context.StateElapsedTime >= context.EffectiveConfig.StartingCountdownDuration)
            {
                room.StateMachine.TransitionTo<PlayingState>();
                return;
            }

            // Check if player left and we no longer meet requirements
            if (room.CurrentPlayers < context.EffectiveConfig.MinPlayersToStart)
            {
                if (context.EffectiveConfig.EnableDebugLogs)
                    Debug.LogWarning($"[StartingState] Not enough players, returning to Lobby");
                room.StateMachine.TransitionTo<LobbyState>();
            }
        }

        public void OnPlayerJoined(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[StartingState] Player joined during starting phase");
        }

        public void OnPlayerLeft(Room room, RoomStateContext context, NetworkConnection conn)
        {
            if (context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[StartingState] Player left during starting phase");
        }

        public RoomStateData GetStateData(RoomStateContext context)
        {
            var countdownRemaining = Mathf.Max(0f, context.EffectiveConfig.StartingCountdownDuration - context.StateElapsedTime);

            var data = new Dictionary<string, string>
            {
                ["CountdownRemaining"] = countdownRemaining.ToString("F2")
            };

            return new RoomStateData(StateTypeID, context.StateElapsedTime, data);
        }

        public bool CanPlayerJoin(Room room, NetworkConnection conn, out string reason)
        {
            // Allow joining during Starting phase
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
