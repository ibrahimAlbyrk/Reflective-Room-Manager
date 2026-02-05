using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates
{
    /// <summary>
    /// ScriptableObject that holds collection of room templates.
    /// Loaded at runtime to populate RoomTemplateManager.
    /// </summary>
    [CreateAssetMenu(fileName = "TemplateLibrary", menuName = "REFLECTIVE/Template Library", order = 0)]
    public class TemplateLibrary : ScriptableObject
    {
        [Tooltip("All available room templates")]
        [SerializeField] private List<RoomTemplate> _templates = new List<RoomTemplate>();

        /// <summary>
        /// All templates in this library.
        /// </summary>
        public IReadOnlyList<RoomTemplate> Templates => _templates;

        /// <summary>
        /// Number of templates in this library.
        /// </summary>
        public int Count => _templates.Count;

        /// <summary>
        /// Gets template at index.
        /// </summary>
        public RoomTemplate this[int index] => _templates[index];

        /// <summary>
        /// Validates all templates in library.
        /// </summary>
        /// <param name="errors">List of validation errors.</param>
        /// <returns>True if all templates are valid.</returns>
        public bool ValidateAll(out List<string> errors)
        {
            errors = new List<string>();
            bool allValid = true;

            foreach (var template in _templates)
            {
                if (template == null)
                {
                    errors.Add("[null] Template reference is null");
                    allValid = false;
                    continue;
                }

                if (!template.Validate(out string error))
                {
                    errors.Add($"[{template.TemplateName}] {error}");
                    allValid = false;
                }
            }

            return allValid;
        }

        /// <summary>
        /// Gets template by ID.
        /// </summary>
        public RoomTemplate GetByID(uint templateID)
        {
            foreach (var template in _templates)
            {
                if (template != null && template.TemplateID == templateID)
                    return template;
            }
            return null;
        }

        /// <summary>
        /// Checks if library contains a template with the given ID.
        /// </summary>
        public bool ContainsID(uint templateID)
        {
            return GetByID(templateID) != null;
        }

        private void OnValidate()
        {
            // Remove null entries
            _templates.RemoveAll(t => t == null);

            // Check for duplicate IDs
            var ids = new HashSet<uint>();
            foreach (var template in _templates)
            {
                if (template == null) continue;

                if (ids.Contains(template.TemplateID))
                {
                    Debug.LogWarning($"[TemplateLibrary] Duplicate template ID detected: {template.TemplateID} ({template.TemplateName})");
                }
                else
                {
                    ids.Add(template.TemplateID);
                }
            }
        }
    }
}
