using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.State
{
    using Events;

    /// <summary>
    /// Context object providing state access to config, events, and shared data.
    /// Injected into state lifecycle methods.
    /// </summary>
    public class RoomStateContext
    {
        public RoomStateConfig Config { get; }
        public RoomEventManager EventManager { get; }
        public RoomStateEffectiveConfig EffectiveConfig { get; private set; }

        /// <summary>Time elapsed since entering current state</summary>
        public float StateElapsedTime { get; private set; }

        /// <summary>Persistent data (survives state transitions)</summary>
        private readonly Dictionary<string, object> _persistentData;

        /// <summary>Transient data (cleared on state transition)</summary>
        private readonly Dictionary<string, object> _transientData;

        public RoomStateContext(RoomStateConfig config, RoomEventManager eventManager)
        {
            Config = config;
            EventManager = eventManager;
            StateElapsedTime = 0f;
            _persistentData = new Dictionary<string, object>();
            _transientData = new Dictionary<string, object>();

            // Default effective config from base config
            EffectiveConfig = new RoomStateEffectiveConfig
            {
                MinPlayersToStart = config.MinPlayersToStart,
                LobbyCountdownDuration = config.LobbyCountdownDuration,
                StartingCountdownDuration = config.StartingCountdownDuration,
                MaxGameDuration = config.MaxGameDuration,
                AllowPausing = config.AllowPausing,
                AllowJoinDuringPlay = config.AllowJoinDuringPlay,
                PauseTimeout = config.PauseTimeout,
                EndScreenDuration = config.EndScreenDuration,
                AutoReturnToLobby = config.AutoReturnToLobby,
                AutoCloseRoomOnEnd = config.AutoCloseRoomOnEnd,
                RequireAllPlayersReady = config.RequireAllPlayersReady,
                EnableDebugLogs = config.EnableDebugLogs
            };
        }

        /// <summary>
        /// Applies per-room config override.
        /// </summary>
        public void ApplyOverride(RoomStateConfigOverride configOverride)
        {
            EffectiveConfig = configOverride.MergeWith(Config);
        }

        public void IncrementStateTime(float deltaTime)
        {
            StateElapsedTime += deltaTime;
        }

        public void ResetStateTime()
        {
            StateElapsedTime = 0f;
        }

        /// <summary>Gets data from transient storage (current state only)</summary>
        public T GetTransientData<T>(string key, T defaultValue = default)
        {
            if (_transientData.TryGetValue(key, out var value))
                return (T)value;
            return defaultValue;
        }

        /// <summary>Sets transient data (cleared on state change)</summary>
        public void SetTransientData(string key, object value)
        {
            _transientData[key] = value;
        }

        /// <summary>Gets data from persistent storage (survives state changes)</summary>
        public T GetPersistentData<T>(string key, T defaultValue = default)
        {
            if (_persistentData.TryGetValue(key, out var value))
                return (T)value;
            return defaultValue;
        }

        /// <summary>Sets persistent data (survives state changes)</summary>
        public void SetPersistentData(string key, object value)
        {
            _persistentData[key] = value;
        }

        public void ClearTransientData()
        {
            _transientData.Clear();
        }

        public void ClearAllData()
        {
            _persistentData.Clear();
            _transientData.Clear();
        }

        /// <summary>Converts context data to network-serializable format</summary>
        public Dictionary<string, string> SerializePersistentData()
        {
            var result = new Dictionary<string, string>();
            foreach (var kvp in _persistentData)
            {
                result[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
            }
            return result;
        }

        /// <summary>Restores context from network data</summary>
        public void RestoreFromData(RoomStateData data)
        {
            StateElapsedTime = data.ElapsedTime;

            if (data.Data == null) return;

            foreach (var kvp in data.Data)
            {
                _persistentData[kvp.Key] = kvp.Value;
            }
        }
    }
}
