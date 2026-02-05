using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Types
{
    using Roles;

    /// <summary>
    /// Vote to kick a player from the room.
    /// Context.CustomData should be NetworkConnection of target player.
    /// </summary>
    public class KickPlayerVote : BaseVoteType
    {
        public override string TypeID => "kick_player";
        public override string DisplayName => "Kick Player";

        public override float WinningThreshold => 0.66f;
        public override float Cooldown => 120f;

        public override string GetQuestion(VoteContext context)
        {
            var target = context.CustomData as NetworkConnection;
            if (target == null) return "Kick player?";

            var playerName = GetPlayerName(target);
            return $"Kick player '{playerName}'?";
        }

        public override string[] GetOptions(VoteContext context)
        {
            return new[] { "Yes", "No" };
        }

        public override bool CanInitiate(NetworkConnection initiator, Room room, out string reason)
        {
            if (!base.CanInitiate(initiator, room, out reason))
                return false;

            // Cannot kick room owner
            if (room.RoleManager != null)
            {
                // We cannot fully check target here without context,
                // but we can validate at vote start time via VoteManager
            }

            return true;
        }

        public override bool CanVote(NetworkConnection voter, Room room, VoteContext context)
        {
            if (!base.CanVote(voter, room, context))
                return false;

            // Target player cannot vote on their own kick
            var target = context?.CustomData as NetworkConnection;
            if (target != null && voter == target)
                return false;

            return true;
        }

        public override void ApplyResult(VoteResult result, Room room)
        {
            // Option 0 = Yes, Option 1 = No
            if (result.WinningOption != 0) return;

            Debug.Log("[KickPlayerVote] Kick vote passed - kicking player");

            // Kick is handled externally by the game developer via the OnVoteEnded event
            // or by overriding this class. The framework provides the vote mechanism,
            // the actual kick action depends on the game's architecture.
        }

        private static string GetPlayerName(NetworkConnection conn)
        {
            return $"Player{(conn as NetworkConnectionToClient)?.connectionId}";
        }
    }
}
