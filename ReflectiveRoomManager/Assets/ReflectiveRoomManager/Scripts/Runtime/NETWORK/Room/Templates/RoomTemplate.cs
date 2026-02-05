using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates
{
    using State;
    using Team.Config;

    /// <summary>
    /// ScriptableObject template for room configuration.
    /// Defines preset room settings, state config, team setup, and validators.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomTemplate", menuName = "REFLECTIVE/Room Template", order = 1)]
    public class RoomTemplate : ScriptableObject
    {
        [Header("Template Identity")]
        [Tooltip("Unique template ID (auto-generated, do not modify)")]
        [SerializeField] private uint _templateID;

        [Tooltip("Display name for this template")]
        [SerializeField] private string _templateName = "New Template";

        [Tooltip("Short description of this template")]
        [TextArea(2, 4)]
        [SerializeField] private string _description = "";

        [Tooltip("Icon for UI display")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Category for filtering")]
        [SerializeField] private RoomTemplateCategory _category = RoomTemplateCategory.Custom;

        [Header("Room Defaults")]
        [Tooltip("Prefix for auto-generated room names (e.g., 'QuickMatch_')")]
        [SerializeField] private string _defaultRoomNamePrefix = "Room";

        [Tooltip("Scene to load for this room type")]
        [SerializeField] private string _sceneName;

        [Tooltip("Default maximum player count")]
        [Range(1, 64)]
        [SerializeField] private int _defaultMaxPlayers = 8;

        [Tooltip("Minimum players required (validation)")]
        [Range(1, 64)]
        [SerializeField] private int _minimumPlayers = 1;

        [Tooltip("Maximum players allowed (validation)")]
        [Range(1, 64)]
        [SerializeField] private int _maximumPlayers = 64;

        [Tooltip("Room is private by default")]
        [SerializeField] private bool _isPrivateByDefault;

        [Header("Feature Configuration")]
        [Tooltip("State machine configuration (optional)")]
        [SerializeField] private RoomStateConfig _stateConfig;

        [Tooltip("Team configuration (optional)")]
        [SerializeField] private TeamConfig _teamConfig;

        [Header("Spectator Settings")]
        [Tooltip("Allow spectators in this room type")]
        [SerializeField] private bool _allowSpectators;

        [Tooltip("Maximum spectators (if enabled)")]
        [Range(0, 50)]
        [SerializeField] private int _maxSpectators;

        [Header("Chat Settings")]
        [Tooltip("Enable chat in this room type")]
        [SerializeField] private bool _enableChat = true;

        [Tooltip("Enable team-only chat")]
        [SerializeField] private bool _enableTeamChat;

        [Header("Custom Data")]
        [Tooltip("Default custom data key-value pairs")]
        [SerializeField] private CustomDataEntry[] _defaultCustomData = new CustomDataEntry[0];

        [Header("Validation")]
        [Tooltip("Custom validators for this template")]
        [SerializeReference] private List<ITemplateValidator> _validators = new List<ITemplateValidator>();

        #region Public Properties

        public uint TemplateID => _templateID;
        public string TemplateName => _templateName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public RoomTemplateCategory Category => _category;
        public string DefaultRoomNamePrefix => _defaultRoomNamePrefix;
        public string SceneName => _sceneName;
        public int DefaultMaxPlayers => _defaultMaxPlayers;
        public int MinimumPlayers => _minimumPlayers;
        public int MaximumPlayers => _maximumPlayers;
        public bool IsPrivateByDefault => _isPrivateByDefault;
        public RoomStateConfig StateConfig => _stateConfig;
        public TeamConfig TeamConfig => _teamConfig;
        public bool AllowSpectators => _allowSpectators;
        public int MaxSpectators => _maxSpectators;
        public bool EnableChat => _enableChat;
        public bool EnableTeamChat => _enableTeamChat;
        public IReadOnlyList<ITemplateValidator> Validators => _validators;

        #endregion

        /// <summary>
        /// Gets default custom data as dictionary.
        /// </summary>
        public Dictionary<string, string> GetDefaultCustomData()
        {
            var result = new Dictionary<string, string>();
            if (_defaultCustomData == null) return result;

            foreach (var entry in _defaultCustomData)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    result[entry.Key] = entry.Value ?? string.Empty;
                }
            }
            return result;
        }

        /// <summary>
        /// Validates template configuration.
        /// </summary>
        public bool Validate(out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(_templateName))
            {
                error = "Template name is required";
                return false;
            }

            if (string.IsNullOrEmpty(_sceneName))
            {
                error = "Scene name is required";
                return false;
            }

            if (_defaultMaxPlayers < _minimumPlayers)
            {
                error = $"Default max players ({_defaultMaxPlayers}) cannot be less than minimum ({_minimumPlayers})";
                return false;
            }

            if (_defaultMaxPlayers > _maximumPlayers)
            {
                error = $"Default max players ({_defaultMaxPlayers}) cannot exceed maximum ({_maximumPlayers})";
                return false;
            }

            if (_allowSpectators && _maxSpectators <= 0)
            {
                error = "Max spectators must be > 0 if spectators are allowed";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Runs custom validators with override.
        /// </summary>
        public bool ValidateWithOverride(RoomTemplateOverride templateOverride, out string error)
        {
            error = null;

            // Base validation
            if (!Validate(out error))
                return false;

            // Custom validators
            if (_validators != null)
            {
                foreach (var validator in _validators)
                {
                    if (validator != null && !validator.Validate(this, templateOverride, out error))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generates unique template ID (editor-only).
        /// </summary>
        public void GenerateTemplateID()
        {
            if (_templateID == 0)
            {
                _templateID = (uint)Random.Range(1000, 999999);
            }
        }

        private void OnValidate()
        {
            // Auto-generate ID if missing
            if (_templateID == 0)
            {
                GenerateTemplateID();
            }

            // Clamp values
            _defaultMaxPlayers = Mathf.Clamp(_defaultMaxPlayers, 1, 64);
            _minimumPlayers = Mathf.Clamp(_minimumPlayers, 1, 64);
            _maximumPlayers = Mathf.Clamp(_maximumPlayers, 1, 64);
            _maxSpectators = Mathf.Clamp(_maxSpectators, 0, 50);

            // Ensure minimum <= default <= maximum
            if (_minimumPlayers > _maximumPlayers)
            {
                _minimumPlayers = _maximumPlayers;
            }

            if (_defaultMaxPlayers < _minimumPlayers)
            {
                _defaultMaxPlayers = _minimumPlayers;
            }

            if (_defaultMaxPlayers > _maximumPlayers)
            {
                _defaultMaxPlayers = _maximumPlayers;
            }
        }
    }

    /// <summary>
    /// Serializable custom data entry for Inspector.
    /// </summary>
    [System.Serializable]
    public struct CustomDataEntry
    {
        public string Key;
        [TextArea(1, 3)]
        public string Value;
    }
}
