using System;

namespace REFLECTIVE.Runtime.NETWORK.Party
{
    /// <summary>
    /// Configurable settings for a party instance.
    /// Can be modified by the party leader.
    /// </summary>
    [Serializable]
    public class PartySettings
    {
        /// <summary>
        /// Whether the party is visible to other players for joining.
        /// </summary>
        public bool IsPublic;

        /// <summary>
        /// Automatically accept invites from friends.
        /// </summary>
        public bool AutoAcceptFriends;

        /// <summary>
        /// Allow voice chat within the party (external integration point).
        /// </summary>
        public bool AllowVoiceChat = true;

        /// <summary>
        /// Custom invite timeout in seconds. Uses config default if 0.
        /// </summary>
        public int InviteTimeoutSeconds;

        public PartySettings()
        {
            IsPublic = false;
            AutoAcceptFriends = false;
            AllowVoiceChat = true;
            InviteTimeoutSeconds = 0;
        }

        public PartySettings(PartySettings other)
        {
            IsPublic = other.IsPublic;
            AutoAcceptFriends = other.AutoAcceptFriends;
            AllowVoiceChat = other.AllowVoiceChat;
            InviteTimeoutSeconds = other.InviteTimeoutSeconds;
        }
    }
}
