using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates
{
    using Messages;

    /// <summary>
    /// Client-side template events.
    /// Subscribe to these events to receive template-related notifications.
    /// </summary>
    public static class TemplateEvents
    {
        /// <summary>
        /// Invoked when template list is received from server.
        /// </summary>
        public static event Action<TemplateData[]> OnTemplateListReceived;

        /// <summary>
        /// Invoked when room creation from template fails.
        /// </summary>
        public static event Action<string> OnRoomCreationError;

        /// <summary>
        /// Invokes the template list received event.
        /// </summary>
        internal static void InvokeTemplateListReceived(TemplateData[] templates)
        {
            OnTemplateListReceived?.Invoke(templates);
        }

        /// <summary>
        /// Invokes the room creation error event.
        /// </summary>
        internal static void InvokeRoomCreationError(string errorMessage)
        {
            OnRoomCreationError?.Invoke(errorMessage);
        }

        /// <summary>
        /// Clears all event subscriptions.
        /// Called during cleanup.
        /// </summary>
        internal static void ClearEvents()
        {
            OnTemplateListReceived = null;
            OnRoomCreationError = null;
        }
    }
}
