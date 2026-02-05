using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Types
{
    using State.States;

    /// <summary>
    /// Vote to skip current round.
    /// Only available during Playing state.
    /// </summary>
    public class SkipRoundVote : BaseVoteType
    {
        public override string TypeID => "skip_round";
        public override string DisplayName => "Skip Round";

        public override float Duration => 20f;
        public override float WinningThreshold => 0.75f;

        public override string GetQuestion(VoteContext context)
        {
            return "Skip current round?";
        }

        public override string[] GetOptions(VoteContext context)
        {
            return new[] { "Yes", "No" };
        }

        public override bool CanInitiate(NetworkConnection initiator, Room room, out string reason)
        {
            if (!base.CanInitiate(initiator, room, out reason))
                return false;

            // Only during Playing state
            if (room.StateMachine != null)
            {
                if (!(room.StateMachine.CurrentState is PlayingState))
                {
                    reason = "Can only skip round during active gameplay";
                    return false;
                }
            }

            return true;
        }

        public override void ApplyResult(VoteResult result, Room room)
        {
            if (result.WinningOption != 0) return;

            Debug.Log("[SkipRoundVote] Skipping round");

            // Transition to Ended state
            room.StateMachine?.TransitionTo<EndedState>();
        }
    }
}
