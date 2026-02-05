using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Types
{
    using State.States;

    /// <summary>
    /// Vote to change the map/scene.
    /// Context.CustomData should be string[] of available maps.
    /// </summary>
    public class MapChangeVote : BaseVoteType
    {
        public override string TypeID => "map_change";
        public override string DisplayName => "Change Map";

        public override float Duration => 45f;
        public override float Cooldown => 180f;

        public override string GetQuestion(VoteContext context)
        {
            return "Which map should we play next?";
        }

        public override string[] GetOptions(VoteContext context)
        {
            if (context.CustomData is string[] maps && maps.Length > 0)
                return maps;

            return new[] { "Map1", "Map2", "Map3" };
        }

        public override bool CanInitiate(NetworkConnection initiator, Room room, out string reason)
        {
            if (!base.CanInitiate(initiator, room, out reason))
                return false;

            // Only in Lobby or Ended state
            if (room.StateMachine != null)
            {
                var state = room.StateMachine.CurrentState;
                if (!(state is LobbyState || state is EndedState))
                {
                    reason = "Can only change map in lobby or after game";
                    return false;
                }
            }

            return true;
        }

        public override void ApplyResult(VoteResult result, Room room)
        {
            Debug.Log($"[MapChangeVote] Map change vote passed: option {result.WinningOption}");

            // Map change is handled externally by the game developer via the OnVoteEnded event
            // or by overriding this class.
        }
    }
}
