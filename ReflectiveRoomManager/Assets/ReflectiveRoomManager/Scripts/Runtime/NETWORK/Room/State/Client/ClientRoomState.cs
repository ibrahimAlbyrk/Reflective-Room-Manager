using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.Client
{
    using Messages;

    /// <summary>
    /// Client-side room state wrapper with prediction.
    /// </summary>
    public class ClientRoomState
    {
        private readonly ClientStatePrediction _prediction;
        private readonly RoomStateConfig _config;
        private byte _currentStateID;
        private Dictionary<string, string> _stateData;

        public ClientRoomState(RoomStateConfig config)
        {
            _config = config;
            _prediction = new ClientStatePrediction();
            _stateData = new Dictionary<string, string>();
        }

        /// <summary>Current state ID</summary>
        public byte CurrentStateID => _currentStateID;

        /// <summary>Predicted elapsed time for smooth UI</summary>
        public float ElapsedTime => _prediction.DisplayElapsed;

        /// <summary>Current state name</summary>
        public string StateName => GetStateName(_currentStateID);

        /// <summary>Gets state-specific data</summary>
        public string GetStateDataValue(string key)
        {
            return _stateData.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Returns countdown remaining based on current state.
        /// </summary>
        public float GetCountdownRemaining()
        {
            return _currentStateID switch
            {
                0 => _prediction.GetCountdownRemaining(_config.LobbyCountdownDuration),   // Lobby
                1 => _prediction.GetCountdownRemaining(_config.StartingCountdownDuration), // Starting
                2 => _prediction.GetCountdownRemaining(_config.MaxGameDuration),           // Playing
                3 => _prediction.GetCountdownRemaining(_config.PauseTimeout),              // Paused
                4 => _prediction.GetCountdownRemaining(_config.EndScreenDuration),         // Ended
                _ => 0f
            };
        }

        /// <summary>
        /// Called when state change message received from server.
        /// </summary>
        public void OnStateChange(RoomStateChangeMessage msg)
        {
            _currentStateID = msg.StateData.StateTypeID;
            _stateData = msg.StateData.Data ?? new Dictionary<string, string>();
            _prediction.OnStateChange(msg.StateData.StateTypeID, msg.StateData.ElapsedTime);
        }

        /// <summary>
        /// Called when state change message received with explicit data.
        /// </summary>
        public void OnStateChange(RoomStateData stateData)
        {
            _currentStateID = stateData.StateTypeID;
            _stateData = stateData.Data ?? new Dictionary<string, string>();
            _prediction.OnStateChange(stateData.StateTypeID, stateData.ElapsedTime);
        }

        /// <summary>
        /// Called when sync message received from server.
        /// </summary>
        public void OnStateSync(RoomStateSyncMessage msg)
        {
            _prediction.OnStateSync(msg.StateTypeID, msg.StateElapsedTime);
            if (msg.StateData != null)
            {
                _stateData = msg.StateData;
            }
        }

        /// <summary>
        /// Called when sync message received with explicit data.
        /// </summary>
        public void OnStateSync(byte stateTypeID, float elapsedTime, Dictionary<string, string> stateData)
        {
            _prediction.OnStateSync(stateTypeID, elapsedTime);
            if (stateData != null)
            {
                _stateData = stateData;
            }
        }

        /// <summary>
        /// Update prediction (call every frame).
        /// </summary>
        public void Update(float deltaTime)
        {
            _prediction.Update(deltaTime);
        }

        /// <summary>
        /// Stops prediction.
        /// </summary>
        public void Stop()
        {
            _prediction.Stop();
        }

        /// <summary>
        /// Resets to initial state.
        /// </summary>
        public void Reset()
        {
            _currentStateID = 0;
            _stateData.Clear();
            _prediction.Reset();
        }

        /// <summary>
        /// Checks if currently in specified state.
        /// </summary>
        public bool IsInState(byte stateID)
        {
            return _currentStateID == stateID;
        }

        /// <summary>
        /// Checks if in Lobby state.
        /// </summary>
        public bool IsInLobby => _currentStateID == 0;

        /// <summary>
        /// Checks if in Starting state.
        /// </summary>
        public bool IsStarting => _currentStateID == 1;

        /// <summary>
        /// Checks if in Playing state.
        /// </summary>
        public bool IsPlaying => _currentStateID == 2;

        /// <summary>
        /// Checks if in Paused state.
        /// </summary>
        public bool IsPaused => _currentStateID == 3;

        /// <summary>
        /// Checks if in Ended state.
        /// </summary>
        public bool IsEnded => _currentStateID == 4;

        private static string GetStateName(byte stateID)
        {
            return stateID switch
            {
                0 => "Lobby",
                1 => "Starting",
                2 => "Playing",
                3 => "Paused",
                4 => "Ended",
                _ => $"Custom_{stateID}"
            };
        }
    }
}
