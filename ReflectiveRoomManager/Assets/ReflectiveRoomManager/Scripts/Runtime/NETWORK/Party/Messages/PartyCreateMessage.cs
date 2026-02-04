using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Messages
{
    /// <summary>
    /// Client-to-Server message requesting party creation.
    /// </summary>
    public struct PartyCreateMessage : NetworkMessage
    {
        /// <summary>
        /// Desired name for the party.
        /// </summary>
        public string PartyName;

        /// <summary>
        /// Desired maximum size for the party.
        /// </summary>
        public int MaxSize;

        /// <summary>
        /// Initial party settings.
        /// </summary>
        public bool IsPublic;
        public bool AutoAcceptFriends;
        public bool AllowVoiceChat;

        public PartyCreateMessage(string partyName, int maxSize, bool isPublic = false,
            bool autoAcceptFriends = false, bool allowVoiceChat = true)
        {
            PartyName = partyName;
            MaxSize = maxSize;
            IsPublic = isPublic;
            AutoAcceptFriends = autoAcceptFriends;
            AllowVoiceChat = allowVoiceChat;
        }
    }
}
