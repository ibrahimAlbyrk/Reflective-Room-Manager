using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Messages
{
    /// <summary>
    /// Client-to-Server message requesting to kick a member from the party (leader only).
    /// </summary>
    public struct PartyKickMessage : NetworkMessage
    {
        /// <summary>
        /// Party ID to kick the member from.
        /// </summary>
        public uint PartyID;

        /// <summary>
        /// Connection ID of the member to kick.
        /// </summary>
        public int TargetConnectionID;

        public PartyKickMessage(uint partyID, int targetConnectionID)
        {
            PartyID = partyID;
            TargetConnectionID = targetConnectionID;
        }
    }
}
