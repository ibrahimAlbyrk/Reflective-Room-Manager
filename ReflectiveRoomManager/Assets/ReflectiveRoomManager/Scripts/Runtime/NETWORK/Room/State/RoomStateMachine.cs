using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State
{
    using Events;
    using States;

    /// <summary>
    /// Manages room state transitions and lifecycle.
    /// Server-authoritative state machine.
    /// </summary>
    public class RoomStateMachine
    {
        private IRoomState _currentState;
        private readonly Room _room;
        private readonly RoomStateContext _context;
        private readonly Dictionary<Type, IRoomState> _states;
        private readonly Dictionary<byte, IRoomState> _statesByID;

        public IRoomState CurrentState => _currentState;
        public RoomStateContext Context => _context;

        public RoomStateMachine(Room room, RoomStateConfig config, RoomEventManager eventManager)
        {
            _room = room;
            _context = new RoomStateContext(config, eventManager);
            _states = new Dictionary<Type, IRoomState>();
            _statesByID = new Dictionary<byte, IRoomState>();

            RegisterDefaultStates();
        }

        /// <summary>
        /// Registers standard states (Lobby, Starting, Playing, Paused, Ended).
        /// </summary>
        private void RegisterDefaultStates()
        {
            RegisterState(new LobbyState());
            RegisterState(new StartingState());
            RegisterState(new PlayingState());
            RegisterState(new PausedState());
            RegisterState(new EndedState());

            // Set initial state to Lobby
            _currentState = _states[typeof(LobbyState)];
        }

        /// <summary>
        /// Registers a custom state.
        /// </summary>
        public void RegisterState(IRoomState state)
        {
            var type = state.GetType();
            if (_states.ContainsKey(type))
            {
                Debug.LogWarning($"[RoomStateMachine] State {type.Name} already registered");
                return;
            }

            if (_statesByID.ContainsKey(state.StateTypeID))
            {
                Debug.LogError($"[RoomStateMachine] State ID {state.StateTypeID} already in use by {_statesByID[state.StateTypeID].StateName}");
                return;
            }

            _states[type] = state;
            _statesByID[state.StateTypeID] = state;
        }

        /// <summary>
        /// Gets a registered state by type.
        /// </summary>
        public T GetState<T>() where T : class, IRoomState
        {
            return _states.TryGetValue(typeof(T), out var state) ? state as T : null;
        }

        /// <summary>
        /// Transitions to a new state by type.
        /// Server-only operation.
        /// </summary>
        public bool TransitionTo<T>() where T : IRoomState
        {
            return TransitionTo(typeof(T));
        }

        /// <summary>
        /// Transitions to a new state by Type.
        /// </summary>
        public bool TransitionTo(Type stateType)
        {
            if (!_states.TryGetValue(stateType, out var targetState))
            {
                Debug.LogError($"[RoomStateMachine] State {stateType.Name} not registered");
                return false;
            }

            return TransitionTo(targetState);
        }

        /// <summary>
        /// Transitions to a new state by state ID (used for network sync).
        /// </summary>
        public bool TransitionToByID(byte stateID)
        {
            if (!_statesByID.TryGetValue(stateID, out var targetState))
            {
                Debug.LogError($"[RoomStateMachine] State ID {stateID} not found");
                return false;
            }

            return TransitionTo(targetState);
        }

        /// <summary>
        /// Core transition logic with validation.
        /// </summary>
        public bool TransitionTo(IRoomState targetState)
        {
            if (_currentState == targetState)
            {
                if (_context.EffectiveConfig.EnableDebugLogs)
                    Debug.LogWarning($"[RoomStateMachine] Already in {targetState.StateName} state");
                return false;
            }

            if (!_currentState.CanTransitionTo(targetState))
            {
                Debug.LogWarning($"[RoomStateMachine] Cannot transition from {_currentState.StateName} to {targetState.StateName}");
                return false;
            }

            var previousState = _currentState;

            // Exit current state
            _currentState.OnExit(_room, _context);

            // Transition
            _currentState = targetState;
            _context.ResetStateTime();
            _context.ClearTransientData();

            // Enter new state
            _currentState.OnEnter(_room, _context);

            // Notify vote manager of state change
            _room.NotifyStateChangedForVoting(_currentState);

            // Log transition
            if (_context.EffectiveConfig.EnableDebugLogs)
                Debug.Log($"[RoomStateMachine] Room '{_room.Name}' transitioned: {previousState.StateName} -> {_currentState.StateName}");

            // Invoke event
            _context.EventManager?.Invoke_OnServerRoomStateChanged(_room.ID, previousState, _currentState);

            return true;
        }

        /// <summary>
        /// Update current state (called every frame on server).
        /// </summary>
        public void Update(float deltaTime)
        {
            _context.IncrementStateTime(deltaTime);
            _currentState.OnUpdate(_room, _context, deltaTime);
        }

        /// <summary>
        /// Notifies current state of player join.
        /// </summary>
        public void OnPlayerJoined(NetworkConnection conn)
        {
            _currentState.OnPlayerJoined(_room, _context, conn);
        }

        /// <summary>
        /// Notifies current state of player leave.
        /// </summary>
        public void OnPlayerLeft(NetworkConnection conn)
        {
            _currentState.OnPlayerLeft(_room, _context, conn);
        }

        /// <summary>
        /// Checks if a player can join in the current state.
        /// </summary>
        public bool CanPlayerJoin(NetworkConnection conn, out string reason)
        {
            return _currentState.CanPlayerJoin(_room, conn, out reason);
        }

        /// <summary>
        /// Checks if a player can leave in the current state.
        /// </summary>
        public bool CanPlayerLeave(NetworkConnection conn, out string reason)
        {
            return _currentState.CanPlayerLeave(_room, conn, out reason);
        }

        /// <summary>
        /// Gets current state data for network sync.
        /// </summary>
        public RoomStateData GetCurrentStateData()
        {
            return _currentState.GetStateData(_context);
        }

        /// <summary>
        /// Restores state from network data (client-side).
        /// </summary>
        public void RestoreState(RoomStateData stateData)
        {
            if (!_statesByID.TryGetValue(stateData.StateTypeID, out var targetState))
            {
                Debug.LogError($"[RoomStateMachine] Cannot restore state ID {stateData.StateTypeID}");
                return;
            }

            if (_currentState != targetState)
            {
                _currentState = targetState;
            }

            _context.RestoreFromData(stateData);
        }
    }
}
