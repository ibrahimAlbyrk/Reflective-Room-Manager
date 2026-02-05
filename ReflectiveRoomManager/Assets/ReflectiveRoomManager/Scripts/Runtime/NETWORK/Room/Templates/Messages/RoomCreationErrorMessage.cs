using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Messages
{
    /// <summary>
    /// Server response when room creation fails.
    /// </summary>
    public struct RoomCreationErrorMessage : NetworkMessage
    {
        public string ErrorMessage;

        public RoomCreationErrorMessage(string errorMessage)
        {
            ErrorMessage = errorMessage ?? "Unknown error";
        }

        /// <summary>
        /// Creates an error message for template not found.
        /// </summary>
        public static RoomCreationErrorMessage TemplateNotFound(uint templateID)
        {
            return new RoomCreationErrorMessage($"Template ID {templateID} not found");
        }

        /// <summary>
        /// Creates an error message for validation failure.
        /// </summary>
        public static RoomCreationErrorMessage ValidationFailed(string reason)
        {
            return new RoomCreationErrorMessage($"Validation failed: {reason}");
        }

        /// <summary>
        /// Creates an error message for template system disabled.
        /// </summary>
        public static RoomCreationErrorMessage SystemDisabled()
        {
            return new RoomCreationErrorMessage("Template system is not enabled");
        }
    }
}
