using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Messages
{
    /// <summary>
    /// Client request to create room from template.
    /// </summary>
    public struct CreateRoomFromTemplateMessage : NetworkMessage
    {
        public uint TemplateID;
        public RoomTemplateOverrideData Override;

        public CreateRoomFromTemplateMessage(uint templateID)
        {
            TemplateID = templateID;
            Override = new RoomTemplateOverrideData { MaxPlayers = -1 };
        }

        public CreateRoomFromTemplateMessage(uint templateID, RoomTemplateOverrideData overrideData)
        {
            TemplateID = templateID;
            Override = overrideData;
        }

        /// <summary>
        /// Creates a message from a template ID and RoomTemplateOverride.
        /// </summary>
        public static CreateRoomFromTemplateMessage Create(uint templateID, RoomTemplateOverride templateOverride)
        {
            return new CreateRoomFromTemplateMessage
            {
                TemplateID = templateID,
                Override = RoomTemplateOverrideData.FromRoomTemplateOverride(templateOverride)
            };
        }
    }
}
