using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates
{
    using Structs;

    /// <summary>
    /// Manages room templates at runtime.
    /// Initialized by RoomManagerBase when template system is enabled.
    /// </summary>
    public class RoomTemplateManager
    {
        private readonly Dictionary<uint, RoomTemplate> _templates;
        private readonly Dictionary<RoomTemplateCategory, List<RoomTemplate>> _templatesByCategory;
        private readonly TemplateLibrary _library;

        /// <summary>
        /// Creates a new RoomTemplateManager from a template library.
        /// </summary>
        /// <param name="library">The template library to load from.</param>
        public RoomTemplateManager(TemplateLibrary library)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
            _templates = new Dictionary<uint, RoomTemplate>();
            _templatesByCategory = new Dictionary<RoomTemplateCategory, List<RoomTemplate>>();

            Initialize();
        }

        /// <summary>
        /// Number of registered templates.
        /// </summary>
        public int Count => _templates.Count;

        /// <summary>
        /// Loads all templates from library.
        /// </summary>
        private void Initialize()
        {
            foreach (var template in _library.Templates)
            {
                RegisterTemplate(template);
            }

            Debug.Log($"[RoomTemplateManager] Loaded {_templates.Count} templates");
        }

        /// <summary>
        /// Registers a template at runtime.
        /// </summary>
        public void RegisterTemplate(RoomTemplate template)
        {
            if (template == null)
            {
                Debug.LogWarning("[RoomTemplateManager] Cannot register null template");
                return;
            }

            if (_templates.ContainsKey(template.TemplateID))
            {
                Debug.LogWarning($"[RoomTemplateManager] Template ID {template.TemplateID} already registered, skipping '{template.TemplateName}'");
                return;
            }

            _templates[template.TemplateID] = template;

            // Add to category list
            if (!_templatesByCategory.ContainsKey(template.Category))
            {
                _templatesByCategory[template.Category] = new List<RoomTemplate>();
            }
            _templatesByCategory[template.Category].Add(template);
        }

        /// <summary>
        /// Unregisters a template.
        /// </summary>
        public bool UnregisterTemplate(uint templateID)
        {
            if (!_templates.TryGetValue(templateID, out var template))
                return false;

            _templates.Remove(templateID);

            if (_templatesByCategory.TryGetValue(template.Category, out var categoryList))
            {
                categoryList.Remove(template);
            }

            return true;
        }

        /// <summary>
        /// Gets template by ID.
        /// </summary>
        public RoomTemplate GetTemplate(uint templateID)
        {
            _templates.TryGetValue(templateID, out var template);
            return template;
        }

        /// <summary>
        /// Gets template by name (case-insensitive).
        /// </summary>
        public RoomTemplate GetTemplateByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return _templates.Values.FirstOrDefault(t =>
                string.Equals(t.TemplateName, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all registered templates.
        /// </summary>
        public List<RoomTemplate> GetAllTemplates()
        {
            return _templates.Values.ToList();
        }

        /// <summary>
        /// Gets templates filtered by category.
        /// </summary>
        public List<RoomTemplate> GetTemplatesByCategory(RoomTemplateCategory category)
        {
            if (_templatesByCategory.TryGetValue(category, out var templates))
                return new List<RoomTemplate>(templates);
            return new List<RoomTemplate>();
        }

        /// <summary>
        /// Checks if a template exists.
        /// </summary>
        public bool HasTemplate(uint templateID)
        {
            return _templates.ContainsKey(templateID);
        }

        /// <summary>
        /// Validates template with override.
        /// </summary>
        public bool ValidateTemplate(RoomTemplate template, RoomTemplateOverride templateOverride, out string error)
        {
            error = null;

            if (template == null)
            {
                error = "Template is null";
                return false;
            }

            // Template self-validation with validators
            if (!template.ValidateWithOverride(templateOverride, out error))
                return false;

            // Override-specific validation
            if (templateOverride.MaxPlayers.HasValue)
            {
                int maxPlayers = templateOverride.MaxPlayers.Value;
                if (maxPlayers < template.MinimumPlayers || maxPlayers > template.MaximumPlayers)
                {
                    error = $"Max players override ({maxPlayers}) out of range [{template.MinimumPlayers}, {template.MaximumPlayers}]";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates RoomInfo from template + override.
        /// Primary method used by RoomManager.
        /// </summary>
        public RoomInfo CreateRoomInfoFromTemplate(uint templateID, RoomTemplateOverride templateOverride, out string error)
        {
            error = null;

            var template = GetTemplate(templateID);
            if (template == null)
            {
                error = $"Template ID {templateID} not found";
                return default;
            }

            if (!ValidateTemplate(template, templateOverride, out error))
            {
                return default;
            }

            return templateOverride.BuildRoomInfo(template);
        }

        /// <summary>
        /// Creates RoomInfo using template defaults (no override).
        /// </summary>
        public RoomInfo CreateRoomInfoFromTemplate(uint templateID, out string error)
        {
            return CreateRoomInfoFromTemplate(templateID, RoomTemplateOverride.Empty, out error);
        }
    }
}
