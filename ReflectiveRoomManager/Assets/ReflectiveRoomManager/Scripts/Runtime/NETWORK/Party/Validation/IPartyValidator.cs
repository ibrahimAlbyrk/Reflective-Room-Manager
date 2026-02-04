using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Validation
{
    /// <summary>
    /// Interface for validating party operations.
    /// Implement this to add custom validation logic.
    /// </summary>
    public interface IPartyValidator
    {
        /// <summary>
        /// Validates whether a player can create a party.
        /// </summary>
        /// <param name="conn">The connection requesting party creation.</param>
        /// <param name="partyName">The desired party name.</param>
        /// <param name="maxSize">The desired maximum party size.</param>
        /// <param name="reason">Reason for rejection if validation fails.</param>
        /// <returns>True if party creation is allowed.</returns>
        bool CanCreateParty(NetworkConnection conn, string partyName, int maxSize, out string reason);

        /// <summary>
        /// Validates whether a player can join a party.
        /// </summary>
        /// <param name="conn">The connection requesting to join.</param>
        /// <param name="party">The party to join.</param>
        /// <param name="reason">Reason for rejection if validation fails.</param>
        /// <returns>True if joining is allowed.</returns>
        bool CanJoinParty(NetworkConnection conn, Party party, out string reason);

        /// <summary>
        /// Validates whether a player can be invited to a party.
        /// </summary>
        /// <param name="inviter">The connection sending the invite.</param>
        /// <param name="target">The connection being invited.</param>
        /// <param name="party">The party sending the invite.</param>
        /// <param name="reason">Reason for rejection if validation fails.</param>
        /// <returns>True if the invite is allowed.</returns>
        bool CanInviteToParty(NetworkConnection inviter, NetworkConnection target, Party party, out string reason);

        /// <summary>
        /// Validates whether a player can kick another member from the party.
        /// </summary>
        /// <param name="kicker">The connection requesting the kick.</param>
        /// <param name="target">The connection being kicked.</param>
        /// <param name="party">The party.</param>
        /// <param name="reason">Reason for rejection if validation fails.</param>
        /// <returns>True if the kick is allowed.</returns>
        bool CanKickFromParty(NetworkConnection kicker, NetworkConnection target, Party party, out string reason);
    }
}
