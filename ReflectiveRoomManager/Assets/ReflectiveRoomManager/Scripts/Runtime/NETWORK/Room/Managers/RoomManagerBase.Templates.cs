using Mirror;
using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Templates;
    using Templates.Handlers;
    using Templates.Messages;
    using Structs;

    /// <summary>
    /// Partial class for Room Templates system integration in RoomManagerBase.
    /// </summary>
    public abstract partial class RoomManagerBase
    {
        #region Serialize Variables

        [Header("Room Templates")]
        [Tooltip("Enable room template system")]
        [SerializeField] protected bool _enableTemplates;

        [Tooltip("Template library asset")]
        [SerializeField] protected TemplateLibrary _templateLibrary;

        #endregion

        #region Properties

        /// <summary>
        /// Whether the template system is enabled.
        /// </summary>
        public bool EnableTemplates => _enableTemplates;

        /// <summary>
        /// The template library.
        /// </summary>
        public TemplateLibrary TemplateLibrary => _templateLibrary;

        /// <summary>
        /// The template manager instance.
        /// </summary>
        public RoomTemplateManager TemplateManager => m_templateManager;

        #endregion

        #region Private Fields

        protected RoomTemplateManager m_templateManager;
        private bool _templateServerHandlersRegistered;
        private bool _templateClientHandlersRegistered;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the template system.
        /// Called during Awake if template system is enabled.
        /// </summary>
        protected virtual void InitializeTemplateSystem()
        {
            if (!_enableTemplates) return;

            if (_templateLibrary == null)
            {
                Debug.LogError("[RoomManagerBase] Template system enabled but no library assigned!");
                _enableTemplates = false;
                return;
            }

            m_templateManager = new RoomTemplateManager(_templateLibrary);

            // Validate all templates
            if (!_templateLibrary.ValidateAll(out var errors))
            {
                foreach (var error in errors)
                {
                    Debug.LogError($"[RoomManagerBase] Template validation error: {error}");
                }
            }

            Debug.Log($"[RoomManagerBase] Template system initialized with {m_templateManager.Count} templates");
        }

        /// <summary>
        /// Registers template system server network handlers.
        /// Called when server starts.
        /// </summary>
        protected virtual void RegisterTemplateServerHandlers()
        {
            if (!_enableTemplates || _templateServerHandlersRegistered) return;

            TemplateNetworkHandlers.RegisterServerHandlers();
            _templateServerHandlersRegistered = true;
        }

        /// <summary>
        /// Unregisters template system server network handlers.
        /// Called when server stops.
        /// </summary>
        protected virtual void UnregisterTemplateServerHandlers()
        {
            if (!_templateServerHandlersRegistered) return;

            TemplateNetworkHandlers.UnregisterServerHandlers();
            _templateServerHandlersRegistered = false;
        }

        /// <summary>
        /// Registers template system client network handlers.
        /// Called when client starts.
        /// </summary>
        protected virtual void RegisterTemplateClientHandlers()
        {
            if (!_enableTemplates || _templateClientHandlersRegistered) return;

            TemplateNetworkHandlers.RegisterClientHandlers();
            _templateClientHandlersRegistered = true;
        }

        /// <summary>
        /// Unregisters template system client network handlers.
        /// Called when client stops.
        /// </summary>
        protected virtual void UnregisterTemplateClientHandlers()
        {
            if (!_templateClientHandlersRegistered) return;

            TemplateNetworkHandlers.UnregisterClientHandlers();
            _templateClientHandlersRegistered = false;
        }

        #endregion

        #region Template API

        /// <summary>
        /// Creates a room from a template with optional override.
        /// Server-only method.
        /// </summary>
        /// <param name="templateID">The template ID to use.</param>
        /// <param name="templateOverride">Optional override values.</param>
        /// <param name="creator">The connection requesting the room (null for server-created).</param>
        public virtual void CreateRoomFromTemplate(uint templateID, RoomTemplateOverride templateOverride, NetworkConnection creator = null)
        {
            if (!_enableTemplates || m_templateManager == null)
            {
                Debug.LogError("[RoomManagerBase] Template system not enabled");
                if (creator != null)
                {
                    creator.Send(RoomCreationErrorMessage.SystemDisabled());
                }
                return;
            }

            // Get template
            var template = m_templateManager.GetTemplate(templateID);
            if (template == null)
            {
                Debug.LogError($"[RoomManagerBase] Template ID {templateID} not found");
                if (creator != null)
                {
                    creator.Send(RoomCreationErrorMessage.TemplateNotFound(templateID));
                }
                return;
            }

            // Validate and create RoomInfo
            var roomInfo = m_templateManager.CreateRoomInfoFromTemplate(templateID, templateOverride, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[RoomManagerBase] Template validation failed: {error}");
                if (creator != null)
                {
                    creator.Send(RoomCreationErrorMessage.ValidationFailed(error));
                }
                return;
            }

            // Create room using existing method
            CreateRoom(roomInfo, creator);

            // Apply template-specific configuration to the created room
            ApplyTemplateConfiguration(template, templateOverride, roomInfo.RoomName);
        }

        /// <summary>
        /// Creates a room from a template using default settings.
        /// Server-only method.
        /// </summary>
        public void CreateRoomFromTemplate(uint templateID, NetworkConnection creator = null)
        {
            CreateRoomFromTemplate(templateID, RoomTemplateOverride.Empty, creator);
        }

        /// <summary>
        /// Gets a template by ID.
        /// </summary>
        public RoomTemplate GetTemplate(uint templateID)
        {
            return m_templateManager?.GetTemplate(templateID);
        }

        /// <summary>
        /// Gets a template by name.
        /// </summary>
        public RoomTemplate GetTemplateByName(string name)
        {
            return m_templateManager?.GetTemplateByName(name);
        }

        /// <summary>
        /// Gets all templates.
        /// </summary>
        public List<RoomTemplate> GetAllTemplates()
        {
            return m_templateManager?.GetAllTemplates() ?? new List<RoomTemplate>();
        }

        /// <summary>
        /// Gets templates by category.
        /// </summary>
        public List<RoomTemplate> GetTemplatesByCategory(RoomTemplateCategory category)
        {
            return m_templateManager?.GetTemplatesByCategory(category) ?? new List<RoomTemplate>();
        }

        #endregion

        #region Template Configuration

        /// <summary>
        /// Applies template configuration to a created room.
        /// Override to customize template application behavior.
        /// </summary>
        protected virtual void ApplyTemplateConfiguration(RoomTemplate template, RoomTemplateOverride templateOverride, string roomName)
        {
            if (template == null) return;

            var room = GetRoom(roomName);
            if (room == null)
            {
                Debug.LogWarning($"[RoomManagerBase] Could not find room '{roomName}' to apply template configuration");
                return;
            }

            // Apply state config if present and state machine is enabled
            if (_enableStateMachine && template.StateConfig != null)
            {
                // The room is already initialized with state machine in CreateRoom
                // We can apply additional template-specific state config here if needed
                room.UpdateCustomData("template.stateConfig", template.StateConfig.name);
            }

            // Apply team config if present and team system is enabled
            if (_enableTeamSystem && template.TeamConfig != null)
            {
                // Re-initialize team system with template config
                var teamManager = GetRoomTeamManager(room);
                if (teamManager != null)
                {
                    teamManager.Initialize(room, template.TeamConfig);
                }
                room.UpdateCustomData("template.teamConfig", template.TeamConfig.name);
            }

            // Store template reference in room custom data
            room.UpdateCustomData("templateID", template.TemplateID.ToString());
            room.UpdateCustomData("templateName", template.TemplateName);

            // Store additional template settings
            room.UpdateCustomData("template.allowSpectators", template.AllowSpectators.ToString());
            room.UpdateCustomData("template.maxSpectators", template.MaxSpectators.ToString());
            room.UpdateCustomData("template.enableChat", template.EnableChat.ToString());
            room.UpdateCustomData("template.enableTeamChat", template.EnableTeamChat.ToString());

            Debug.Log($"[RoomManagerBase] Applied template '{template.TemplateName}' configuration to room '{roomName}'");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleans up the template system.
        /// Called during OnDestroy.
        /// </summary>
        protected virtual void CleanupTemplateSystem()
        {
            if (_templateServerHandlersRegistered)
            {
                TemplateNetworkHandlers.UnregisterServerHandlers();
                _templateServerHandlersRegistered = false;
            }

            if (_templateClientHandlersRegistered)
            {
                TemplateNetworkHandlers.UnregisterClientHandlers();
                _templateClientHandlersRegistered = false;
            }

            TemplateNetworkHandlers.ClearClientEvents();
            m_templateManager = null;
        }

        #endregion

        #region Client API (Static)

        /// <summary>
        /// Requests the template list from server.
        /// </summary>
        public static void RequestTemplateList()
        {
            if (NetworkClient.connection == null)
            {
                Debug.LogWarning("[RoomManagerBase] Cannot request templates: not connected");
                return;
            }

            NetworkClient.Send(RequestTemplateListMessage.All());
        }

        /// <summary>
        /// Requests templates filtered by category.
        /// </summary>
        public static void RequestTemplateList(RoomTemplateCategory category)
        {
            if (NetworkClient.connection == null)
            {
                Debug.LogWarning("[RoomManagerBase] Cannot request templates: not connected");
                return;
            }

            NetworkClient.Send(RequestTemplateListMessage.ForCategory(category));
        }

        /// <summary>
        /// Sends a request to create room from template.
        /// </summary>
        public static void RequestCreateRoomFromTemplate(uint templateID, RoomTemplateOverride templateOverride)
        {
            if (NetworkClient.connection == null)
            {
                Debug.LogWarning("[RoomManagerBase] Cannot create room: not connected");
                return;
            }

            var msg = CreateRoomFromTemplateMessage.Create(templateID, templateOverride);
            NetworkClient.Send(msg);
        }

        /// <summary>
        /// Sends a request to create room from template with default settings.
        /// </summary>
        public static void RequestCreateRoomFromTemplate(uint templateID)
        {
            RequestCreateRoomFromTemplate(templateID, RoomTemplateOverride.Empty);
        }

        #endregion
    }
}
