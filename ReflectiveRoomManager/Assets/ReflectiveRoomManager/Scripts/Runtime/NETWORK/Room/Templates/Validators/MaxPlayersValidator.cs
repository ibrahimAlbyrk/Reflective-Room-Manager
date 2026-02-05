using System;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Room.Templates;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Validators
{
    /// <summary>
    /// Validates max player count is within template limits.
    /// </summary>
    [Serializable]
    public class MaxPlayersValidator : ITemplateValidator
    {
        [SerializeField] private int _absoluteMin = 1;
        [SerializeField] private int _absoluteMax = 64;

        public string ValidatorName => "Max Players Validator";

        public bool Validate(RoomTemplate template, RoomTemplateOverride templateOverride, out string error)
        {
            error = null;

            if (template == null)
            {
                error = "Template is null";
                return false;
            }

            // Check template's default max players
            if (template.DefaultMaxPlayers < _absoluteMin || template.DefaultMaxPlayers > _absoluteMax)
            {
                error = $"Template default max players ({template.DefaultMaxPlayers}) out of absolute range [{_absoluteMin}, {_absoluteMax}]";
                return false;
            }

            // Check override if present
            if (templateOverride.MaxPlayers.HasValue)
            {
                int overrideValue = templateOverride.MaxPlayers.Value;

                // Check against absolute limits
                if (overrideValue < _absoluteMin || overrideValue > _absoluteMax)
                {
                    error = $"Override max players ({overrideValue}) out of absolute range [{_absoluteMin}, {_absoluteMax}]";
                    return false;
                }

                // Check against template limits
                if (overrideValue < template.MinimumPlayers)
                {
                    error = $"Override max players ({overrideValue}) below template minimum ({template.MinimumPlayers})";
                    return false;
                }

                if (overrideValue > template.MaximumPlayers)
                {
                    error = $"Override max players ({overrideValue}) exceeds template maximum ({template.MaximumPlayers})";
                    return false;
                }
            }

            return true;
        }
    }
}
