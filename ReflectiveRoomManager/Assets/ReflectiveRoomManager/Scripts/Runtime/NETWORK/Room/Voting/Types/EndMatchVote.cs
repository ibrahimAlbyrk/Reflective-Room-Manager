using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Types
{
    using State.States;

    /// <summary>
    /// Vote to end the match early.
    /// Only available during Playing state.
    /// </summary>
    public class EndMatchVote : BaseVoteType
    {
        public override string TypeID => "end_match";
        public override string DisplayName => "End Match";

        public override float WinningThreshold => 0.66f;
        public override float Cooldown => 120f;

        public override string GetQuestion(VoteContext context)
        {
            return "End match early?";
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
                    reason = "Can only end match during active gameplay";
                    return false;
                }
            }

            return true;
        }

        public override void ApplyResult(VoteResult result, Room room)
        {
            if (result.WinningOption != 0) return;

            Debug.Log("[EndMatchVote] Ending match early");

            // Transition to Ended state
            room.StateMachine?.TransitionTo<EndedState>();
        }
    }
}
