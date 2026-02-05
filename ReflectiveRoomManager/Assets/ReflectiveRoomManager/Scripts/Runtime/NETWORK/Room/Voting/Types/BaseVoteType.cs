using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting.Types
{
    using Roles;

    /// <summary>
    /// Abstract base class for vote types.
    /// Provides default implementations.
    /// </summary>
    public abstract class BaseVoteType : IVoteType
    {
        public abstract string TypeID { get; }
        public abstract string DisplayName { get; }

        // Default configuration (can be overridden)
        public virtual float Duration => 30f;
        public virtual float MinParticipationRate => 0.5f;
        public virtual float WinningThreshold => 0.51f;
        public virtual TieResolutionMode TieResolution => TieResolutionMode.Fail;
        public virtual bool AllowVoteChange => true;
        public virtual bool AllowSpectatorVote => false;
        public virtual float Cooldown => 60f;

        public abstract string GetQuestion(VoteContext context);
        public abstract string[] GetOptions(VoteContext context);

        /// <summary>
        /// Default: Anyone in the room can initiate.
        /// </summary>
        public virtual bool CanInitiate(NetworkConnection initiator, Room room, out string reason)
        {
            reason = null;

            if (!room.Connections.Contains(initiator))
            {
                reason = "Not in room";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Default: All room members can vote (spectators excluded if configured).
        /// </summary>
        public virtual bool CanVote(NetworkConnection voter, Room room, VoteContext context)
        {
            if (!room.Connections.Contains(voter))
                return false;

            // Check spectator voting permission
            if (!AllowSpectatorVote && room.RoleManager != null)
            {
                var role = room.RoleManager.GetPlayerRole(voter);
                if (role == RoomRole.Guest)
                    return false;
            }

            return true;
        }

        public virtual void OnVoteStarted(ActiveVote vote, Room room)
        {
            // Default: No action
        }

        public virtual void OnVoteEnded(VoteResult result, Room room)
        {
            // Default: No action
        }

        public abstract void ApplyResult(VoteResult result, Room room);
    }
}
