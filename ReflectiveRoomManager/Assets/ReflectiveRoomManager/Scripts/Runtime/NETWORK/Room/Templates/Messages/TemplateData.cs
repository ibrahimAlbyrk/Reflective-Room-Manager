using System;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Messages
{
    /// <summary>
    /// Network-serializable template data.
    /// Lightweight version of RoomTemplate for client display.
    /// </summary>
    [Serializable]
    public struct TemplateData
    {
        public uint TemplateID;
        public string TemplateName;
        public string Description;
        public string IconPath;
        public RoomTemplateCategory Category;
        public int DefaultMaxPlayers;
        public int MinimumPlayers;
        public int MaximumPlayers;
        public bool IsPrivateByDefault;
        public bool AllowSpectators;
        public bool EnableChat;

        /// <summary>
        /// Creates TemplateData from a RoomTemplate.
        /// </summary>
        public static TemplateData FromTemplate(RoomTemplate template)
        {
            if (template == null)
                return default;

            return new TemplateData
            {
                TemplateID = template.TemplateID,
                TemplateName = template.TemplateName,
                Description = template.Description,
                IconPath = template.Icon != null ? template.Icon.name : string.Empty,
                Category = template.Category,
                DefaultMaxPlayers = template.DefaultMaxPlayers,
                MinimumPlayers = template.MinimumPlayers,
                MaximumPlayers = template.MaximumPlayers,
                IsPrivateByDefault = template.IsPrivateByDefault,
                AllowSpectators = template.AllowSpectators,
                EnableChat = template.EnableChat
            };
        }
    }
}
