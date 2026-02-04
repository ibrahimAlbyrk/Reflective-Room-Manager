using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Messages
{
    /// <summary>
    /// Client-to-Server message requesting to leave a party.
    /// </summary>
    public struct PartyLeaveMessage : NetworkMessage
    {
        /// <summary>
        /// Party ID to leave.
        /// </summary>
        public uint PartyID;

        public PartyLeaveMessage(uint partyID)
        {
            PartyID = partyID;
        }
    }
}
