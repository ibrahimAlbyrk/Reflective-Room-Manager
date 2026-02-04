using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Messages
{
    /// <summary>
    /// Server-to-Client message notifying a player they have been invited to a party.
    /// </summary>
    public struct PartyInviteNotificationMessage : NetworkMessage
    {
        /// <summary>
        /// Party ID the invite is for.
        /// </summary>
        public uint PartyID;

        /// <summary>
        /// Name of the player who sent the invite.
        /// </summary>
        public string InviterName;

        /// <summary>
        /// Name of the party.
        /// </summary>
        public string PartyName;

        /// <summary>
        /// Seconds until the invite expires.
        /// </summary>
        public float SecondsUntilExpiry;

        public PartyInviteNotificationMessage(uint partyID, string inviterName, string partyName, float secondsUntilExpiry)
        {
            PartyID = partyID;
            InviterName = inviterName;
            PartyName = partyName;
            SecondsUntilExpiry = secondsUntilExpiry;
        }
    }
}
