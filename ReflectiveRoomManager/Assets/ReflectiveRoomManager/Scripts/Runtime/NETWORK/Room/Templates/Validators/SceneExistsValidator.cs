using System;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Room.Templates;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Validators
{
    /// <summary>
    /// Validates that the scene name is not empty.
    /// Note: Full scene validation (checking if scene exists in build settings)
    /// should be done in editor-only code.
    /// </summary>
    [Serializable]
    public class SceneExistsValidator : ITemplateValidator
    {
        public string ValidatorName => "Scene Exists Validator";

        public bool Validate(RoomTemplate template, RoomTemplateOverride templateOverride, out string error)
        {
            error = null;

            if (template == null)
            {
                error = "Template is null";
                return false;
            }

            if (string.IsNullOrWhiteSpace(template.SceneName))
            {
                error = "Scene name is empty or whitespace";
                return false;
            }

            // Note: In editor, you could add additional validation to check
            // if the scene exists in build settings using EditorBuildSettings.scenes
            // This is a runtime-safe validation that only checks for empty values

            return true;
        }
    }
}
