using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates
{
    using Structs;
    using State;

    /// <summary>
    /// Allows partial override of template settings at room creation.
    /// Only set fields will override the base template.
    /// </summary>
    [Serializable]
    public struct RoomTemplateOverride
    {
        /// <summary>Override room name (null = use prefix + auto-generated ID)</summary>
        public string RoomName;

        /// <summary>Override max players (null = use template default)</summary>
        public int? MaxPlayers;

        /// <summary>Override privacy (null = use template default)</summary>
        public bool? IsPrivate;

        /// <summary>Additional or overridden custom data</summary>
        public Dictionary<string, string> CustomDataOverride;

        /// <summary>Override state config values (optional)</summary>
        public RoomStateConfigOverride StateConfigOverride;

        /// <summary>
        /// Applies override to template and generates final RoomInfo.
        /// </summary>
        public RoomInfo BuildRoomInfo(RoomTemplate template)
        {
            if (template == null)
            {
                Debug.LogError("[RoomTemplateOverride] Cannot build RoomInfo from null template");
                return default;
            }

            var roomInfo = new RoomInfo
            {
                // Use override room name or generate from prefix
                RoomName = !string.IsNullOrEmpty(RoomName)
                    ? RoomName
                    : $"{template.DefaultRoomNamePrefix}_{GenerateRandomSuffix()}",

                // Use override or template default
                MaxPlayers = MaxPlayers ?? template.DefaultMaxPlayers,
                IsPrivate = IsPrivate ?? template.IsPrivateByDefault,

                // Scene from template (not overridable)
                SceneName = template.SceneName,

                // Merge custom data
                CustomData = MergeCustomData(template)
            };

            // Store template ID in custom data for tracking
            roomInfo.CustomData["templateID"] = template.TemplateID.ToString();

            // Store state config override if present
            if (HasStateConfigOverride())
            {
                roomInfo.CustomData["stateConfig.override"] = SerializeStateConfigOverride();
            }

            return roomInfo;
        }

        /// <summary>
        /// Merges template custom data with override.
        /// Override values take precedence.
        /// </summary>
        private Dictionary<string, string> MergeCustomData(RoomTemplate template)
        {
            var merged = template.GetDefaultCustomData();

            if (CustomDataOverride != null)
            {
                foreach (var kvp in CustomDataOverride)
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }

            return merged;
        }

        /// <summary>
        /// Checks if any state config override is set.
        /// </summary>
        public bool HasStateConfigOverride()
        {
            return StateConfigOverride.MinPlayersToStart.HasValue ||
                   StateConfigOverride.LobbyCountdownDuration.HasValue ||
                   StateConfigOverride.StartingCountdownDuration.HasValue ||
                   StateConfigOverride.MaxGameDuration.HasValue ||
                   StateConfigOverride.PauseTimeout.HasValue ||
                   StateConfigOverride.EndScreenDuration.HasValue;
        }

        /// <summary>
        /// Serializes state config override to string for CustomData storage.
        /// </summary>
        private string SerializeStateConfigOverride()
        {
            var parts = new List<string>();

            if (StateConfigOverride.MinPlayersToStart.HasValue)
                parts.Add($"minPlayers:{StateConfigOverride.MinPlayersToStart.Value}");

            if (StateConfigOverride.LobbyCountdownDuration.HasValue)
                parts.Add($"lobbyCountdown:{StateConfigOverride.LobbyCountdownDuration.Value.ToString(CultureInfo.InvariantCulture)}");

            if (StateConfigOverride.StartingCountdownDuration.HasValue)
                parts.Add($"startingCountdown:{StateConfigOverride.StartingCountdownDuration.Value.ToString(CultureInfo.InvariantCulture)}");

            if (StateConfigOverride.MaxGameDuration.HasValue)
                parts.Add($"maxDuration:{StateConfigOverride.MaxGameDuration.Value.ToString(CultureInfo.InvariantCulture)}");

            if (StateConfigOverride.PauseTimeout.HasValue)
                parts.Add($"pauseTimeout:{StateConfigOverride.PauseTimeout.Value.ToString(CultureInfo.InvariantCulture)}");

            if (StateConfigOverride.EndScreenDuration.HasValue)
                parts.Add($"endScreenDuration:{StateConfigOverride.EndScreenDuration.Value.ToString(CultureInfo.InvariantCulture)}");

            return string.Join(";", parts);
        }

        private static string GenerateRandomSuffix()
        {
            return UnityEngine.Random.Range(1000, 9999).ToString();
        }

        /// <summary>
        /// Creates an empty override (uses all template defaults).
        /// </summary>
        public static RoomTemplateOverride Empty => new RoomTemplateOverride();
    }
}
