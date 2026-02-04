using System;
using System.Collections.Generic;
using System.Globalization;

namespace REFLECTIVE.Runtime.NETWORK.Room.State
{
    /// <summary>
    /// Per-room config overrides.
    /// Only set fields will override the base RoomStateConfig.
    /// Use case: Tournament rooms with different timeouts.
    /// </summary>
    [Serializable]
    public struct RoomStateConfigOverride
    {
        /// <summary>Override minimum players to start (null = use base config)</summary>
        public int? MinPlayersToStart;

        /// <summary>Override lobby countdown duration</summary>
        public float? LobbyCountdownDuration;

        /// <summary>Override starting countdown duration</summary>
        public float? StartingCountdownDuration;

        /// <summary>Override max game duration</summary>
        public float? MaxGameDuration;

        /// <summary>Override pause timeout</summary>
        public float? PauseTimeout;

        /// <summary>Override end screen duration</summary>
        public float? EndScreenDuration;

        /// <summary>
        /// Creates effective config by merging override with base.
        /// </summary>
        public RoomStateEffectiveConfig MergeWith(RoomStateConfig baseConfig)
        {
            return new RoomStateEffectiveConfig
            {
                MinPlayersToStart = MinPlayersToStart ?? baseConfig.MinPlayersToStart,
                LobbyCountdownDuration = LobbyCountdownDuration ?? baseConfig.LobbyCountdownDuration,
                StartingCountdownDuration = StartingCountdownDuration ?? baseConfig.StartingCountdownDuration,
                MaxGameDuration = MaxGameDuration ?? baseConfig.MaxGameDuration,
                AllowPausing = baseConfig.AllowPausing,
                AllowJoinDuringPlay = baseConfig.AllowJoinDuringPlay,
                PauseTimeout = PauseTimeout ?? baseConfig.PauseTimeout,
                EndScreenDuration = EndScreenDuration ?? baseConfig.EndScreenDuration,
                AutoReturnToLobby = baseConfig.AutoReturnToLobby,
                AutoCloseRoomOnEnd = baseConfig.AutoCloseRoomOnEnd,
                RequireAllPlayersReady = baseConfig.RequireAllPlayersReady,
                EnableDebugLogs = baseConfig.EnableDebugLogs
            };
        }

        /// <summary>
        /// Creates override from CustomData dictionary.
        /// Use case: Client sends override values in room creation request.
        /// </summary>
        public static RoomStateConfigOverride FromCustomData(Dictionary<string, string> data)
        {
            var result = new RoomStateConfigOverride();

            if (data.TryGetValue("stateConfig.minPlayers", out var minPlayers) &&
                int.TryParse(minPlayers, out var minPlayersValue))
            {
                result.MinPlayersToStart = minPlayersValue;
            }

            if (data.TryGetValue("stateConfig.lobbyCountdown", out var lobbyCountdown) &&
                float.TryParse(lobbyCountdown, NumberStyles.Float, CultureInfo.InvariantCulture, out var lobbyCountdownValue))
            {
                result.LobbyCountdownDuration = lobbyCountdownValue;
            }

            if (data.TryGetValue("stateConfig.startingCountdown", out var startingCountdown) &&
                float.TryParse(startingCountdown, NumberStyles.Float, CultureInfo.InvariantCulture, out var startingCountdownValue))
            {
                result.StartingCountdownDuration = startingCountdownValue;
            }

            if (data.TryGetValue("stateConfig.maxDuration", out var maxDuration) &&
                float.TryParse(maxDuration, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDurationValue))
            {
                result.MaxGameDuration = maxDurationValue;
            }

            if (data.TryGetValue("stateConfig.pauseTimeout", out var pauseTimeout) &&
                float.TryParse(pauseTimeout, NumberStyles.Float, CultureInfo.InvariantCulture, out var pauseTimeoutValue))
            {
                result.PauseTimeout = pauseTimeoutValue;
            }

            if (data.TryGetValue("stateConfig.endScreenDuration", out var endScreenDuration) &&
                float.TryParse(endScreenDuration, NumberStyles.Float, CultureInfo.InvariantCulture, out var endScreenDurationValue))
            {
                result.EndScreenDuration = endScreenDurationValue;
            }

            return result;
        }
    }

    /// <summary>
    /// Effective config after merging override with base.
    /// Used internally by RoomStateContext.
    /// </summary>
    public struct RoomStateEffectiveConfig
    {
        public int MinPlayersToStart;
        public float LobbyCountdownDuration;
        public float StartingCountdownDuration;
        public float MaxGameDuration;
        public bool AllowPausing;
        public bool AllowJoinDuringPlay;
        public float PauseTimeout;
        public float EndScreenDuration;
        public bool AutoReturnToLobby;
        public bool AutoCloseRoomOnEnd;
        public bool RequireAllPlayersReady;
        public bool EnableDebugLogs;
    }
}
