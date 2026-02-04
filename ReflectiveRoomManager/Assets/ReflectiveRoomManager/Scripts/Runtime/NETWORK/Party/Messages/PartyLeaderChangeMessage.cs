using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Messages
{
    /// <summary>
    /// Server-to-Client message notifying that the party leader has changed.
    /// </summary>
    public struct PartyLeaderChangeMessage : NetworkMessage
    {
        /// <summary>
        /// Party ID where leadership changed.
        /// </summary>
        public uint PartyID;

        /// <summary>
        /// Connection ID of the new leader.
        /// </summary>
        public int NewLeaderConnectionID;

        /// <summary>
        /// Reason for the leadership change.
        /// </summary>
        public LeaderChangeReason Reason;

        public PartyLeaderChangeMessage(uint partyID, int newLeaderConnectionID, LeaderChangeReason reason)
        {
            PartyID = partyID;
            NewLeaderConnectionID = newLeaderConnectionID;
            Reason = reason;
        }
    }

    /// <summary>
    /// Reason for a party leadership change.
    /// </summary>
    public enum LeaderChangeReason : byte
    {
        /// <summary>
        /// Leader manually transferred leadership.
        /// </summary>
        ManualTransfer,

        /// <summary>
        /// Leader voluntarily left the party.
        /// </summary>
        LeaderLeft,

        /// <summary>
        /// Leader was disconnected from the server.
        /// </summary>
        LeaderDisconnected
    }
}
