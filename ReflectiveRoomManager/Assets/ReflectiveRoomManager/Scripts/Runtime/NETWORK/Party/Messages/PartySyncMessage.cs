using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Messages
{
    /// <summary>
    /// Server-to-Client message synchronizing party state.
    /// </summary>
    public struct PartySyncMessage : NetworkMessage
    {
        /// <summary>
        /// Party ID being synced.
        /// </summary>
        public uint PartyID;

        /// <summary>
        /// Party name.
        /// </summary>
        public string PartyName;

        /// <summary>
        /// Maximum party size.
        /// </summary>
        public int MaxSize;

        /// <summary>
        /// Connection ID of the leader.
        /// </summary>
        public int LeaderConnectionID;

        /// <summary>
        /// All party members.
        /// </summary>
        public PartyMemberData[] Members;

        /// <summary>
        /// Party settings.
        /// </summary>
        public bool IsPublic;
        public bool AutoAcceptFriends;
        public bool AllowVoiceChat;

        public PartySyncMessage(uint partyID, string partyName, int maxSize, int leaderConnectionID,
            PartyMemberData[] members, bool isPublic, bool autoAcceptFriends, bool allowVoiceChat)
        {
            PartyID = partyID;
            PartyName = partyName;
            MaxSize = maxSize;
            LeaderConnectionID = leaderConnectionID;
            Members = members;
            IsPublic = isPublic;
            AutoAcceptFriends = autoAcceptFriends;
            AllowVoiceChat = allowVoiceChat;
        }
    }

    /// <summary>
    /// Serializable party member data for network transmission.
    /// </summary>
    public struct PartyMemberData
    {
        public int ConnectionID;
        public string PlayerName;
        public bool IsReady;

        public PartyMemberData(int connectionID, string playerName, bool isReady)
        {
            ConnectionID = connectionID;
            PlayerName = playerName;
            IsReady = isReady;
        }
    }
}
