using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Messages
{
    /// <summary>
    /// Server response with available templates.
    /// </summary>
    public struct TemplateListMessage : NetworkMessage
    {
        public TemplateData[] Templates;

        public TemplateListMessage(TemplateData[] templates)
        {
            Templates = templates ?? System.Array.Empty<TemplateData>();
        }

        /// <summary>
        /// Number of templates in this message.
        /// </summary>
        public int Count => Templates?.Length ?? 0;
    }
}
