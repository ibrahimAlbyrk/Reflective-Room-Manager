using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Types
{
    using State.States;

    /// <summary>
    /// Vote to restart the match from beginning.
    /// </summary>
    public class RestartMatchVote : BaseVoteType
    {
        public override string TypeID => "restart_match";
        public override string DisplayName => "Restart Match";

        public override float WinningThreshold => 0.75f;
        public override float Cooldown => 90f;

        public override string GetQuestion(VoteContext context)
        {
            return "Restart match from beginning?";
        }

        public override string[] GetOptions(VoteContext context)
        {
            return new[] { "Yes", "No" };
        }

        public override void ApplyResult(VoteResult result, Room room)
        {
            if (result.WinningOption != 0) return;

            Debug.Log("[RestartMatchVote] Restarting match");

            // Transition to Lobby state (which will restart)
            room.StateMachine?.TransitionTo<LobbyState>();
        }
    }
}
