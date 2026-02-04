using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Party.Validation
{
    /// <summary>
    /// Default implementation of party validation.
    /// Performs basic permission checks.
    /// </summary>
    public class DefaultPartyValidator : IPartyValidator
    {
        public bool CanCreateParty(NetworkConnection conn, string partyName, int maxSize, out string reason)
        {
            reason = null;

            if (conn == null)
            {
                reason = "Invalid connection";
                return false;
            }

            if (string.IsNullOrWhiteSpace(partyName))
            {
                reason = "Party name cannot be empty";
                return false;
            }

            if (maxSize < 2)
            {
                reason = "Party must allow at least 2 members";
                return false;
            }

            return true;
        }

        public bool CanJoinParty(NetworkConnection conn, Party party, out string reason)
        {
            reason = null;

            if (conn == null)
            {
                reason = "Invalid connection";
                return false;
            }

            if (party == null)
            {
                reason = "Party not found";
                return false;
            }

            if (party.IsMember(conn))
            {
                reason = "Already a member of this party";
                return false;
            }

            if (party.IsFull)
            {
                reason = "Party is full";
                return false;
            }

            return true;
        }

        public bool CanInviteToParty(NetworkConnection inviter, NetworkConnection target, Party party, out string reason)
        {
            reason = null;

            if (inviter == null || target == null)
            {
                reason = "Invalid connection";
                return false;
            }

            if (party == null)
            {
                reason = "Party not found";
                return false;
            }

            if (!party.IsMember(inviter))
            {
                reason = "You are not a member of this party";
                return false;
            }

            if (party.IsMember(target))
            {
                reason = "Target is already a member";
                return false;
            }

            if (party.IsFull)
            {
                reason = "Party is full";
                return false;
            }

            if (party.HasPendingInvite(target))
            {
                reason = "Target already has a pending invite";
                return false;
            }

            return true;
        }

        public bool CanKickFromParty(NetworkConnection kicker, NetworkConnection target, Party party, out string reason)
        {
            reason = null;

            if (kicker == null || target == null)
            {
                reason = "Invalid connection";
                return false;
            }

            if (party == null)
            {
                reason = "Party not found";
                return false;
            }

            if (!party.IsLeader(kicker))
            {
                reason = "Only the party leader can kick members";
                return false;
            }

            if (!party.IsMember(target))
            {
                reason = "Target is not a party member";
                return false;
            }

            if (kicker == target)
            {
                reason = "Cannot kick yourself";
                return false;
            }

            return true;
        }
    }
}
