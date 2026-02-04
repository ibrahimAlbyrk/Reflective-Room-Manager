using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using State;
    using Events;

    /// <summary>
    /// Partial class extension for Room state machine support.
    /// </summary>
    public partial class Room
    {
        /// <summary>
        /// State machine instance (null if state machine not enabled).
        /// </summary>
        public RoomStateMachine StateMachine { get; private set; }

        /// <summary>
        /// Initializes state machine for this room.
        /// Called during room creation.
        /// </summary>
        internal void InitializeStateMachine(RoomStateConfig config, RoomEventManager eventManager)
        {
            if (StateMachine != null)
            {
                Debug.LogWarning($"[Room] State machine already initialized for room '{Name}'");
                return;
            }

            if (config == null)
            {
                Debug.LogError($"[Room] Cannot initialize state machine for room '{Name}': config is null");
                return;
            }

            StateMachine = new RoomStateMachine(this, config, eventManager);
        }

        /// <summary>
        /// Initializes state machine with per-room config override.
        /// </summary>
        internal void InitializeStateMachine(RoomStateConfig config, RoomEventManager eventManager, RoomStateConfigOverride configOverride)
        {
            InitializeStateMachine(config, eventManager);

            if (StateMachine != null)
            {
                StateMachine.Context.ApplyOverride(configOverride);
            }
        }

        /// <summary>
        /// Updates state machine (called from RoomManagerBase.Update).
        /// </summary>
        internal void UpdateStateMachine(float deltaTime)
        {
            StateMachine?.Update(deltaTime);
        }

        /// <summary>
        /// Checks if state machine allows player to join.
        /// </summary>
        internal bool CanPlayerJoinState(Mirror.NetworkConnection conn, out string reason)
        {
            if (StateMachine == null)
            {
                reason = null;
                return true;
            }

            return StateMachine.CanPlayerJoin(conn, out reason);
        }

        /// <summary>
        /// Checks if state machine allows player to leave.
        /// </summary>
        internal bool CanPlayerLeaveState(Mirror.NetworkConnection conn, out string reason)
        {
            if (StateMachine == null)
            {
                reason = null;
                return true;
            }

            return StateMachine.CanPlayerLeave(conn, out reason);
        }

        /// <summary>
        /// Notifies state machine that a player joined.
        /// </summary>
        internal void NotifyStateMachinePlayerJoined(Mirror.NetworkConnection conn)
        {
            StateMachine?.OnPlayerJoined(conn);
        }

        /// <summary>
        /// Notifies state machine that a player left.
        /// </summary>
        internal void NotifyStateMachinePlayerLeft(Mirror.NetworkConnection conn)
        {
            StateMachine?.OnPlayerLeft(conn);
        }
    }
}
