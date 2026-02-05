namespace REFLECTIVE.Runtime.NETWORK.Room.Templates
{
    /// <summary>
    /// Interface for custom template validators.
    /// Implement to add validation rules for room templates.
    /// </summary>
    public interface ITemplateValidator
    {
        /// <summary>
        /// Display name for this validator.
        /// </summary>
        string ValidatorName { get; }

        /// <summary>
        /// Validates the template with optional override.
        /// </summary>
        /// <param name="template">The template to validate.</param>
        /// <param name="templateOverride">Optional override values.</param>
        /// <param name="error">Error message if validation fails.</param>
        /// <returns>True if validation passes.</returns>
        bool Validate(RoomTemplate template, RoomTemplateOverride templateOverride, out string error);
    }
}
