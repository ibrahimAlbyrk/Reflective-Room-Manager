using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Messages
{
    /// <summary>
    /// Client-to-Server message responding to a party invite.
    /// </summary>
    public struct PartyInviteResponseMessage : NetworkMessage
    {
        /// <summary>
        /// Party ID the response is for.
        /// </summary>
        public uint PartyID;

        /// <summary>
        /// Whether the invite was accepted.
        /// </summary>
        public bool Accepted;

        public PartyInviteResponseMessage(uint partyID, bool accepted)
        {
            PartyID = partyID;
            Accepted = accepted;
        }
    }
}
