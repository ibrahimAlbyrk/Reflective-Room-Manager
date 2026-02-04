using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Messages
{
    /// <summary>
    /// Client-to-Server message requesting to invite a player to the party.
    /// </summary>
    public struct PartyInviteMessage : NetworkMessage
    {
        /// <summary>
        /// Party ID to invite the player to.
        /// </summary>
        public uint PartyID;

        /// <summary>
        /// Connection ID of the target player to invite.
        /// </summary>
        public int TargetConnectionID;

        public PartyInviteMessage(uint partyID, int targetConnectionID)
        {
            PartyID = partyID;
            TargetConnectionID = targetConnectionID;
        }
    }
}
